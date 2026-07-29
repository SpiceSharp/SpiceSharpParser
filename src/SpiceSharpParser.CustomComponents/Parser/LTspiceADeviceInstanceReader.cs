using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// The syntax-level representation of one LTspice A-device instance.
    /// </summary>
    internal sealed class LTspiceADeviceInstance
    {
        public const int TerminalCount = 8;

        public LTspiceADeviceInstance(
            string[] terminals,
            string model,
            SpiceLineInfo modelLineInfo,
            IReadOnlyDictionary<string, ADeviceParameter> parameters)
        {
            Terminals = terminals;
            Model = model;
            ModelLineInfo = modelLineInfo;
            Parameters = parameters;
        }

        public string[] Terminals { get; }

        public string Model { get; }

        public SpiceLineInfo ModelLineInfo { get; }

        public IReadOnlyDictionary<string, ADeviceParameter> Parameters { get; }
    }

    /// <summary>
    /// Reads terminal, model, flag, and raw parameter syntax without expanding it.
    /// </summary>
    internal static class LTspiceADeviceInstanceReader
    {
        private const int ModelIndex = LTspiceADeviceInstance.TerminalCount;
        private const int RequiredParameterCount = ModelIndex + 1;

        public static bool TryRead(
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            out LTspiceADeviceInstance result)
        {
            result = null;

            if (parameters.Count < RequiredParameterCount)
            {
                LTspiceADeviceDiagnostics.AddError(
                    context,
                    $"LTspice A-device '{originalName}' expects eight terminals followed by a model name.",
                    parameters.LineInfo);
                return false;
            }

            for (int index = 0; index < RequiredParameterCount; index++)
            {
                if (!(parameters[index] is SingleParameter))
                {
                    string position = index < LTspiceADeviceInstance.TerminalCount
                        ? $"terminal {index + 1}"
                        : "model name";
                    LTspiceADeviceDiagnostics.AddError(
                        context,
                        $"LTspice A-device '{originalName}' has an invalid {position}.",
                        parameters[index].LineInfo);
                    return false;
                }
            }

            var terminals = new string[LTspiceADeviceInstance.TerminalCount];
            for (int index = 0; index < terminals.Length; index++)
            {
                string terminal = parameters[index].Value;
                terminals[index] = context.ReaderSettings.ExpandSubcircuits
                    ? context.NameGenerator.GenerateNodeName(terminal)
                    : terminal;
            }

            string model = parameters[ModelIndex].Value.ToUpperInvariant();
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
                        LTspiceADeviceDiagnostics.AddError(
                            context,
                            $"Unknown bare flag '{name}' on LTspice A-device '{originalName}' ({model}). "
                            + "Only OTA Linear and Asym may use bare flag syntax.",
                            parameter.LineInfo);
                        return false;
                    }
                }
                else
                {
                    LTspiceADeviceDiagnostics.AddError(
                        context,
                        $"Unsupported parameter syntax '{parameter}' on LTspice A-device '{originalName}'.",
                        parameter.LineInfo);
                    return false;
                }

                if (parsed.ContainsKey(name))
                {
                    LTspiceADeviceDiagnostics.AddError(
                        context,
                        $"Duplicate LTspice A-device parameter '{name}' on component '{originalName}'.",
                        parameter.LineInfo);
                    return false;
                }

                parsed.Add(name, new ADeviceParameter(expression, parameter.LineInfo));
            }

            result = new LTspiceADeviceInstance(
                terminals,
                model,
                parameters[ModelIndex].LineInfo,
                parsed);
            return true;
        }

        private static bool IsAllowedFlag(string model, string flag)
        {
            return model == "OTA"
                && (flag.Equals("linear", StringComparison.OrdinalIgnoreCase)
                    || flag.Equals("asym", StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// A raw A-device parameter expression and its source location.
    /// </summary>
    internal sealed class ADeviceParameter
    {
        public ADeviceParameter(string expression, SpiceLineInfo lineInfo)
        {
            Expression = expression;
            LineInfo = lineInfo;
        }

        public string Expression { get; }

        public SpiceLineInfo LineInfo { get; }
    }

    internal static class LTspiceADeviceDiagnostics
    {
        public static void AddError(
            IReadingContext context,
            string message,
            SpiceLineInfo lineInfo)
        {
            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                message,
                lineInfo);
        }
    }
}
