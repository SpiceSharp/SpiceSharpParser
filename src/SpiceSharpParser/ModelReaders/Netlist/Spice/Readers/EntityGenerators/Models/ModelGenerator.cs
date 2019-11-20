using System.Collections.Generic;
using SpiceSharp.Circuits;
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
                    context.SetParameter(entity, ap.Name, ap.Value, onload);
                }
                else
                {
                    context.Result.AddWarning("Unsupported parameter: " + parameter.Image);
                }
            }
        }
    }
}
