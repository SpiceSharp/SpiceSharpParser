using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class RLCModelGenerator : ModelGenerator
    {
        /// <summary>
        /// Gets generated SPICE types by generator.
        /// </summary>
        /// <returns>
        /// Generated SPICE types.
        /// </returns>
        public override IEnumerable<string> GetGeneratedTypes()
        {
            return new List<string>() { "r", "c" };
        }

        protected override Entity GenerateModel(string name, string type)
        {
            switch (type)
            {
                case "r": return new ResistorModel(name);
                case "c": return new CapacitorModel(name);
            }

            return null;
        }
    }
}
