using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    public interface IModelsGenerator
    {
        Model GenerateModel(IModelGenerator modelGenerator, string id, string originalName, string type, SpiceSharpParser.Models.Netlist.Spice.Objects.ParameterCollection parameters, IReadingContext context);
    }
}