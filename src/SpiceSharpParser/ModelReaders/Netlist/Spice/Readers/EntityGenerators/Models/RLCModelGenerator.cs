using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class RLCModelGenerator : ModelGenerator
    {
        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "r", "c" };
        }

        internal override Entity GenerateModel(string name, string type)
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
