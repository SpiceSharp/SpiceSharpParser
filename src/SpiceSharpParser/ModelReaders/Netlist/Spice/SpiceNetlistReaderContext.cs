using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice
{
    public class SpiceNetlistReaderContext : ISpiceNetlistReaderContext
    {
        public SpiceNetlistReaderContext()
        {
            Controls = new ControlRegistry();
            Components = new EntityGeneratorRegistry();
            Models = new EntityGeneratorRegistry();
            WaveForms = new WaveformRegistry();
            Exporters = new ExporterRegistry();

            var componentReader = new ComponentReader(Components);
            var modelReader = new Readers.ModelReader(Models);
            var waveFormReader = new WaveformReader(WaveForms);
            var subcircuitReader = new SubcircuitDefinitionReader();
            var controlReader = new ControlReader(Controls);
            var commentReader = new CommentReader();

            // Create main reader
            var statementsReader = new StatementsReader(
                    new IStatementReader[]
                    {
                            controlReader,
                            componentReader,
                            modelReader,
                            subcircuitReader,
                            commentReader,
                    },
                    new IRegistry[]
                    {
                            Controls,
                            Components,
                            Models,
                            WaveForms,
                            Exporters,
                    },
                    new StatementsOrderer(Controls));

            // Register waveform generators
            WaveForms.Add(new SineGenerator());
            WaveForms.Add(new PulseGenerator());

            // Register exporters
            Exporters.Add(new VoltageExporter());
            Exporters.Add(new CurrentExporter());
            Exporters.Add(new PropertyExporter());

            // Register model generators
            Models.Add(new RLCModelGenerator());
            Models.Add(new DiodeModelGenerator());
            Models.Add(new BipolarModelGenerator());
            Models.Add(new SwitchModelGenerator());
            Models.Add(new MosfetModelGenerator());

            // Register controls
            Controls.Add(new StRegisterControl());
            Controls.Add(new StepRegisterControl());
            Controls.Add(new ParamControl());
            Controls.Add(new FuncControl());
            Controls.Add(new GlobalControl());
            Controls.Add(new OptionControl());
            Controls.Add(new TempControl());
            Controls.Add(new StControl());
            Controls.Add(new StepControl());
            Controls.Add(new TransientControl());
            Controls.Add(new ACControl());
            Controls.Add(new DCControl());
            Controls.Add(new OPControl());
            Controls.Add(new NoiseControl());
            Controls.Add(new LetControl());
            Controls.Add(new SaveControl(Exporters));
            Controls.Add(new PlotControl(Exporters));
            Controls.Add(new PrintControl(Exporters));
            Controls.Add(new ICControl());
            Controls.Add(new NodeSetControl());

            // Register component generators
            Components.Add(new RLCGenerator());
            Components.Add(new VoltageSourceGenerator(waveFormReader));
            Components.Add(new CurrentSourceGenerator(waveFormReader));
            Components.Add(new SwitchGenerator());
            Components.Add(new BipolarJunctionTransistorGenerator());
            Components.Add(new DiodeGenerator());
            Components.Add(new MosfetGenerator());
            Components.Add(new SubCircuitGenerator(
                componentReader,
                modelReader,
                controlReader,
                subcircuitReader));

            StatementsReader = statementsReader;
        }

        public IControlRegistry Controls { get; }

        public IWaveformRegistry WaveForms { get; }

        public IExporterRegistry Exporters { get; }

        public IEntityGeneratorRegistry Components { get; }

        public IEntityGeneratorRegistry Models { get; }

        public IStatementsReader StatementsReader { get; }

        public void Read(Statements statements, IReadingContext readingContext)
        {
            StatementsReader.Read(statements, readingContext);
        }
    }
}
