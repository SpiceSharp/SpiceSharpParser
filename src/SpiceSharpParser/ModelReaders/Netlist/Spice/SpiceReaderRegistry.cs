using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceReaderRegistry : ISpiceReaderRegistry
    {
        public SpiceReaderRegistry(
            IRegistry<WaveformGenerator> waveformRegistry,
            IRegistry<BaseControl> controlRegistry,
            IRegistry<ModelGenerator> modelsRegistry,
            IRegistry<EntityGenerator> componentRegistry)
        {
            WaveformReader = new WaveformReader(waveformRegistry);
            ComponentReader = new ComponentReader(componentRegistry);
            ModelReader = new ModelReader(modelsRegistry);
            ControlReader = new ControlReader(controlRegistry);
            SubcircuitDefinitionReader = new SubcircuitDefinitionReader();
            CommentReader = new CommentReader();
        }

        public IWaveformReader WaveformReader { get; set; }

        public ISubcircuitDefinitionReader SubcircuitDefinitionReader { get; set; }

        public IComponentReader ComponentReader { get; set; }

        public IControlReader ControlReader { get; set; }

        public IModelReader ModelReader { get; set; }

        public StatementReader<CommentLine> CommentReader { get; set; }
    }
}
