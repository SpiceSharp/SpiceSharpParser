using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface IAppendModelPreprocessor
    {
        void Preprocess(SpiceNetlist netlistModel);
    }
}
