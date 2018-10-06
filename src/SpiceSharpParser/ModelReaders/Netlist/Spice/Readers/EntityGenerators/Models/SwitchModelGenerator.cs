using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class SwitchModelGenerator : ModelGenerator
    {
        public override IEnumerable<string> GeneratedTypes => new List<string>() { "sw", "csw" };

        public override SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            SpiceSharp.Components.Model model = null;

            switch (type.ToLower())
            {
                case "sw": model = new VoltageSwitchModel(id); break;
                case "csw": model = new CurrentSwitchModel(id); break;
            }

            if (model != null)
            {
                SetParameters(context, model, parameters);
            }

            return model;
        }
    }
}
