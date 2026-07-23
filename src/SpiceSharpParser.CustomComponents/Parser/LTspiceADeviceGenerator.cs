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
                ("rout", "ROUT"));

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
                ("rclamp", "RCLAMP"));

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
        private readonly AnalogSubcircuitLibrary _analog;

        /// <summary>
        /// Initializes a new instance of the <see cref="LTspiceADeviceGenerator"/> class.
        /// </summary>
        public LTspiceADeviceGenerator()
        {
            _digital = DigitalSubcircuitLibrary.LoadBuiltIn();
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

            string portableInstanceName = "X" + componentIdentifier;
            try
            {
                switch (model)
                {
                    case "SRFLOP":
                        AddSetResetFlipFlop(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "DFLOP":
                        AddDFlipFlop(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "PHASEDET":
                        AddPhaseDetector(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "COUNTER":
                        AddCounter(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "SAMPLEHOLD":
                        AddSampleHold(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "OTA":
                        AddOta(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "VARISTOR":
                        AddVaristor(context, portableInstanceName, terminals, instanceParameters);
                        break;
                    case "MODULATE":
                    case "MODULATOR":
                        AddModulator(context, portableInstanceName, terminals, instanceParameters);
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, SetResetParameterMap, context, out Dictionary<string, string> mapped)
                || !TryCreateDigitalRails(
                    context,
                    instanceName,
                    terminals[7],
                    parameters,
                    mapped,
                    out string highNode,
                    out string lowNode))
            {
                return;
            }

            SetCompatibilityDefaults(mapped, includeDelay: true, includeState: true);
            _digital.Library.AddInstance(
                context.ContextEntities,
                "DIG_SR_LATCH",
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, DFlipFlopParameterMap, context, out Dictionary<string, string> mapped)
                || !TryCreateDigitalRails(
                    context,
                    instanceName,
                    terminals[7],
                    parameters,
                    mapped,
                    out string highNode,
                    out string lowNode))
            {
                return;
            }

            SetCompatibilityDefaults(mapped, includeDelay: true, includeState: true);
            _digital.Library.AddInstance(
                context.ContextEntities,
                "DIG_DFF",
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, PhaseDetectorParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            mapped["RSTATE"] = "1";
            mapped["CMEM"] = "1p";
            _digital.Library.AddInstance(
                context.ContextEntities,
                "DIG_PHASE_DETECTOR",
                instanceName,
                new[] { terminals[0], terminals[1], terminals[6], terminals[7] },
                mapped);
        }

        private void AddCounter(
            IReadingContext context,
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, CounterParameterMap, context, out Dictionary<string, string> mapped)
                || !TryCreateDigitalRails(
                    context,
                    instanceName,
                    terminals[7],
                    parameters,
                    mapped,
                    out string highNode,
                    out string lowNode))
            {
                return;
            }

            SetCompatibilityDefaults(mapped, includeDelay: false, includeState: false);
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, SampleHoldParameterMap, context, out Dictionary<string, string> mapped))
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, OtaParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, VaristorParameterMap, context, out Dictionary<string, string> mapped))
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
            string instanceName,
            string[] terminals,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            if (!TryMapParameters(parameters, ModulatorParameterMap, context, out Dictionary<string, string> mapped))
            {
                return;
            }

            _analog.Library.AddInstance(
                context.ContextEntities,
                "ANALOG_MODULATOR",
                instanceName,
                new[] { terminals[0], terminals[1], terminals[6], terminals[7] },
                mapped);
        }

        private static bool TryCreateDigitalRails(
            IReadingContext context,
            string instanceName,
            string commonNode,
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            IDictionary<string, string> mapped,
            out string highNode,
            out string lowNode)
        {
            highNode = instanceName + ".__a_vhigh";
            lowNode = instanceName + ".__a_vlow";
            if (!TryEvaluate(parameters, "vhigh", context, 1.0, out double high)
                || !TryEvaluate(parameters, "vlow", context, 0.0, out double low))
            {
                return false;
            }

            if (high <= low)
            {
                AddError(
                    context,
                    $"LTspice A-device Vhigh ({high}) must be greater than Vlow ({low}).",
                    parameters.TryGetValue("vhigh", out ADeviceParameter highParameter)
                        ? highParameter.LineInfo
                        : null);
                return false;
            }

            if (parameters.ContainsKey("ref"))
            {
                if (!TryEvaluate(parameters, "ref", context, 0.0, out double reference))
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
                        $"Unsupported LTspice A-device parameter '{item.Key}'.",
                        item.Value.LineInfo);
                    success = false;
                    continue;
                }

                if (portableName == null)
                {
                    continue;
                }

                try
                {
                    double value = context.Evaluator.EvaluateDouble(item.Value.Expression);
                    result[portableName] = value.ToString("R", CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    AddError(
                        context,
                        $"Could not evaluate LTspice A-device parameter '{item.Key}': {exception.Message}",
                        item.Value.LineInfo);
                    success = false;
                }
            }

            return success;
        }

        private static bool TryEvaluate(
            IReadOnlyDictionary<string, ADeviceParameter> parameters,
            string name,
            IReadingContext context,
            double defaultValue,
            out double result)
        {
            if (!parameters.TryGetValue(name, out ADeviceParameter parameter))
            {
                result = defaultValue;
                return true;
            }

            try
            {
                result = context.Evaluator.EvaluateDouble(parameter.Expression);
                return true;
            }
            catch (Exception exception)
            {
                AddError(
                    context,
                    $"Could not evaluate LTspice A-device parameter '{name}': {exception.Message}",
                    parameter.LineInfo);
                result = double.NaN;
                return false;
            }
        }

        private static void SetCompatibilityDefaults(
            IDictionary<string, string> parameters,
            bool includeDelay,
            bool includeState)
        {
            if (includeDelay && !parameters.ContainsKey("TPD"))
            {
                parameters["TPD"] = "0";
            }

            if (!parameters.ContainsKey("ROUT"))
            {
                parameters["ROUT"] = "1";
            }

            parameters["COUT"] = "1f";
            if (includeState)
            {
                parameters["RSTATE"] = "1";
                parameters["CMEM"] = "1p";
            }
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
