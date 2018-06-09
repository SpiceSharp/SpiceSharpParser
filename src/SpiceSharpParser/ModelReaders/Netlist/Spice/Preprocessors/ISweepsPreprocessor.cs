using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Preprocessors
{
    public interface ISweepsPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel);
    }
}