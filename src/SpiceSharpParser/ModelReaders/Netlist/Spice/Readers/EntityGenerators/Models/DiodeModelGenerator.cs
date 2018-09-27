using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models
{
    public class DiodeModelGenerator : ModelGenerator
    {
        public override IEnumerable<string> GeneratedTypes
        {
            get
            {
                return new List<string>() { "d" };
            }
        }

        public override SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context)
        {
            var model = new DiodeModel(name);
            SetParameters(context, model, parameters);
            return model;
        }
    }
}
