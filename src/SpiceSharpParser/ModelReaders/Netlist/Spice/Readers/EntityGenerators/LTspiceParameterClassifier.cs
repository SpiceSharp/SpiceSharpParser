using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    internal static class LTspiceParameterClassifier
    {
        // These are schematic/library annotations or passive part ratings. LTspice carries them
        // through generated decks, but SpiceSharpParser has no BOM/layout surface where they matter.
        private static readonly ISet<string> MetadataNoOpParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mfg",
            "manufacturer",
            "pn",
            "part",
            "desc",
            "description",
            "v",
            "irms",
            "ipk",
        };

        // Unsupported passive parasitics change circuit topology in LTspice. Resistor and capacitor
        // parasitics that are synthesized by the parser are consumed by RLCKGenerator before this
        // classifier sees component extras.
        private static readonly ISet<string> PassiveParasiticParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "rser",
            "rpar",
            "cpar",
            "lser",
            "rlshunt",
        };

        // LTspice switches to a separate idealized diode model when these are present. Mapping them
        // onto Berkeley diode parameters would be numerically misleading, so they are explicit gaps.
        private static readonly ISet<string> IdealDiodeParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ron",
            "roff",
            "vfwd",
            "vrev",
            "rrev",
            "ilimit",
            "revilimit",
            "epsilon",
            "revepsilon",
        };

        // LTspice voltage-switch extras imply added series elements or current limiting. Ron/Roff/Vt/Vh
        // remain pass-through; these behavior-changing extras need runtime/topology support first.
        private static readonly ISet<string> UnsupportedSwitchParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lser",
            "vser",
            "ilimit",
        };

        public static ParameterCollection NormalizeModelParameters(
            IReadingContext context,
            string modelName,
            string modelType,
            ParameterCollection parameters)
        {
            if (!IsLTspice(context))
            {
                return parameters;
            }

            if (IsVoltageSwitchModelType(modelType))
            {
                return NormalizePairedAlias(
                    context,
                    modelName,
                    modelType,
                    parameters,
                    "von",
                    "voff",
                    "vt",
                    "vh");
            }

            if (IsCurrentSwitchModelType(modelType))
            {
                return NormalizePairedAlias(
                    context,
                    modelName,
                    modelType,
                    parameters,
                    "ion",
                    "ioff",
                    "it",
                    "ih");
            }

            return parameters;
        }

        public static bool TryHandleModelParameter(
            IReadingContext context,
            IEntity entity,
            string modelName,
            string modelType,
            AssignmentParameter parameter,
            bool beforeTemperature)
        {
            if (!IsLTspice(context))
            {
                return false;
            }

            if (IsMetadataNoOp(parameter.Name))
            {
                AddIgnoredParameterWarning(context, "model", modelName, parameter.Name, parameter.LineInfo);
                return true;
            }

            if (IsRlcModelType(modelType) && Is(parameter.Name, "tc"))
            {
                MapTemperatureCoefficient(context, entity, modelName, parameter, beforeTemperature);
                return true;
            }

            if (IsDiodeModelType(modelType) && IdealDiodeParameters.Contains(parameter.Name))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice diode model parameter '{parameter.Name}' on model '{modelName}': ideal-diode linear-region/current-limit semantics are not mapped yet.",
                    parameter.LineInfo);
                return true;
            }

            if (IsSwitchModelType(modelType) && UnsupportedSwitchParameters.Contains(parameter.Name))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice switch model parameter '{parameter.Name}' on model '{modelName}': switch series elements/current limiting are not synthesized yet.",
                    parameter.LineInfo);
                return true;
            }

            return false;
        }

        public static bool TryHandleComponentParameter(
            IReadingContext context,
            IEntity entity,
            string componentName,
            string componentType,
            AssignmentParameter parameter,
            bool beforeTemperature = true)
        {
            if (!IsLTspice(context))
            {
                return false;
            }

            if (IsMetadataNoOp(parameter.Name))
            {
                AddIgnoredParameterWarning(context, "component", componentName, parameter.Name, parameter.LineInfo);
                return true;
            }

            if (IsPassiveComponentType(componentType) && PassiveParasiticParameters.Contains(parameter.Name))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice {componentType.ToUpperInvariant()} instance parameter '{parameter.Name}' on component '{componentName}': passive parasitic topology is not synthesized yet.",
                    parameter.LineInfo);
                return true;
            }

            if (IsCapacitorComponentType(componentType) && Is(parameter.Name, "q"))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice capacitor parameter 'Q' on component '{componentName}': charge-defined capacitors are not mapped yet.",
                    parameter.LineInfo);
                return true;
            }

            if (IsInductorComponentType(componentType) && Is(parameter.Name, "flux"))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice inductor parameter 'Flux' on component '{componentName}': flux-defined inductors are not mapped yet.",
                    parameter.LineInfo);
                return true;
            }

            return false;
        }

        public static bool TryRejectUnsupportedPassiveValue(
            IReadingContext context,
            string componentName,
            string componentType,
            Parameter parameter)
        {
            if (!IsLTspice(context) || !(parameter is AssignmentParameter assignment))
            {
                return false;
            }

            return TryHandleComponentParameter(
                context,
                null,
                componentName,
                componentType,
                assignment);
        }

        public static bool TryAddUnsupportedModelTypeError(
            IReadingContext context,
            string modelName,
            string modelType,
            SpiceLineInfo lineInfo)
        {
            if (!IsLTspice(context))
            {
                return false;
            }

            if (Is(modelType, "vdmos"))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice model type 'VDMOS' for model '{modelName}': vertical power MOSFET behavior requires SpiceSharp engine support.",
                    lineInfo);
                return true;
            }

            if (Is(modelType, "ltra"))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice model type 'LTRA' for model '{modelName}': lossy transmission-line behavior requires SpiceSharp engine support.",
                    lineInfo);
                return true;
            }

            if (Is(modelType, "urc"))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice model type 'URC' for model '{modelName}': uniform RC-line behavior requires SpiceSharp engine support.",
                    lineInfo);
                return true;
            }

            return false;
        }

        public static bool TryAddUnsupportedComponentTypeError(
            IReadingContext context,
            string componentName,
            SpiceLineInfo lineInfo)
        {
            if (!IsLTspice(context) || string.IsNullOrEmpty(componentName))
            {
                return false;
            }

            if (componentName.StartsWith("O", StringComparison.OrdinalIgnoreCase))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice component '{componentName}': O/LTRA lossy transmission lines require SpiceSharp engine support.",
                    lineInfo);
                return true;
            }

            if (componentName.StartsWith("U", StringComparison.OrdinalIgnoreCase))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice component '{componentName}': U/URC uniform RC lines require SpiceSharp engine support.",
                    lineInfo);
                return true;
            }

            return false;
        }

        public static void AddUnsupportedMosfetLevelError(
            IReadingContext context,
            int level,
            string modelType,
            SpiceLineInfo lineInfo)
        {
            if (IsLTspice(context))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice MOS model level {level} for '{modelType}': only legacy levels 1, 2, and 3 are mapped in SpiceSharpParser.",
                    lineInfo);
            }
            else
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unknown mosfet model level {level}",
                    lineInfo);
            }
        }

        public static bool TryAddUnsupportedThreeTerminalMosfetError(
            IReadingContext context,
            string componentName,
            ParameterCollection parameters)
        {
            if (!IsLTspice(context) || parameters.Count != 4)
            {
                return false;
            }

            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                $"Unsupported LTspice three-terminal MOS syntax for component '{componentName}': VDMOS/power-MOS behavior requires SpiceSharp engine support.",
                parameters.LineInfo);
            return true;
        }

        private static ParameterCollection NormalizePairedAlias(
            IReadingContext context,
            string modelName,
            string modelType,
            ParameterCollection parameters,
            string highAlias,
            string lowAlias,
            string midpointParameter,
            string halfSpanParameter)
        {
            var highIndex = FindAssignmentIndex(parameters, highAlias);
            var lowIndex = FindAssignmentIndex(parameters, lowAlias);
            if (highIndex < 0 && lowIndex < 0)
            {
                return parameters;
            }

            var result = new ParameterCollection();
            var skip = new HashSet<int>();

            if (highIndex >= 0 && lowIndex >= 0)
            {
                if (FindAssignmentIndex(parameters, midpointParameter) >= 0
                    || FindAssignmentIndex(parameters, halfSpanParameter) >= 0)
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Unsupported LTspice switch alias mix on model '{modelName}': do not combine '{highAlias}/{lowAlias}' with '{midpointParameter}/{halfSpanParameter}'.",
                        parameters[Math.Min(highIndex, lowIndex)].LineInfo);
                    skip.Add(highIndex);
                    skip.Add(lowIndex);
                }
                else
                {
                    var high = (AssignmentParameter)parameters[highIndex];
                    var low = (AssignmentParameter)parameters[lowIndex];
                    var lineInfo = high.LineInfo ?? low.LineInfo;

                    // LTspice Vt/It is the midpoint, while Vh/Ih is half the on/off span.
                    result.Add(CreateAssignment(midpointParameter, $"(({high.Value}) + ({low.Value})) / 2", lineInfo));
                    result.Add(CreateAssignment(halfSpanParameter, $"(({high.Value}) - ({low.Value})) / 2", lineInfo));
                    skip.Add(highIndex);
                    skip.Add(lowIndex);
                }
            }
            else
            {
                var presentIndex = highIndex >= 0 ? highIndex : lowIndex;
                var presentName = highIndex >= 0 ? highAlias : lowAlias;
                var missingName = highIndex >= 0 ? lowAlias : highAlias;
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice switch alias '{presentName}' on model '{modelName}': expected matching '{missingName}' to derive '{midpointParameter}/{halfSpanParameter}'.",
                    parameters[presentIndex].LineInfo);
                skip.Add(presentIndex);
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                if (!skip.Contains(i))
                {
                    result.Add(parameters[i]);
                }
            }

            return result;
        }

        private static void MapTemperatureCoefficient(
            IReadingContext context,
            IEntity entity,
            string modelName,
            AssignmentParameter parameter,
            bool beforeTemperature)
        {
            if (parameter.Values.Count == 0 || parameter.Values.Count > 2)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Unsupported LTspice model parameter 'tc' on model '{modelName}': expected one or two coefficients.",
                    parameter.LineInfo);
                return;
            }

            context.SetParameter(entity, "tc1", parameter.Values[0], beforeTemperature);
            if (parameter.Values.Count == 2)
            {
                context.SetParameter(entity, "tc2", parameter.Values[1], beforeTemperature);
            }
        }

        private static void AddIgnoredParameterWarning(
            IReadingContext context,
            string targetKind,
            string targetName,
            string parameterName,
            SpiceLineInfo lineInfo)
        {
            context.Result.ValidationResult.AddWarning(
                ValidationEntrySource.Reader,
                $"Ignored LTspice {targetKind} parameter '{parameterName}' on '{targetName}': metadata/rating parameter is not used by SpiceSharpParser.",
                lineInfo);
        }

        private static int FindAssignmentIndex(ParameterCollection parameters, string name)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] is AssignmentParameter assignment && Is(assignment.Name, name))
                {
                    return i;
                }
            }

            return -1;
        }

        private static AssignmentParameter CreateAssignment(string name, string value, SpiceLineInfo lineInfo)
        {
            return new AssignmentParameter(name, null, new List<string> { value }, false, lineInfo);
        }

        private static bool IsLTspice(IReadingContext context)
        {
            return context?.ReaderSettings?.Compatibility?.IsLTspice == true;
        }

        private static bool IsMetadataNoOp(string parameterName)
        {
            return MetadataNoOpParameters.Contains(parameterName);
        }

        private static bool IsRlcModelType(string modelType)
        {
            return Is(modelType, "r") || Is(modelType, "res") || Is(modelType, "c");
        }

        private static bool IsDiodeModelType(string modelType)
        {
            return Is(modelType, "d");
        }

        private static bool IsSwitchModelType(string modelType)
        {
            return IsVoltageSwitchModelType(modelType) || IsCurrentSwitchModelType(modelType);
        }

        private static bool IsVoltageSwitchModelType(string modelType)
        {
            return Is(modelType, "sw");
        }

        private static bool IsCurrentSwitchModelType(string modelType)
        {
            return Is(modelType, "csw");
        }

        private static bool IsPassiveComponentType(string componentType)
        {
            return Is(componentType, "r") || Is(componentType, "c") || Is(componentType, "l");
        }

        private static bool IsCapacitorComponentType(string componentType)
        {
            return Is(componentType, "c");
        }

        private static bool IsInductorComponentType(string componentType)
        {
            return Is(componentType, "l");
        }

        private static bool Is(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
