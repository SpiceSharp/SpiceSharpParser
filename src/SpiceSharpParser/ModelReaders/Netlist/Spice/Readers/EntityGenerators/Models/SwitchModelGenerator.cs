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
                case "sw":
                    var vsModel = new VoltageSwitchModel(id);
                    var vsContextModel = new Context.Models.Model(id, vsModel, vsModel.Parameters);
                    SetParameters(context, vsModel, parameters);
                    SetDimensionParameters(context, vsContextModel, parameters);
                    return vsContextModel;
                case "csw":
                    var csModel = new CurrentSwitchModel(id);
                    var csContextModel = new Context.Models.Model(id, csModel, csModel.Parameters);
                    SetParameters(context, csModel, parameters);
                    SetDimensionParameters(context, csContextModel, parameters);
                    return csContextModel;

                case "vswitch":
                    var vSwitchModel = new VSwitchModel(id);
                    var vSwitchContextModel = new Context.Models.Model(id, vSwitchModel, vSwitchModel.Parameters);
                    SetParameters(context, vSwitchModel, parameters);
                    SetDimensionParameters(context, vSwitchContextModel, parameters);
                    return vSwitchContextModel;

                case "iswitch":
                    var iSwitchModel = new ISwitchModel(id);
                    var iSwitchContextModel = new Context.Models.Model(id, iSwitchModel, iSwitchModel.Parameters);
                    SetParameters(context, iSwitchModel, parameters);
                    SetDimensionParameters(context, iSwitchContextModel, parameters);
                    return iSwitchContextModel;
            }

            return null;
        }
    }
}