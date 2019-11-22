using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    public interface IModelGenerator
    {
        SpiceSharp.Components.Model Generate(string id, string type, ParameterCollection parameters, ICircuitContext context);
    }
}