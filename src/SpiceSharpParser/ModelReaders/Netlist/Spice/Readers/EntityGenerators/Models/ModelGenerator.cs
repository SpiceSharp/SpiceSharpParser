using System;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public abstract class ModelGenerator : IModelGenerator
    {
        public abstract SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, ICircuitContext context);

        protected void SetParameters(ICircuitContext context, Entity entity, ParameterCollection parameters, bool onload = true)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        context.SetParameter(entity, ap.Name, ap.Value, onload);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddValidationException(new SpiceSharpParserException($"Problem with setting parameter: {parameter.Image}", ex, parameter.LineInfo));
                    }
                }
                else
                {
                    context.Result.AddValidationException(new SpiceSharpParserException($"Unsupported parameter: {parameter.Image}", parameter.LineInfo));
                }
            }
        }
    }
}