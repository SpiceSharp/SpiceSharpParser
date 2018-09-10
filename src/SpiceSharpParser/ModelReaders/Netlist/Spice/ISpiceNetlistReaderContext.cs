using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public interface ISpiceNetlistReaderContext
    {
        IControlRegistry Controls { get; }

        IWaveformRegistry WaveForms { get; }

        IExporterRegistry Exporters { get; }

        IEntityGeneratorRegistry Components { get; }

        IEntityGeneratorRegistry Models { get; }

        void Read(Statements statements, IReadingContext readingContext);
    }
}
