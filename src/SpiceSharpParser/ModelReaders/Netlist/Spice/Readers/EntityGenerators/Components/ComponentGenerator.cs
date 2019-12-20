using System;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components
{
    public abstract class ComponentGenerator : IComponentGenerator
    {
        public abstract SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context);

        protected void SetParameters(ICircuitContext context, Entity entity, ParameterCollection parameters, bool onload)
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
                        context.Result.AddValidationException(new InvalidParameterException($"Problem with setting parameter: {parameter.Image}", parameter.LineInfo));
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