using System;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public abstract class ComponentGenerator : IComponentGenerator
    {
        public abstract IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context);

        protected void SetParameters(ICircuitContext context, IEntity entity, ParameterCollection parameters, bool onload)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        context.SetParameter(entity, ap.Name, ap.Value, true, onload);
                    }
                    catch (Exception)
                    {
                        context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Problem with setting parameter: {parameter.Image}", parameter.LineInfo));
                    }
                }
                else
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported parameter: {parameter.Image}", parameter.LineInfo));
                }
            }
        }
    }
}