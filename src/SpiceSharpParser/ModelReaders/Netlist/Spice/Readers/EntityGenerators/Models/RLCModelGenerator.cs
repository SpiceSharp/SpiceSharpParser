using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class RLCModelGenerator : ModelGenerator
    {
        public override SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            SpiceSharp.Components.Model model = null;

            switch (type.ToLower())
            {
                case "res":
                case "r":
                    model = new ResistorModel(id);
                    break;
                case "c":
                    model = new CapacitorModel(id);
                    break;
            }

            if (model != null)
            {
                SetParameters(context, model, parameters);
            }

            return model;
        }
    }
}
