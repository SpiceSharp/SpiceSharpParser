using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public interface ISpiceReaderRegistry
    {
        IWaveformReader WaveformReader { get; set; }

        ISubcircuitDefinitionReader SubcircuitDefinitionReader { get; set; }

        IComponentReader ComponentReader { get; set; }

        IControlReader ControlReader { get; set; }

        IModelReader ModelReader { get; set; }

        StatementReader<CommentLine> CommentReader { get; set; }

    }
}
