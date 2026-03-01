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
                    var resistorModel = new ResistorModel(id);
                    var resistorContextModel = new Context.Models.Model(id, resistorModel, resistorModel.Parameters);
                    SetParameters(context, resistorModel, resistorContextModel, parameters);
                    return resistorContextModel;

                case "c":
                    var capacitorModel = new CapacitorModel(id);
                    var capacitorContextModel = new Context.Models.Model(id, capacitorModel, capacitorModel.Parameters);
                    SetParameters(context, capacitorModel, capacitorContextModel, parameters);
                    return capacitorContextModel;
            }

            return null;
        }
    }
}