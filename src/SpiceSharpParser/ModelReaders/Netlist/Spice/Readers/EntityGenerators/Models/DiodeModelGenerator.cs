using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class DiodeModelGenerator : ModelGenerator
    {
        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public override IEnumerable<string> GetGeneratedTypes()
        {
            return new List<string>() { "d" };
        }

        protected override Entity GenerateModel(string name, string type)
        {
            return new DiodeModel(name);
        }
    }
}
