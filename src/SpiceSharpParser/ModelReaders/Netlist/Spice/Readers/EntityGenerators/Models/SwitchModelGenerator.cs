using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class SwitchModelGenerator : ModelGenerator
    {
        public override IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "sw", "csw" };
            }
        }

        public override SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            SpiceSharp.Components.Model model = null;

            switch (type)
            {
                case "sw": model = new VoltageSwitchModel(name);break;
                case "csw": model = new CurrentSwitchModel(name);break;
            }
            if (model != null)
            {
                SetParameters(context, model, parameters, true);
            }
            return model;
        }
    }
}
