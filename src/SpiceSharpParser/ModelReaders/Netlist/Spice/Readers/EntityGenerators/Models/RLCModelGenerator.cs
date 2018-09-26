using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class RLCModelGenerator : IModelGenerator
    {
        /// <summary>
        /// Gets generated SPICE types by generator.
        /// </summary>
        /// <returns>
        /// Generated SPICE types.
        /// </returns>
        public IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "r", "c" };
            }
        }

        public SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            SpiceSharp.Components.Model model = null;

            switch (type)
            {
                case "r": model = new ResistorModel(name); break;
                case "c": model = new CapacitorModel(name);break;
            }

            if (model != null)
            {
                context.SetParameters(model, parameters, true);
            }

            return model;
        }
    }
}
