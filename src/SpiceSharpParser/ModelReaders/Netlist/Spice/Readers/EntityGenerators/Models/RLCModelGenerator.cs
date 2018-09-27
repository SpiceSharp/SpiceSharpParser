using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class RLCModelGenerator : ModelGenerator
    {
        public override IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "r", "c" };
            }
        }

        public override SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            SpiceSharp.Components.Model model = null;

            switch (type)
            {
                case "r": model = new ResistorModel(name); break;
                case "c": model = new CapacitorModel(name);break;
            }

            if (model != null)
            {
                SetParameters(context, model, parameters, true);
            }

            return model;
        }
    }
}
