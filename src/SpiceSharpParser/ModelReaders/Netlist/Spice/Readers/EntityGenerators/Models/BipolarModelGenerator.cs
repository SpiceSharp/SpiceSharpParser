using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class BipolarModelGenerator : ModelGenerator
    {
        public override Model Generate(string id, string type, SpiceSharpParser.Models.Netlist.Spice.Objects.ParameterCollection parameters, IReadingContext context)
        {
            BipolarJunctionTransistorModel bjtModel = new BipolarJunctionTransistorModel(id);

            if (type.ToLower() == "npn")
            {
                bjtModel.SetParameter("npn", true);
            }
            else if (type.ToLower() == "pnp")
            {
                bjtModel.SetParameter("pnp", true);
            }

            var contextModel = new Model(id, bjtModel, bjtModel.Parameters);
            SetParameters(context, bjtModel, contextModel, parameters);

            return contextModel;
        }
    }
}