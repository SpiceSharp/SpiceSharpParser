using SpiceSharpParser.Model;

namespace SpiceSharpParser.Preprocessors
{
    public interface IAppendModelPreProcessor
    {
        void Process(Netlist netlistModel);
    }
}
