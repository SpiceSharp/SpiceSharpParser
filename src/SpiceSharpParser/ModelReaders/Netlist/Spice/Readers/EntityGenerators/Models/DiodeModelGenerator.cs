using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Extensions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class DiodeModelGenerator : IModelGenerator
    {
        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice types
        /// </returns>
        public IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "d" };
            }
        }

        public SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            var model = new DiodeModel(name);
            context.SetParameters(model, parameters, true);
            return model;
        }
    }
}
