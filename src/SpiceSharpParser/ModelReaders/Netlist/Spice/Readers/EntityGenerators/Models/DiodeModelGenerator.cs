using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class DiodeModelGenerator : ModelGenerator
    {
        public override SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, ICircuitContext context)
        {
            var model = new DiodeModel(id);
            SetParameters(context, model, parameters);
            return model;
        }
    }
}