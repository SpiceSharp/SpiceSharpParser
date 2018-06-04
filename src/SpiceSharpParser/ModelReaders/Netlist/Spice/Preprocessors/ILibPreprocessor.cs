using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Preprocessors
{
    public interface ILibPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel, string currentDirectoryPath = null);
    }
}
