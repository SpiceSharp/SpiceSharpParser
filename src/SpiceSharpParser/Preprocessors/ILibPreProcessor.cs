using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface ILibPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel, string currentDirectoryPath = null);
    }
}
