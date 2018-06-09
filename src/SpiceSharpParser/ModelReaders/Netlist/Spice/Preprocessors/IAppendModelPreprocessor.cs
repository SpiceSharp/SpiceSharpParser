using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Preprocessors
{
    public interface IAppendModelPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel);
    }
}
