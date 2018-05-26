using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface IAppendModelPreProcessor
    {
        void Process(SpiceNetlist netlistModel);
    }
}
