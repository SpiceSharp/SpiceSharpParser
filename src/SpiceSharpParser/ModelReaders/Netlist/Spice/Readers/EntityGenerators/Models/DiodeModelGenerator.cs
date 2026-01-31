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
            var diodeModel = new DiodeModel(id);
            var contextModel = new Model(id, diodeModel, diodeModel.Parameters);
            SetParameters(context, diodeModel, parameters);
            SetDimensionParameters(context, contextModel, parameters);

            return contextModel;
        }
    }
}