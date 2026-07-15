using System.Security.Cryptography;
using System.Text;
using SpiceSharp.Simulations;
using SpiceSharpParser;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParserAIExamples;

public sealed partial class AcceptedExampleTests
{
    private static readonly Lazy<FixtureIntegritySnapshot> Integrity = new(BuildIntegritySnapshot);

    private static AcceptedExampleCase RunAcceptedExample(string id, string netlist)
    {
        var testCase = AcceptedExampleFixture.Get(id);
        var normalizedNetlist = NormalizeInlineNetlist(netlist);
        Assert.Equal(testCase.NetlistHash, ShortHash(normalizedNetlist));
        AssertAcceptedExampleIntegrity(testCase, Integrity.Value);
        return ExecuteAcceptedExample(testCase, normalizedNetlist);
    }

    private static AcceptedExampleCase ExecuteAcceptedExample(AcceptedExampleCase testCase, string netlist)
    {
        var parser = new SpiceNetlistParser();
        var parseResult = parser.ParseNetlist(netlist);
        var reader = new SpiceNetlistReader(new SpiceNetlistReaderSettings());
        var model = reader.Read(parseResult.FinalModel);

        Assert.False(
            model.ValidationResult.HasError,
            $"{CaseLabel(testCase)}: parser/reader validation failed: {string.Join(Environment.NewLine, model.ValidationResult.Select(item => item.Message))}");

        foreach (var simulation in model.Simulations)
        {
            simulation.InvokeEvents(simulation.Run(model.Circuit, -1)).ToArray();
        }

        var measurements = model.Measurements
            .SelectMany(entry => entry.Value.Select((measurement, index) => new MeasurementReport
            {
                Name = measurement.Name,
                Key = entry.Key,
                Index = index,
                Value = measurement.Value,
                ValueIsFinite = double.IsFinite(measurement.Value),
                Success = measurement.Success,
                MeasurementType = measurement.MeasurementType,
                SimulationName = measurement.SimulationName,
            }))
            .ToList();

        return new AcceptedExampleCase
        {
            Id = testCase.Id,
            Source = testCase.Source,
            ExampleId = testCase.ExampleId,
            TestName = testCase.TestName,
            NetlistHash = testCase.NetlistHash,
            GeneratorModel = testCase.GeneratorModel,
            GeneratorSource = testCase.GeneratorSource,
            QualityTier = testCase.QualityTier,
            Status = testCase.Status,
            PytestStatus = testCase.PytestStatus,
            PromptCount = testCase.PromptCount,
            RepresentativePrompt = testCase.RepresentativePrompt,
            PairIds = testCase.PairIds,
            Prompts = testCase.Prompts,
            Measurements = measurements,
        };
    }

    private static string NormalizeInlineNetlist(string netlist) =>
        netlist.Replace("\r\n", "\n").Replace('\r', '\n').Trim() + "\n";

