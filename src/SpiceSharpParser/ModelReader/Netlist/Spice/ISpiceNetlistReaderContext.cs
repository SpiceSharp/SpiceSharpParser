using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice
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
