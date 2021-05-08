using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class DiodeModelGenerator : ModelGenerator
    {
        public override Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            var model = new DiodeModel(id);
            SetParameters(context, model, parameters);

            return new Model(id, model, model.Parameters);
        }
    }
}