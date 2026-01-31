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
        /// The dimension parameters used for model selection (not to be set on SpiceSharp entities).
        /// </summary>
        private static readonly HashSet<string> DimensionParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lmin", "lmax", "wmin", "wmax"
        };

        public abstract Context.Models.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context);

        protected void SetParameters(IReadingContext context, IEntity entity, ParameterCollection parameters, bool onload = true)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    // Skip dimension parameters - they are handled separately
                    if (DimensionParameterNames.Contains(ap.Name))
                    {
                        continue;
                    }

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
                    context.Result.ValidationResult.AddError(ValidationEntrySource.Reader,  $"Unsupported parameter: {parameter}", parameter.LineInfo);
                }
            }
        }

        /// <summary>
        /// Sets dimension parameters (lmin, lmax, wmin, wmax) on a model for model selection.
        /// </summary>
        /// <param name="context">The reading context.</param>
        /// <param name="model">The model to set dimension parameters on.</param>
        /// <param name="parameters">The parameters collection.</param>
        protected void SetDimensionParameters(IReadingContext context, Context.Models.Model model, ParameterCollection parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap && DimensionParameterNames.Contains(ap.Name))
                {
                    try
                    {
                        var value = context.Evaluator.EvaluateDouble(ap.Value);
                        model.SetDimensionParameter(ap.Name, value);
                    }
                    catch (Exception ex)
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Problem with setting dimension parameter: {parameter}", parameter.LineInfo, ex);
                    }
                }
            }
        }
    }
}