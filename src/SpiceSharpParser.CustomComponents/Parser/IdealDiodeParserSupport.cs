using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.CustomComponents
{
    internal static class IdealDiodeParserSupport
    {
        private static readonly ISet<string> IdealModelParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        private static readonly ISet<string> ModelParameters = new HashSet<string>(IdealModelParameters, StringComparer.OrdinalIgnoreCase)
        {
            "rs",
        };

        private static readonly ISet<string> InstanceParameters = new HashSet<string>(ModelParameters, StringComparer.OrdinalIgnoreCase)
        {
            "area",
            "off",
            "m",
            "n",
        };

        private static readonly ISet<string> SelectionParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lmin",
            "lmax",
            "wmin",
            "wmax",
        };

        private static readonly ISet<string> IgnoredClassicModelParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "is",
            "tnom",
            "n",
            "tt",
            "cjo",
            "cj0",
            "vj",
            "m",
            "eg",
            "xti",
            "fc",
            "bv",
            "ibv",
            "kf",
            "af",
        };

        private static readonly ISet<string> IgnoredInstanceParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "temp",
            "ic",
        };

        private static readonly ISet<string> MetadataParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        public static bool HasIdealParameter(ParameterCollection parameters)
        {
            return parameters
                .OfType<AssignmentParameter>()
                .Any(parameter => IdealModelParameters.Contains(parameter.Name));
        }

        public static bool IsModelParameter(string parameterName)
        {
            return ModelParameters.Contains(parameterName);
        }

        public static void SetModelParameters(IReadingContext context, IdealDiodeModel entity, Model model, ParameterCollection parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (!(parameter is AssignmentParameter assignment))
                {
                    AddUnsupportedParameter(context, "ideal diode model", model.Name, parameter);
                    continue;
                }

                if (SelectionParameters.Contains(assignment.Name))
                {
                    StoreSelectionParameter(context, model, assignment);
                    continue;
                }

                if (IgnoredClassicModelParameters.Contains(assignment.Name)
                    || MetadataParameters.Contains(assignment.Name))
                {
                    continue;
                }

                if (!ModelParameters.Contains(assignment.Name))
                {
                    AddUnsupportedParameter(context, "ideal diode model", model.Name, parameter);
                    continue;
                }

                SetParameter(context, entity, assignment);
            }
        }

        public static bool SetInstanceParameter(IReadingContext context, IdealDiode entity, string name, AssignmentParameter parameter)
        {
            if (IgnoredInstanceParameters.Contains(parameter.Name) || MetadataParameters.Contains(parameter.Name))
            {
                return false;
            }

            if (!InstanceParameters.Contains(parameter.Name))
            {
                AddUnsupportedParameter(context, "ideal diode", name, parameter);
                return false;
            }

            return SetParameter(context, entity, parameter);
        }

        public static bool SetPositionalAreaParameter(IReadingContext context, IdealDiode entity, Parameter parameter)
        {
            return SetParameter(context, entity, "area", parameter);
        }

        private static void StoreSelectionParameter(IReadingContext context, Model model, AssignmentParameter parameter)
        {
            try
            {
                var value = context.Evaluator.EvaluateDouble(parameter.Value);
                model.SetSelectionParameter(parameter.Name, value);
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Problem with setting ideal diode model selection parameter: {parameter}",
                    parameter.LineInfo,
                    ex);
            }
        }

        private static bool SetParameter(IReadingContext context, IEntity entity, AssignmentParameter parameter)
        {
            return SetParameter(context, entity, parameter.Name, parameter);
        }

        private static bool SetParameter(IReadingContext context, IEntity entity, string parameterName, Parameter parameter)
        {
            try
            {
                context.SetParameter(entity, parameterName, parameter.Value, logError: false);
                return true;
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Problem with setting parameter: {parameter}",
                    parameter.LineInfo,
                    ex);
                return false;
            }
        }

        private static void AddUnsupportedParameter(IReadingContext context, string targetKind, string targetName, Parameter parameter)
        {
            context.Result.ValidationResult.AddError(
                ValidationEntrySource.Reader,
                $"Unsupported {targetKind} parameter '{parameter}' on '{targetName}'.",
                parameter.LineInfo);
        }
    }
}
