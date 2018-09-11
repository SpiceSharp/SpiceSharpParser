using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Preprocessors
{
    public interface ISweepsPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel);
    }
}