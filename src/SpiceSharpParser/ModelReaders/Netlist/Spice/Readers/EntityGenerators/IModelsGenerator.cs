using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    public interface IModelsGenerator
    {
        SpiceSharp.Components.Model GenerateModel(IModelGenerator modelGenerator, string id, string originalName, string type, ParameterCollection parameters, ICircuitContext context);
    }
}