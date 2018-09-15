using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public interface ISpiceEntityRegistry
    {
        IRegistry<BaseControl> Controls { get; set; }

        IRegistry<WaveformGenerator> WaveForms { get; set; }

        IRegistry<Exporter> Exporters { get; set; }

        IRegistry<EntityGenerator> Components { get; set; }

        IRegistry<ModelGenerator> Models { get; set; }
    }
}
