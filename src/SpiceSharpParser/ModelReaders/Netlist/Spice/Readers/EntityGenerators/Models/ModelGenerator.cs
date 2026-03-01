using System;
using System.Collections.Generic;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public abstract class ModelGenerator : IModelGenerator
    {
        /// <summary>
        /// Parameters to skip when setting on the SpiceSharp entity (because they are selection-only).
        /// Subclasses can override to add custom selection parameter names.
        /// </summary>
        protected virtual ISet<string> EntitySkipParameters { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lmin", "lmax", "wmin", "wmax"
        };

        public abstract Context.Models.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context);

        protected void SetParameters(IReadingContext context, IEntity entity, Context.Models.Model model, ParameterCollection parameters, bool onload = true)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    // Store ALL parameters on Model for predicate-based selection
                    try
                    {
                        var value = context.Evaluator.EvaluateDouble(ap.Value);
                        model.SetSelectionParameter(ap.Name, value);
                    }
                    catch
                    {
                        // Evaluation error — will be reported below when setting on entity
                    }

                    // Skip entity-setting for params in the skip set
                    if (EntitySkipParameters.Contains(ap.Name))
                    {
                        continue;
                    }

                    // Set on SpiceSharp entity
                    try
                    {
                        context.SetParameter(entity, ap.Name, ap.Value, onload);
                    }
                    catch (Exception ex)
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Problem with setting parameter: {parameter}", parameter.LineInfo, ex);
                    }
                }
                else
                {
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Unsupported parameter: {parameter}", parameter.LineInfo);
                }
            }
        }
    }
}
