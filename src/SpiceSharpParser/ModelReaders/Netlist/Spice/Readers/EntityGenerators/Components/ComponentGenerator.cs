using System;
using System.Linq;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public abstract class ComponentGenerator : IComponentGenerator
    {
        public abstract IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context);

        /// <summary>
        /// Creates a predicate that checks whether a model's selection parameters
        /// satisfy min/max range constraints for the given instance parameter values.
        /// For each (name, value) pair, checks model's {name}min and {name}max.
        /// </summary>
        public static Func<Context.Models.Model, bool> CreateRangePredicate(params (string name, double? value)[] parameters)
        {
            if (parameters.All(p => p.value == null))
            {
                return null;
            }

            return model =>
            {
                foreach (var (name, value) in parameters)
                {
                    if (!value.HasValue)
                    {
                        continue;
                    }

                    if (model.TryGetSelectionParameter(name + "min", out double min) && min > 0 && value.Value < min)
                    {
                        return false;
                    }

                    if (model.TryGetSelectionParameter(name + "max", out double max) && max > 0 && value.Value > max)
                    {
                        return false;
                    }
                }

                return true;
            };
        }

        /// <summary>
        /// Gets the value of a named assignment parameter from the collection, or null if not found.
        /// </summary>
        public static double? GetAssignmentParameterValue(string name, ParameterCollection parameters, IReadingContext context)
        {
            var parameter = parameters.FirstOrDefault(p => p is AssignmentParameter ap && ap.Name.ToLower() == name);
            if (parameter is AssignmentParameter ap)
            {
                return context.Evaluator.EvaluateDouble(ap.Value);
            }
            return null;
        }

        protected void SetParameters(IReadingContext context, IEntity entity, ParameterCollection parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        context.SetParameter(entity, ap.Name, ap.Value);
                    }
                    catch (Exception)
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Problem with setting parameter: {parameter}", parameter.LineInfo);
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