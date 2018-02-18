using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;
using SpiceSharp;

namespace SpiceNetlist.SpiceSharpConnector
{
    /// <summary>
    /// Translates a netlist in Netlist Object Model to the netlist for SpiceSharp
    /// </summary>
    public class Connector
    {
        public Connector()
        {
            Controls = new ControlRegistry();
            Components = new EntityGeneratorRegistry();
            Models = new EntityGeneratorRegistry();
            Waveforms = new WaveformRegistry();
            StatementsProcessor = new StatementsProcessor(Models, Components, Controls, Waveforms);

            InitRegistries();
        }

        /// <summary>
        /// Gets registry of supported waveforms
        /// </summary>
        public WaveformRegistry Waveforms { get; }

        /// <summary>
        /// Gets registry of supported controls
        /// </summary>
        public ControlRegistry Controls { get; }

        /// <summary>
        /// Gets registry of supported components
        /// </summary>
        public EntityGeneratorRegistry Components { get; }

        /// <summary>
        /// Gets registry of supported models
        /// </summary>
        public EntityGeneratorRegistry Models { get; }

        /// <summary>
        /// Gets main processor
        /// </summary>
        protected StatementsProcessor StatementsProcessor { get; }

        /// <summary>
        /// Init registries
        /// </summary>
        protected virtual void InitRegistries()
        {
            Waveforms.Add(new SineGenerator());
            Waveforms.Add(new PulseGenerator());

            Models.Add(new RLCModelGenerator());
            Models.Add(new DiodeModelGenerator());
            Models.Add(new BipolarModelGenerator());
            Models.Add(new SwitchModelGenerator());

            Controls.Add(new ParamControl());
            Controls.Add(new OptionControl());
            Controls.Add(new TransientControl());
            Controls.Add(new ACControl());
            Controls.Add(new DCControl());
            Controls.Add(new OPControl());
            Controls.Add(new NoiseControl());
            Controls.Add(new SaveControl());
            Controls.Add(new ICControl());

            Components.Add(new RLCGenerator());
            Components.Add(new VoltageSourceGenerator(StatementsProcessor.WaveformProcessor));
            Components.Add(new CurrentSourceGenerator(StatementsProcessor.WaveformProcessor));
            Components.Add(new SwitchGenerator());
            Components.Add(new BipolarJunctionTransistorGenerator());
            Components.Add(new DiodeGenerator());
            Components.Add(new SubCircuitGenerator(StatementsProcessor.ComponentProcessor, StatementsProcessor.ModelProcessor));
        }

        /// <summary>
        /// Translates Netlist object mode to SpiceSharp netlist
        /// </summary>
        public NetList Translate(SpiceNetlist.NetList netlist)
        {
            NetList result = new NetList
            {
                Circuit = new Circuit(),
                Title = netlist.Title
            };

            var processingContext = new ProcessingContext(string.Empty, result);
            StatementsProcessor.Process(netlist.Statements, processingContext);
            return result;
        }
    }
}
