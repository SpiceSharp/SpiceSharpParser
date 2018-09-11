using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Preprocessors
{
    public interface IAppendModelPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel);
    }
}
