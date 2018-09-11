using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Preprocessors
{
    public interface ILibPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel, string currentDirectoryPath = null);
    }
}
