using System;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public abstract class ModelGenerator : IModelGenerator
    {
        public abstract Context.Models.Model Generate(string id, string type, ParameterCollection parameters, ICircuitContext context);

        protected void SetParameters(ICircuitContext context, IEntity entity, ParameterCollection parameters, bool onload = true)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        context.SetParameter(entity, ap.Name, ap.Value, onload);
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