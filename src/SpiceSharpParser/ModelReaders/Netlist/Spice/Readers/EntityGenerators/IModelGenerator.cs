using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    public interface IModelGenerator
    {
        Model Generate(string id, string type, SpiceSharpParser.Models.Netlist.Spice.Objects.ParameterCollection parameters, IReadingContext context);
    }
}