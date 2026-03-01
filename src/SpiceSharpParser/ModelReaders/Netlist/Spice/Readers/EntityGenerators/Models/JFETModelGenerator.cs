using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class JFETModelGenerator : ModelGenerator
    {
        public override Context.Models.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            var jfetModel = new JFETModel(id);
            switch (type.ToLower())
            {
                case "pjf": jfetModel.SetParameter("pjf", true); break;
                case "njf": jfetModel.SetParameter("njf", true); break;
            }

            var contextModel = new Context.Models.Model(id, jfetModel, jfetModel.Parameters);
            SetParameters(context, jfetModel, contextModel, parameters);
            return contextModel;
        }
    }
}