using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class SwitchModelGenerator : IModelGenerator
    {
        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice Types
        /// </returns>
        public IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "sw", "csw" };
            }
        }

        public SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            SpiceSharp.Components.Model model = null;

            switch (type)
            {
                case "sw": model = new VoltageSwitchModel(name);break;
                case "csw": model = new CurrentSwitchModel(name);break;
            }
            if (model != null)
            {
                context.SetParameters(model, parameters, true);
            }
            return model;
        }
    }
}
