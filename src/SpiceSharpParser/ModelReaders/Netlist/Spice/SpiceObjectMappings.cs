using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceObjectMappings : ISpiceObjectMappings
    {
        public SpiceObjectMappings()
        {
            Controls = new ControlMapper();
            Components = new EntityGeneratorMapper();
            Models = new ModelGeneratorMapper();
            Waveforms = new WaveformMapper();
            Exporters = new ExporterMapper();

            // Register waveform generators
            Waveforms.Map("sine", new SineGenerator());
            Waveforms.Map("pulse", new PulseGenerator());

            // Register exporters
            Exporters.Map(new string[] { "v", "vi", "vr", "vm", "vdb", "vph", "vp" }, new VoltageExporter());
            Exporters.Map(new string[] { "i", "ir", "ii", "im", "idb", "ip" }, new CurrentExporter());
            Exporters.Map("@", new PropertyExporter());

            // Register model generators
            Models.Map(new string[] { "r", "l", "c", "k" }, new RLCModelGenerator());
            Models.Map("d", new DiodeModelGenerator());
            Models.Map(new string[] { "npn", "pnp" }, new BipolarModelGenerator());
            Models.Map(new string[] { "sw", "cs" }, new SwitchModelGenerator());
            Models.Map(new string[] { "pmos", "nmos" }, new MosfetModelGenerator());

            // Register controls
            Controls.Map("st_r", new StRegisterControl());
            Controls.Map("step_r", new StepRegisterControl());
            Controls.Map("param", new ParamControl());
            Controls.Map("func", new FuncControl());
            Controls.Map("global", new GlobalControl());
            Controls.Map("connect", new ConnectControl());
            Controls.Map("options", new OptionsControl());
            Controls.Map("temp", new TempControl());
            Controls.Map("st", new StControl());
            Controls.Map("step", new StepControl());
            Controls.Map("mc", new McControl());
            Controls.Map("tran", new TransientControl());
            Controls.Map("ac", new ACControl());
            Controls.Map("dc", new DCControl());
            Controls.Map("op", new OPControl());
            Controls.Map("noise", new NoiseControl());
            Controls.Map("let", new LetControl());
            Controls.Map("save", new SaveControl(Exporters));
            Controls.Map("plot", new PlotControl(Exporters));
            Controls.Map("print", new PrintControl(Exporters));
            Controls.Map("ic", new ICControl());
            Controls.Map("nodeset", new NodeSetControl());

            // Register component generators
            Components.Map(new string[] { "r", "l", "c", "k" }, new RLCGenerator());
            Components.Map(new string[] { "v", "h", "e" }, new VoltageSourceGenerator());
            Components.Map(new string[] { "i", "g", "f" }, new CurrentSourceGenerator());
            Components.Map(new string[] { "s", "w" }, new SwitchGenerator());
            Components.Map("q", new BipolarJunctionTransistorGenerator());
            Components.Map("d", new DiodeGenerator());
            Components.Map("m", new MosfetGenerator());
            Components.Map("x", new SubCircuitGenerator());
        }

        /// <summary>
        /// Gets or sets the control mapper.
        /// </summary>
        public IMapper<BaseControl> Controls { get; set; }

        /// <summary>
        /// Gets or sets the waveform mapper.
        /// </summary>
        public IMapper<WaveformGenerator> Waveforms { get; set; }

        /// <summary>
        /// Gets or sets the exporter mapper.
        /// </summary>
        public IMapper<Exporter> Exporters { get; set; }

        /// <summary>
        /// Gets or sets the components mapper.
        /// </summary>
        public IMapper<IComponentGenerator> Components { get; set; }

        /// <summary>
        /// Gets or sets the models mapper.
        /// </summary>
        public IMapper<IModelGenerator> Models { get; set; }
    }
}
