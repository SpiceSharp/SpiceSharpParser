using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface ILibPreProcessor
    {
        void Process(SpiceNetlist netlistModel, string currentDirectoryPath = null);
    }
}
