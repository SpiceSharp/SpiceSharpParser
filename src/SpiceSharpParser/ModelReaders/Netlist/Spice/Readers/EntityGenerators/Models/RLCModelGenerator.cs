using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class RLCModelGenerator : ModelGenerator
    {
        public override Context.Models.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "res":
                case "r":
                    var model = new ResistorModel(id);
                    SetParameters(context, model, parameters);
                    return new Context.Models.Model(id, model, model.Parameters);

                case "c":
                    var model2 = new CapacitorModel(id);
                    SetParameters(context, model2, parameters);
                    return new Context.Models.Model(id, model2, model2.Parameters);
            }

            return null;
        }
    }
}