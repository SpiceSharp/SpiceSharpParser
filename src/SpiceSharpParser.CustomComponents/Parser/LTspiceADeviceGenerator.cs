using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.CustomComponents.Analog;
using SpiceSharpParser.CustomComponents.Digital;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Expands supported LTspice A-device instances into portable subcircuits.
    /// </summary>
    public sealed class LTspiceADeviceGenerator : IComponentGenerator
    {
        private static readonly IReadOnlyDictionary<string, string> SetResetParameterMap =
            CreateParameterMap(
                ("vhigh", null),
                ("vlow", null),
                ("ref", null),
                ("td", "TPD"),
                ("rout", "ROUT"),
                ("ic", "IC"));

        private static readonly IReadOnlyDictionary<string, string> DFlipFlopParameterMap =
            CreateParameterMap(
                ("vhigh", null),
                ("vlow", null),
                ("ref", null),
                ("td", "TPD"),
                ("rout", "ROUT"),
                ("ic", "IC"));

        private static readonly IReadOnlyDictionary<string, string> PhaseDetectorParameterMap =
            CreateParameterMap(
                ("ref", "REF"),
                ("iout", "IOUT"),
                ("vhigh", "VHIGH"),
                ("vlow", "VLOW"),
                ("rout", "ROUT"),
                ("rclamp", "RCLAMP"),
                ("cout", "COUT"));

        private static readonly IReadOnlyDictionary<string, string> CounterParameterMap =
            CreateParameterMap(
                ("vhigh", null),
                ("vlow", null),
                ("ref", null),
                ("cycles", "CYCLES"),
                ("duty", "DUTY"),
                ("rout", "ROUT"));

        private static readonly IReadOnlyDictionary<string, string> SampleHoldParameterMap =
            CreateParameterMap(
                ("ref", "REF"),
                ("vhigh", "VHIGH"),
                ("vlow", "VLOW"),
                ("td", "TPD"),
                ("rout", "ROUT"));

        private static readonly IReadOnlyDictionary<string, string> OtaParameterMap =
            CreateParameterMap(
                ("g", "G"),
                ("ref", "REF"),
                ("iout", "IOUT"),
                ("isrc", "ISRC"),
                ("isink", "ISINK"),
                ("ioffset", "IOFFSET"),
                ("powerup", "POWERUP"),
                ("asym", "ASYM"),
                ("linear", "LINEAR"),
                ("rout", "ROUT"),
                ("vhigh", "VHIGH"),
                ("vlow", "VLOW"),
                ("rclamp", "RCLAMP"),
                ("cout", "COUT"));

        private static readonly IReadOnlyDictionary<string, string> VaristorParameterMap =
            CreateParameterMap(
                ("rclamp", "RCLAMP"),
                ("roff", "ROFF"));

        private static readonly IReadOnlyDictionary<string, string> ModulatorParameterMap =
            CreateParameterMap(
                ("mark", "MARK"),
                ("space", "SPACE"),
                ("rout", "ROUT"));

        private readonly DigitalSubcircuitLibrary _digital;
        private readonly DigitalSubcircuitLibrary _zeroDelayDigital;
        private readonly AnalogSubcircuitLibrary _analog;

        /// <summary>
        /// Initializes a new instance of the <see cref="LTspiceADeviceGenerator"/> class.
        /// </summary>
        public LTspiceADeviceGenerator()
        {
            _digital = DigitalSubcircuitLibrary.LoadBuiltIn();
            _zeroDelayDigital = DigitalSubcircuitLibrary.LoadADeviceZeroDelay();
            _analog = AnalogSubcircuitLibrary.LoadBuiltIn();
        }

        /// <inheritdoc />
        public IEntity Generate(
            string componentIdentifier,
            string originalName,
            string type,
            ParameterCollection parameters,
            IReadingContext context)
        {
            if (!LTspiceADeviceInstanceReader.TryRead(
                    originalName,
                    parameters,
                    context,
                    out LTspiceADeviceInstance instance))
            {
                return null;
            }

            string[] terminals = instance.Terminals;
            string model = instance.Model;
            IReadOnlyDictionary<string, ADeviceParameter> instanceParameters = instance.Parameters;

            if (!LTspiceADeviceValidator.TryValidate(
                    originalName,
                    model,
                    instanceParameters,
                    context))
            {
                return null;
            }

            string portableInstanceName = CreatePortableInstanceName(componentIdentifier, context);
            StringComparison nodeComparison =
                context.ReaderSettings.CaseSensitivity.IsNodeNameCaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;
            bool[] unusedTerminals = terminals
                .Select(terminal => terminal.Equals(
                    terminals[LTspiceADeviceInstance.TerminalCount - 1],
                    nodeComparison))
                .ToArray();
            ApplyUnusedTerminalSemantics(
                model,
                portableInstanceName,
                terminals,
                unusedTerminals);
            List<IEntity> originalEntities = context.ContextEntities.ToList();
            try
            {
                switch (model)
                {
                    case "SRFLOP":
                        AddSetResetFlipFlop(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "DFLOP":
                        AddDFlipFlop(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "PHASEDET":
                        AddPhaseDetector(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "COUNTER":
                        AddCounter(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "SAMPLEHOLD":
                        AddSampleHold(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "OTA":
                        AddOta(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "VARISTOR":
                        AddVaristor(context, originalName, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "MODULATE":
                    case "MODULATOR":
                        AddModulator(
                            context,
                            originalName,
                            portableInstanceName,
                            terminals,
                            unusedTerminals[1],
                            instanceParameters);
                        break;
                    default:
                        AddError(
                            context,
                            $"Unsupported LTspice A-device model '{model}' on component '{originalName}'. "
                            + "Supported models are SRFLOP, DFLOP, PHASEDET, COUNTER, SAMPLEHOLD, "
                            + "OTA, VARISTOR, MODULATE, and MODULATOR.",
                            instance.ModelLineInfo);
                        break;
                }
            }
            catch (Exception exception) when (
                exception is ArgumentException
                || exception is InvalidOperationException
                || exception is SpiceSubcircuitLibraryException)
            {
                RollBackExpansion(context, originalEntities);
                AddError(
                    context,
                    $"Could not expand LTspice A-device '{originalName}' ({model}): {exception.Message}",
                    parameters.LineInfo);
            }

            return null;
        }

        private void AddSetResetFlipFlop(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, SetResetParameterMap, context, out Dictionary<string, string> mapped)
                || !TryCreateDigitalRails(
                    context,
                    originalName,
                    instanceName,
                    terminals[7],
                    parameters,
                    mapped,
                    out string highNode,
                    out string lowNode))
            {
                return;
            }

            bool usesZeroDelay = UsesZeroDelay(mapped);
            if (usesZeroDelay)
            {
                mapped.Remove("TPD");
            }

            SetCompatibilityDefaults(mapped);
            (usesZeroDelay ? _zeroDelayDigital : _digital).Library.AddInstance(
                context.ContextEntities,
                usesZeroDelay ? "DIG_SR_LATCH_ZERO_DELAY" : "DIG_SR_LATCH",
                instanceName,
                new[]
                {
                    terminals[0],
                    terminals[1],
                    terminals[6],
                    terminals[5],
                    highNode,
                    lowNode,
                },
                mapped);
        }

        private void AddDFlipFlop(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, DFlipFlopParameterMap, context, out Dictionary<string, string> mapped)
                || !TryCreateDigitalRails(
                    context,
                    originalName,
                    instanceName,
                    terminals[7],
                    parameters,
                    mapped,
                    out string highNode,
                    out string lowNode))
            {
                return;
            }

            bool usesZeroDelay = UsesZeroDelay(mapped);
            if (usesZeroDelay)
            {
                mapped.Remove("TPD");
            }

            SetCompatibilityDefaults(mapped);
            (usesZeroDelay ? _zeroDelayDigital : _digital).Library.AddInstance(
                context.ContextEntities,
                usesZeroDelay ? "DIG_DFF_ZERO_DELAY" : "DIG_DFF",
                instanceName,
                new[]
                {
                    terminals[0],
                    terminals[2],
                    terminals[3],
                    terminals[4],
                    terminals[6],
                    terminals[5],
                    highNode,
                    lowNode,
                },
                mapped);
        }

        private void AddPhaseDetector(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, PhaseDetectorParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            _digital.Library.AddInstance(
                context.ContextEntities,
                "DIG_PHASE_DETECTOR",
                instanceName,
                new[] { terminals[0], terminals[1], terminals[6], terminals[7] },
                mapped);
        }

        private void AddCounter(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, CounterParameterMap, context, out Dictionary<string, string> mapped)
                || !TryCreateDigitalRails(
                    context,
                    originalName,
                    instanceName,
                    terminals[7],
                    parameters,
                    mapped,
                    out string highNode,
                    out string lowNode))
            {
                return;
            }

            SetCompatibilityDefaults(mapped);
            _digital.Library.AddInstance(
                context.ContextEntities,
                "DIG_COUNTER",
                instanceName,
                new[]
                {
                    terminals[0],
                    terminals[1],
                    terminals[6],
                    terminals[5],
                    highNode,
                    lowNode,
                },
                mapped);
        }

        private void AddSampleHold(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, SampleHoldParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            _analog.Library.AddInstance(
                context.ContextEntities,
                "ANALOG_SAMPLE_HOLD",
                instanceName,
                new[]
                {
                    terminals[0],
                    terminals[1],
                    terminals[2],
                    terminals[3],
                    terminals[6],
                    terminals[7],
                },
                mapped);
        }

        private void AddOta(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, OtaParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            if (mapped.TryGetValue("IOUT", out string outputLimit))
            {
                if (!mapped.ContainsKey("ISRC"))
                {
                    mapped["ISRC"] = outputLimit;
                }

                if (!mapped.ContainsKey("ISINK"))
                {
                    mapped["ISINK"] = (-double.Parse(
                        outputLimit,
                        CultureInfo.InvariantCulture)).ToString("R", CultureInfo.InvariantCulture);
                }
            }

            _analog.Library.AddInstance(
                context.ContextEntities,
                "ANALOG_OTA",
                instanceName,
                new[]
                {
                    terminals[0],
                    terminals[1],
                    terminals[2],
                    terminals[3],
                    terminals[5],
                    terminals[6],
                    terminals[7],
                },
                mapped);
        }

        private void AddVaristor(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, VaristorParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            _analog.Library.AddInstance(
                context.ContextEntities,
                "ANALOG_VARISTOR",
                instanceName,
                new[] { terminals[0], terminals[1], terminals[6], terminals[7] },
                mapped);
        }

        private void AddModulator(
            IReadingContext context,
            string originalName,
            string instanceName,
            string[] terminals,
            bool amplitudeTerminalIsUnused,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(originalName, parameters, ModulatorParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            if (amplitudeTerminalIsUnused)
            {
                mapped["AMDEFAULT"] = "1";
            }

            _analog.Library.AddInstance(
                context.ContextEntities,
                "ANALOG_MODULATOR",
                instanceName,
                new[] { terminals[0], terminals[1], terminals[6], terminals[7] },
                mapped);
        }

        private static void ApplyUnusedTerminalSemantics(
            string model,
            string instanceName,
            string[] terminals,
            IReadOnlyList<bool> unusedTerminals)
        {
            // Native A-devices treat a terminal connected to terminal 8 (common)
            // as unused. Inputs can remain connected to common, but unused outputs
            // must be detached from the portable model's active output stages.
            foreach (int outputIndex in GetOutputTerminalIndexes(model))
            {
                if (unusedTerminals[outputIndex])
                {
                    terminals[outputIndex] = instanceName
                        + ".__a_nc"
                        + (outputIndex + 1).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private static IEnumerable<int> GetOutputTerminalIndexes(string model)
        {
            switch (model)
            {
                case "SRFLOP":
                case "DFLOP":
                case "COUNTER":
                case "OTA":
                    return new[] { 5, 6 };
                case "PHASEDET":
                case "SAMPLEHOLD":
                case "VARISTOR":
                case "MODULATE":
                case "MODULATOR":
                    return new[] { 6 };
                default:
                    return Array.Empty<int>();
            }
        }

        private static bool TryCreateDigitalRails(
            IReadingContext context,
            string originalName,
            string instanceName,
            string commonNode,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IDictionary<string, string> mapped,
            out string highNode,
            out string lowNode)
        {
            highNode = instanceName + ".__a_vhigh";
            lowNode = instanceName + ".__a_vlow";
            if (!TryEvaluate(parameters, "vhigh", originalName, context, 1.0, out double high)
                || !TryEvaluate(parameters, "vlow", originalName, context, 0.0, out double low))
            {
                return false;
            }

            if (high <= low)
            {
                AddError(
                    context,
                    $"LTspice A-device '{originalName}' Vhigh ({high}) must be greater than Vlow ({low}).",
                    parameters.TryGetValue("vhigh", out ADeviceParameter highParameter)
                        ? highParameter.LineInfo
                        : null);
                return false;
            }

            if (parameters.ContainsKey("ref"))
            {
                if (!TryEvaluate(parameters, "ref", originalName, context, 0.0, out double reference))
                {
                    return false;
                }

                mapped["VTH"] = ((reference - low) / (high - low)).ToString(
                    "R",
                    CultureInfo.InvariantCulture);
            }

            string highSourceName = "V" + instanceName + ".__a_vhigh";
            string lowSourceName = "V" + instanceName + ".__a_vlow";
            if (context.ContextEntities.Contains(highSourceName)
                || context.ContextEntities.Contains(lowSourceName))
            {
                AddError(
                    context,
                    $"Internal rail entities for LTspice A-device '{instanceName}' already exist.",
                    null);
                return false;
            }

            context.ContextEntities.Add(
                new VoltageSource(highSourceName, highNode, commonNode, high));
            context.ContextEntities.Add(
                new VoltageSource(lowSourceName, lowNode, commonNode, low));
            return true;
        }

        private static bool TryMapParameters(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> source,
            IReadOnlyDictionary<string, string> map,
            IReadingContext context,
            out Dictionary<string, string> result)
        {
            return LTspiceADeviceParameterEvaluator.TryMap(
                componentName,
                source,
                map,
                context,
                out result);
        }

        private static bool TryEvaluate(
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string name,
            string componentName,
            IReadingContext context,
            double defaultValue,
            out double result)
        {
            return LTspiceADeviceParameterEvaluator.TryEvaluate(
                parameters,
                name,
                componentName,
                context,
                defaultValue,
                out result);
        }

        private static string CreatePortableInstanceName(
            string componentIdentifier,
            IReadingContext context)
        {
            string preferredName = "X" + componentIdentifier;
            if (!HasGeneratedNameCollision(context, preferredName))
            {
                return preferredName;
            }

            string baseName = "X__a_" + componentIdentifier;
            string candidate = baseName;
            int suffix = 1;
            while (HasGeneratedNameCollision(context, candidate))
            {
                candidate = baseName + "_" + suffix.ToString(CultureInfo.InvariantCulture);
                suffix++;
            }

            return candidate;
        }

        private static bool HasGeneratedNameCollision(IReadingContext context, string instanceName)
        {
            StringComparison comparison = context.ReaderSettings.CaseSensitivity.IsEntityNamesCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            string childPrefix = instanceName + context.NameGenerator.Separator;
            return context.ContextEntities.Contains(instanceName)
                || context.ContextEntities.Contains("V" + instanceName + ".__a_vhigh")
                || context.ContextEntities.Contains("V" + instanceName + ".__a_vlow")
                || context.ContextEntities.Any(
                    entity => entity.Name.StartsWith(childPrefix, comparison));
        }

        private static void RollBackExpansion(
            IReadingContext context,
            IReadOnlyCollection<IEntity> originalEntities)
        {
            List<IEntity> added = context.ContextEntities
                .Where(entity => !originalEntities.Any(
                    original => ReferenceEquals(original, entity)))
                .ToList();
            for (int index = added.Count - 1; index >= 0; index--)
            {
                context.ContextEntities.Remove(added[index].Name);
            }
        }

        private static bool UsesZeroDelay(IReadOnlyDictionary<string, string> parameters)
        {
            return !parameters.TryGetValue("TPD", out string delay)
                || (double.TryParse(
                        delay,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double delayValue)
                    && delayValue == 0.0);
        }

        private static void SetCompatibilityDefaults(IDictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("ROUT"))
            {
                parameters["ROUT"] = "1";
            }

            parameters["COUT"] = "1f";
        }

        private static IReadOnlyDictionary<string, string> CreateParameterMap(
            params (string NativeName, string PortableName)[] items)
        {
            return items.ToDictionary(
                item => item.NativeName,
                item => item.PortableName,
                StringComparer.OrdinalIgnoreCase);
        }

        private static void AddError(
            IReadingContext context,
            string message,
            SpiceLineInfo lineInfo)
        {
            LTspiceADeviceDiagnostics.AddError(context, message, lineInfo);
        }
    }
}
