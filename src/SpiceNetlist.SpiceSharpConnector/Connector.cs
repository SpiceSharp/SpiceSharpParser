using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Semiconductors;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;
using SpiceNetlist.SpiceSharpConnector.Registries;
using SpiceSharp;

namespace SpiceNetlist.SpiceSharpConnector
{
    /// <summary>
    /// Translates a netlist in Netlist Object Model to the netlist for SpiceSharp
    /// </summary>
    public class Connector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Connector"/> class.
        /// </summary>
        public Connector()
        {
            Controls = new ControlRegistry();
            Components = new EntityGeneratorRegistry();
            Models = new EntityGeneratorRegistry();
            Waveforms = new WaveformRegistry();
            Exporters = new ExporterRegistry();

            StatementsProcessor = new StatementsProcessor(Models, Components, Controls, Waveforms);

            InitRegistries();
        }

        /// <summary>
        /// Gets the registry of supported waveforms
        /// </summary>
        public WaveformRegistry Waveforms { get; }

        /// <summary>
        /// Gets the registry of supported controls
        /// </summary>
        public ControlRegistry Controls { get; }

        /// <summary>
        /// Gets the registry of supported components
        /// </summary>
        public EntityGeneratorRegistry Components { get; }

        /// <summary>
        /// Gets the registry of supported models
        /// </summary>
        public EntityGeneratorRegistry Models { get; }

        /// <summary>
        /// Gets the registry of supported exporters
        /// </summary>
        public ExporterRegistry Exporters { get; }

        /// <summary>
        /// Gets main processor
        /// </summary>
        protected StatementsProcessor StatementsProcessor { get; }

        /// <summary>
        /// Translates Netlist object mode to SpiceSharp netlist
        /// </summary>
        /// <param name="netlist">A object model of the netlist</param>
        /// <returns>
        /// A new SpiceSharp netlist
        /// </returns>
        public Netlist Translate(SpiceNetlist.Netlist netlist)
        {
            Netlist result = new Netlist(new Circuit(), netlist.Title);

            var processingContext = new ProcessingContext(string.Empty, result);
            StatementsProcessor.Process(netlist.Statements, processingContext);
            return result;
        }

        /// <summary>
        /// Init registries
        /// </summary>
        protected virtual void InitRegistries()
        {
            Waveforms.Add(new SineGenerator());
            Waveforms.Add(new PulseGenerator());

            Exporters.Add(new VoltageExporter());
            Exporters.Add(new CurrentExporter());

            Models.Add(new RLCModelGenerator());
            Models.Add(new DiodeModelGenerator());
            Models.Add(new BipolarModelGenerator());
            Models.Add(new SwitchModelGenerator());
            Models.Add(new MosfetModelGenerator());

            Controls.Add(new ParamControl());
            Controls.Add(new OptionControl());
            Controls.Add(new TransientControl());
            Controls.Add(new ACControl());
            Controls.Add(new DCControl());
            Controls.Add(new OPControl());
            Controls.Add(new NoiseControl());
            Controls.Add(new SaveControl(Exporters));
            Controls.Add(new ICControl());

            Components.Add(new RLCGenerator());
            Components.Add(new VoltageSourceGenerator(StatementsProcessor.WaveformProcessor));
            Components.Add(new CurrentSourceGenerator(StatementsProcessor.WaveformProcessor));
            Components.Add(new SwitchGenerator());
            Components.Add(new BipolarJunctionTransistorGenerator());
            Components.Add(new DiodeGenerator());
            Components.Add(new MosfetGenerator());
            Components.Add(new SubCircuitGenerator(StatementsProcessor.ComponentProcessor, StatementsProcessor.ModelProcessor));
        }
    }
}
