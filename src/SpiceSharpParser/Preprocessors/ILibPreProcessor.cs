using SpiceSharpParser.Model.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface ILibPreProcessor
    {
        void Process(Netlist netlistModel, string currentDirectoryPath = null);
    }
}