    private static string ShortHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()[..16];
    }

    private static void AssertAcceptedExampleIntegrity(AcceptedExampleCase testCase, FixtureIntegritySnapshot snapshot)
    {
        Assert.Contains(testCase.Id, snapshot.CaseIds);
        Assert.Equal("gold", testCase.QualityTier);
        Assert.Equal("ok", testCase.Status);
        Assert.Equal("ok", testCase.PytestStatus);
        Assert.True(testCase.PromptCount > 0, $"{testCase.Id} has no prompts");
        Assert.Equal(testCase.PromptCount, testCase.Prompts.Count);
        Assert.Equal(testCase.PromptCount, testCase.PairIds.Count);
        Assert.NotEmpty(testCase.Measurements);
        Assert.True(snapshot.SourceCounts.ContainsKey(testCase.Source), $"{testCase.Id} has unknown source {testCase.Source}");
    }

    private static void AssertAllMeasurementsSuccessful(AcceptedExampleCase result)
    {
        Assert.True(result.Measurements.Count > 0, $"{CaseLabel(result)}: expected at least one measurement");
        var failed = result.Measurements
            .Where(item => !item.Success || !item.ValueIsFinite || item.Value is null || !double.IsFinite(item.Value.Value))
            .Select(item => item.Name)
            .ToArray();
        Assert.True(failed.Length == 0, $"{CaseLabel(result)}: failed/non-finite measurements: {string.Join(", ", failed)}");
    }

    private static void AssertHasSuccessfulMeasurement(AcceptedExampleCase result, string? name)
    {
        var found = result.Measurements.Any(item =>
            (name is null || string.Equals(MeasurementName(item), name, StringComparison.OrdinalIgnoreCase))
            && item.Success
            && item.ValueIsFinite
            && item.Value is not null
            && double.IsFinite(item.Value.Value));
        Assert.True(found, $"{CaseLabel(result)}: expected a successful measurement named {name ?? "<any>"}");
    }

    private static void AssertMeasurementCount(AcceptedExampleCase result, string name, int expected)
    {
        var count = result.Measurements.Count(item =>
            string.Equals(MeasurementName(item), name, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(expected, count);
    }

    private static void AssertMeasurementBetween(AcceptedExampleCase result, string name, double low, double high, int index = 0)
    {
        var value = MeasurementValue(result, name, index);
        Assert.True(low <= value && value <= high, $"{CaseLabel(result)}: {name}={value} is outside [{low}, {high}]");
    }

    private static void AssertMeasurementGreater(AcceptedExampleCase result, string name, double threshold, int index = 0)
    {
        var value = MeasurementValue(result, name, index);
        Assert.True(value > threshold, $"{CaseLabel(result)}: {name}={value} is not greater than {threshold}");
    }

    private static void AssertMeasurementLess(AcceptedExampleCase result, string name, double threshold, int index = 0)
    {
        var value = MeasurementValue(result, name, index);
        Assert.True(value < threshold, $"{CaseLabel(result)}: {name}={value} is not less than {threshold}");
    }

    private static void AssertMeasurementNear(AcceptedExampleCase result, string name, double expected, double tolerance, int index = 0)
    {
        var value = MeasurementValue(result, name, index);
        Assert.True(Math.Abs(value - expected) <= tolerance, $"{CaseLabel(result)}: {name}={value} differs from {expected} by more than {tolerance}");
    }

    private static void AssertMeasurementRatioBetween(
        AcceptedExampleCase result,
        string numerator,
        string denominator,
        double low,
        double high,
        int numeratorIndex = 0,
        int denominatorIndex = 0)
    {
        var denominatorValue = MeasurementValue(result, denominator, denominatorIndex);
        Assert.NotEqual(0.0, denominatorValue);
        var ratio = MeasurementValue(result, numerator, numeratorIndex) / denominatorValue;
        Assert.True(low <= ratio && ratio <= high, $"{CaseLabel(result)}: {numerator}/{denominator}={ratio} is outside [{low}, {high}]");
    }

    private static MeasurementReport RequireMeasurement(AcceptedExampleCase result, string name, int index = 0, bool successful = true)
    {
        var found = result.Measurements
            .Where(item => string.Equals(MeasurementName(item), name, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.True(index < found.Length, $"{CaseLabel(result)}: measurement {name} index {index} not found");
        var measurement = found[index];
        if (successful)
        {
            Assert.True(
                measurement.Success && measurement.ValueIsFinite && measurement.Value is not null && double.IsFinite(measurement.Value.Value),
                $"{CaseLabel(result)}: measurement {name} did not succeed with a finite value");
        }

        return measurement;
    }

    private static double MeasurementValue(AcceptedExampleCase result, string name, int index = 0) =>
        MeasurementNumericValue(RequireMeasurement(result, name, index));

    private static IReadOnlyDictionary<string, double> MeasurementsDictionary(AcceptedExampleCase result) =>
        result.Measurements.ToDictionary(
            item => item.Name,
            item => MeasurementNumericValue(item),
            StringComparer.OrdinalIgnoreCase);

    private static double MeasurementNumericValue(MeasurementReport measurement)
    {
        Assert.True(measurement.Value is not null, $"measurement {measurement.Name} has no value");
        return measurement.Value.Value;
    }

    private static string MeasurementName(MeasurementReport item) =>
        string.IsNullOrWhiteSpace(item.Name) ? item.Key : item.Name;

    private static bool IsTruthy(object? value) =>
        value switch
        {
            null => false,
            bool boolean => boolean,
            double number => number != 0,
            int number => number != 0,
            string text => text.Length > 0,
            System.Collections.IEnumerable items => items.Cast<object?>().Any(),
            _ => true,
        };

    private static double ToDouble(object? value)
    {
        return value switch
        {
            double number => number,
            int number => number,
            long number => number,
            float number => number,
            decimal number => (double)number,
            bool boolean => boolean ? 1.0 : 0.0,
            string text when double.TryParse(text, out var parsed) => parsed,
            _ => throw new InvalidOperationException($"value {value} is not numeric"),
        };
    }

    private static int Length(object? value) =>
        value switch
        {
            string text => text.Length,
            System.Collections.ICollection items => items.Count,
            _ => throw new InvalidOperationException($"value {value} has no length"),
        };

    private static string CaseLabel(AcceptedExampleCase result) => $"{result.Id} ({result.ExampleId})";

    private static FixtureIntegritySnapshot BuildIntegritySnapshot()
    {
        var cases = AcceptedExampleFixture.All;
        var manifest = AcceptedExampleFixture.Manifest;

        Assert.Equal(948, cases.Count);
        Assert.Equal(manifest.TotalUniqueCases, cases.Count);
        Assert.Equal(0, manifest.UnmatchedAcceptedPairHashes);
        Assert.Equal(cases.Count, cases.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(cases.Count, cases.Select(item => item.NetlistHash).Distinct(StringComparer.Ordinal).Count());

        var sourceCounts = cases
            .GroupBy(item => item.Source, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        foreach (var source in manifest.Sources)
        {
            Assert.True(sourceCounts.TryGetValue(source.Name, out var sourceCaseCount), $"manifest source {source.Name} has no generated cases");
            Assert.Equal(source.UniqueNetlistCount, sourceCaseCount);
        }

        return new FixtureIntegritySnapshot(
            cases.Select(item => item.Id).ToHashSet(StringComparer.Ordinal),
            sourceCounts);
    }

    private sealed record FixtureIntegritySnapshot(
        IReadOnlySet<string> CaseIds,
        IReadOnlyDictionary<string, int> SourceCounts);
}
