using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
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
        private const int TerminalCount = 8;
        private const int ModelIndex = TerminalCount;
        private const int RequiredParameterCount = TerminalCount + 1;

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
            if (!TryReadInstance(
                    originalName,
                    parameters,
                    context,
                    out string[] terminals,
                    out string model,
                    out IReadOnlyDictionary<string, ADeviceParameter> instanceParameters))
            {
                return null;
            }

            if (!TryValidateModelParameters(
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
                .Select(terminal => terminal.Equals(terminals[TerminalCount - 1], nodeComparison))
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
                            parameters[ModelIndex].LineInfo);
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

        private static bool TryReadInstance(
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            out string[] terminals,
            out string model,
            out IReadOnlyDictionary<string, ADeviceParameter> instanceParameters)
        {
            terminals = null;
            model = null;
            instanceParameters = null;

            if (parameters.Count < RequiredParameterCount)
            {
                AddError(
                    context,
                    $"LTspice A-device '{originalName}' expects eight terminals followed by a model name.",
                    parameters.LineInfo);
                return false;
            }

            for (int index = 0; index < RequiredParameterCount; index++)
            {
                if (!(parameters[index] is SingleParameter))
                {
                    string position = index < TerminalCount
                        ? $"terminal {index + 1}"
                        : "model name";
                    AddError(
                        context,
                        $"LTspice A-device '{originalName}' has an invalid {position}.",
                        parameters[index].LineInfo);
                    return false;
                }
            }

            terminals = new string[TerminalCount];
            for (int index = 0; index < terminals.Length; index++)
            {
                string terminal = parameters[index].Value;
                terminals[index] = context.ReaderSettings.ExpandSubcircuits
                    ? context.NameGenerator.GenerateNodeName(terminal)
                    : terminal;
            }

            model = parameters[ModelIndex].Value.ToUpperInvariant();
            var parsed = new Dictionary<string, ADeviceParameter>(StringComparer.OrdinalIgnoreCase);
            for (int index = RequiredParameterCount; index < parameters.Count; index++)
            {
                Parameter parameter = parameters[index];
                string name;
                string expression;
                if (parameter is AssignmentParameter assignment)
                {
                    name = assignment.Name;
                    expression = assignment.Value;
                }
                else if (parameter is SingleParameter flag)
                {
                    name = flag.Value;
                    expression = "1";

                    if (!IsAllowedFlag(model, name))
                    {
                        AddError(
                            context,
                            $"Unknown bare flag '{name}' on LTspice A-device '{originalName}' ({model}). "
                            + "Only OTA Linear and Asym may use bare flag syntax.",
                            parameter.LineInfo);
                        return false;
                    }
                }
                else
                {
                    AddError(
                        context,
                        $"Unsupported parameter syntax '{parameter}' on LTspice A-device '{originalName}'.",
                        parameter.LineInfo);
                    return false;
                }

                if (parsed.ContainsKey(name))
                {
                    AddError(
                        context,
                        $"Duplicate LTspice A-device parameter '{name}' on component '{originalName}'.",
                        parameter.LineInfo);
                    return false;
                }

                parsed.Add(name, new ADeviceParameter(expression, parameter.LineInfo));
            }

            instanceParameters = parsed;
            return true;
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
            result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool success = true;
            foreach (KeyValuePair<string, ADeviceParameter> item in source)
            {
                if (!map.TryGetValue(item.Key, out string portableName))
                {
                    AddError(
                        context,
                        $"Unsupported LTspice A-device parameter '{item.Key}' on component '{componentName}'.",
                        item.Value.LineInfo);
                    success = false;
                    continue;
                }

                if (portableName == null)
                {
                    continue;
                }

                if (TryEvaluateExpression(
                        componentName,
                        item.Key,
                        item.Value,
                        context,
                        out double value))
                {
                    result[portableName] = value.ToString("R", CultureInfo.InvariantCulture);
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }

        private static bool TryEvaluate(
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string name,
            string componentName,
            IReadingContext context,
            double defaultValue,
            out double result)
        {
            if (!parameters.TryGetValue(name, out ADeviceParameter parameter))
            {
                result = defaultValue;
                return true;
            }

            return TryEvaluateExpression(
                componentName,
                name,
                parameter,
                context,
                out result);
        }

        private static bool TryEvaluateExpression(
            string componentName,
            string parameterName,
            ADeviceParameter parameter,
            IReadingContext context,
            out double result)
        {
            if (TryFindSteppedParameter(
                    parameter.Expression,
                    context,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out string steppedParameter))
            {
                AddError(
                    context,
                    $"LTspice A-device '{componentName}' parameter '{parameterName}' depends on "
                    + $"stepped parameter '{steppedParameter}'. Sweep-dependent A-device parameters "
                    + "are not supported because expanding them would freeze the first sweep value.",
                    parameter.LineInfo);
                result = double.NaN;
                return false;
            }

            try
            {
                result = context.Evaluator.EvaluateDouble(parameter.Expression);
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    AddError(
                        context,
                        $"LTspice A-device '{componentName}' parameter '{parameterName}' must be finite.",
                        parameter.LineInfo);
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                AddError(
                    context,
                    $"Could not evaluate LTspice A-device '{componentName}' parameter "
                    + $"'{parameterName}': {exception.Message}",
                    parameter.LineInfo);
                result = double.NaN;
                return false;
            }
        }

        private static bool TryFindSteppedParameter(
            string expression,
            IReadingContext context,
            ISet<string> visited,
            out string steppedParameter)
        {
            steppedParameter = null;
            foreach (string dependency in context.EvaluationContext.GetExpressionParameters(expression, false))
            {
                if (context.SimulationConfiguration.RegisteredParameterSweeps.Contains(dependency))
                {
                    steppedParameter = dependency;
                    return true;
                }

                if (visited.Add(dependency)
                    && context.EvaluationContext.Parameters.TryGetValue(dependency, out var parameter)
                    && TryFindSteppedParameter(
                        parameter.ValueExpression,
                        context,
                        visited,
                        out steppedParameter))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryValidateModelParameters(
            string componentName,
            string model,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IReadingContext context)
        {
            bool success = true;
            switch (model)
            {
                case "SRFLOP":
                case "DFLOP":
                    success &= TryValidateOptional(
                        componentName, parameters, "td", context, value => value >= 0.0, "zero or greater");
                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "ic", context, value => value >= 0.0 && value <= 1.0, "between zero and one");
                    break;
                case "PHASEDET":
                    success &= TryValidateOptional(
                        componentName, parameters, "iout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "rclamp", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "cout", context, value => value >= 0.0, "zero or greater");
                    success &= TryValidateVoltageWindow(componentName, parameters, context, 10.0, -10.0);
                    break;
                case "COUNTER":
                    if (!parameters.ContainsKey("cycles"))
                    {
                        AddError(
                            context,
                            $"LTspice A-device '{componentName}' (COUNTER) requires the Cycles parameter.",
                            null);
                        success = false;
                    }
                    else if (TryEvaluate(parameters, "cycles", componentName, context, 0.0, out double cycles))
                    {
                        if (cycles < 2.0 || Math.Abs(cycles - Math.Round(cycles)) > 1e-12)
                        {
                            AddParameterRangeError(
                                context,
                                componentName,
                                "cycles",
                                "an integer of at least two",
                                parameters["cycles"]);
                            success = false;
                        }
                    }
                    else
                    {
                        success = false;
                    }

                    success &= TryValidateOptional(
                        componentName, parameters, "duty", context, value => value > 0.0 && value < 1.0, "greater than zero and less than one");
                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    break;
                case "SAMPLEHOLD":
                    if (parameters.TryGetValue("td", out ADeviceParameter delay))
                    {
                        AddError(
                            context,
                            $"LTspice A-device '{componentName}' parameter 'Td' is not supported for "
                            + "SAMPLEHOLD because its clock and track timing semantics are not yet implemented.",
                            delay.LineInfo);
                        success = false;
                    }

                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateVoltageWindow(componentName, parameters, context, 10.0, -10.0);
                    break;
                case "OTA":
                    success &= TryValidateOta(componentName, parameters, context);
                    break;
                case "VARISTOR":
                    success &= TryValidateOptional(
                        componentName, parameters, "rclamp", context, value => value > 0.0, "greater than zero");
                    success &= TryValidateOptional(
                        componentName, parameters, "roff", context, value => value > 0.0, "greater than zero");
                    break;
                case "MODULATE":
                case "MODULATOR":
                    if (!parameters.ContainsKey("mark"))
                    {
                        AddError(
                            context,
                            $"LTspice A-device '{componentName}' ({model}) requires the Mark parameter.",
                            null);
                        success = false;
                    }
                    else
                    {
                        success &= TryValidateOptional(
                            componentName, parameters, "mark", context, value => value >= 0.0, "zero or greater");
                    }

                    if (!parameters.ContainsKey("space"))
                    {
                        AddError(
                            context,
                            $"LTspice A-device '{componentName}' ({model}) requires the Space parameter.",
                            null);
                        success = false;
                    }
                    else
                    {
                        success &= TryValidateOptional(
                            componentName, parameters, "space", context, value => value >= 0.0, "zero or greater");
                    }

                    success &= TryValidateOptional(
                        componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
                    break;
            }

            return success;
        }

        private static bool TryValidateOta(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IReadingContext context)
        {
            bool success = true;
            if (!TryEvaluate(parameters, "iout", componentName, context, 10e-6, out double outputLimit))
            {
                success = false;
            }
            else if (outputLimit <= 0.0)
            {
                AddParameterRangeError(
                    context,
                    componentName,
                    "iout",
                    "greater than zero",
                    parameters.TryGetValue("iout", out ADeviceParameter iout) ? iout : null);
                success = false;
            }

            if (!TryEvaluate(parameters, "isrc", componentName, context, outputLimit, out double sourceLimit))
            {
                success = false;
            }
            else if (sourceLimit <= 0.0)
            {
                AddParameterRangeError(
                    context,
                    componentName,
                    "isrc",
                    "greater than zero",
                    parameters.TryGetValue("isrc", out ADeviceParameter isrc) ? isrc : null);
                success = false;
            }

            if (!TryEvaluate(parameters, "isink", componentName, context, -outputLimit, out double sinkLimit))
            {
                success = false;
            }
            else if (sinkLimit >= 0.0)
            {
                AddParameterRangeError(
                    context,
                    componentName,
                    "isink",
                    "less than zero",
                    parameters.TryGetValue("isink", out ADeviceParameter isink) ? isink : null);
                success = false;
            }

            success &= TryValidateOptional(
                componentName, parameters, "g", context, value => value > 0.0, "greater than zero");
            success &= TryValidateOptional(
                componentName, parameters, "rout", context, value => value > 0.0, "greater than zero");
            success &= TryValidateOptional(
                componentName, parameters, "rclamp", context, value => value > 0.0, "greater than zero");
            success &= TryValidateOptional(
                componentName, parameters, "cout", context, value => value >= 0.0, "zero or greater");
            success &= TryValidateVoltageWindow(componentName, parameters, context, 2.0, 0.0);
            return success;
        }

        private static bool TryValidateOptional(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string parameterName,
            IReadingContext context,
            Func<double, bool> predicate,
            string requirement)
        {
            if (!parameters.ContainsKey(parameterName))
            {
                return true;
            }

            if (!TryEvaluate(parameters, parameterName, componentName, context, 0.0, out double value))
            {
                return false;
            }

            if (predicate(value))
            {
                return true;
            }

            AddParameterRangeError(
                context,
                componentName,
                parameterName,
                requirement,
                parameters[parameterName]);
            return false;
        }

        private static bool TryValidateVoltageWindow(
            string componentName,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IReadingContext context,
            double defaultHigh,
            double defaultLow)
        {
            if (!TryEvaluate(parameters, "vhigh", componentName, context, defaultHigh, out double high)
                || !TryEvaluate(parameters, "vlow", componentName, context, defaultLow, out double low))
            {
                return false;
            }

            if (high > low)
            {
                return true;
            }

            AddError(
                context,
                $"LTspice A-device '{componentName}' Vhigh ({high}) must be greater than Vlow ({low}).",
                parameters.TryGetValue("vhigh", out ADeviceParameter highParameter)
                    ? highParameter.LineInfo
                    : null);
            return false;
        }

        private static void AddParameterRangeError(
            IReadingContext context,
            string componentName,
            string parameterName,
            string requirement,
            ADeviceParameter parameter)
        {
            AddError(
                context,
                $"LTspice A-device '{componentName}' parameter '{parameterName}' must be {requirement}.",
                parameter?.LineInfo);
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

        private static bool IsAllowedFlag(string model, string flag)
        {
            return model == "OTA"
                && (flag.Equals("linear", StringComparison.OrdinalIgnoreCase)
                    || flag.Equals("asym", StringComparison.OrdinalIgnoreCase));
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
            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                message,
                lineInfo);
        }

        private sealed class ADeviceParameter
        {
            public ADeviceParameter(string expression, SpiceLineInfo lineInfo)
            {
                Expression = expression;
                LineInfo = lineInfo;
            }

            public string Expression { get; }

            public SpiceLineInfo LineInfo { get; }
        }
    }
}
