using System.Collections.Generic;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    public interface IModelGenerator
    {
        IEnumerable<string> GeneratedTypes { get; }

        SpiceSharp.Components.Model Generate(string name, string type, ParameterCollection parameters, IReadingContext context);
    }
}
