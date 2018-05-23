using SpiceSharpParser.Model.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface IAppendModelPreProcessor
    {
        void Process(Netlist netlistModel);
    }
}
