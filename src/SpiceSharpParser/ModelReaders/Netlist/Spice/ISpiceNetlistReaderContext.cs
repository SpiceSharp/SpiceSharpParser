using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice
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
