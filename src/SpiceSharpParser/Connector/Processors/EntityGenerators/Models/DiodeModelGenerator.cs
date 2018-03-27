using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.Connector.Processors.EntityGenerators.Models
{
    public class DiodeModelGenerator : ModelGenerator
    {
        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "d" };
        }

        internal override Entity GenerateModel(string name, string type)
        {
            return new DiodeModel(name);
        }
    }
}
