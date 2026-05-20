using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace SpiceSharpParserAIExamples;

internal static class AcceptedExampleFixture
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly Lazy<IReadOnlyList<AcceptedExampleCase>> LazyCases = new(LoadCases);
    private static readonly Lazy<IReadOnlyDictionary<string, AcceptedExampleCase>> LazyCasesById =
        new(() => LazyCases.Value.ToDictionary(item => item.Id, StringComparer.Ordinal));
    private static readonly Lazy<AcceptedExamplesManifest> LazyManifest = new(LoadManifest);

    public static IReadOnlyList<AcceptedExampleCase> All => LazyCases.Value;

    public static AcceptedExamplesManifest Manifest => LazyManifest.Value;

    public static AcceptedExampleCase Get(string id)
    {
        Assert.True(LazyCasesById.Value.TryGetValue(id, out var testCase), $"fixture case {id} was not found");
        return testCase;
    }
    public static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static IReadOnlyList<AcceptedExampleCase> LoadCases()
    {
        var path = FixturePath("accepted_examples_fixture.jsonl");
        var cases = new List<AcceptedExampleCase>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            cases.Add(JsonSerializer.Deserialize<AcceptedExampleCase>(line, JsonOptions)
                ?? throw new InvalidOperationException($"Could not deserialize fixture line from {path}"));
        }

        return cases;
    }

    private static AcceptedExamplesManifest LoadManifest()
    {
        var path = FixturePath("accepted_examples_manifest.json");
        return JsonSerializer.Deserialize<AcceptedExamplesManifest>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException($"Could not deserialize manifest {path}");
    }

    private static string FixturePath(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Generated fixture file is missing: {path}", path);
        }

        return path;
    }
}
