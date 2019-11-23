using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    public interface IComponentGenerator
    {
        SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context);
    }
}