using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Custom;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class SwitchModelGenerator : ModelGenerator
    {
        public override Context.Models.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "sw": var model = new VoltageSwitchModel(id);
                    SetParameters(context, model, parameters);
                    return new Context.Models.Model(id, model, model.Parameters);
                case "csw":
                    var model2 = new CurrentSwitchModel(id);
                    SetParameters(context, model2, parameters);
                    return new Context.Models.Model(id, model2, model2.Parameters);

                case "vswitch":
                    var vSwitchModel = new VSwitchModel(id);
                    SetParameters(context, vSwitchModel, parameters);
                    return new Context.Models.Model(id, vSwitchModel, vSwitchModel.Parameters);

                case "iswitch":
                    var iSwitchModel = new ISwitchModel(id);
                    SetParameters(context, iSwitchModel, parameters);
                    return new Context.Models.Model(id, iSwitchModel, iSwitchModel.Parameters);
            }

            return null;
        }
    }
}