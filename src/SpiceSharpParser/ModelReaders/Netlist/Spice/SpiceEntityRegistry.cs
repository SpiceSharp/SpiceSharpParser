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
    public class SpiceEntityRegistry : ISpiceEntityRegistry
    {
        public SpiceEntityRegistry()
        {
            Controls = new ControlRegistry();
            Components = new EntityGeneratorRegistry();
            Models = new ModelGeneratorRegistry();
            WaveForms = new WaveformRegistry();
            Exporters = new ExporterRegistry();

            // Register waveform generators
            WaveForms.Bind("sine", new SineGenerator());
            WaveForms.Bind("pulse", new PulseGenerator());

            // Register exporters
            Exporters.Bind(new string[] { "v", "vi", "vr", "vm", "vdb", "vph", "vp" }, new VoltageExporter());
            Exporters.Bind(new string[] { "i", "ir", "ii", "im", "idb", "ip" }, new CurrentExporter());
            Exporters.Bind("@", new PropertyExporter());

            // Register model generators
            Models.Bind(new string[] { "r", "l", "c", "k" }, new RLCModelGenerator());
            Models.Bind("d", new DiodeModelGenerator());
            Models.Bind(new string[] { "npn", "pnp" }, new BipolarModelGenerator());
            Models.Bind(new string[] { "sw", "cs" }, new SwitchModelGenerator());
            Models.Bind(new string[] { "pmos", "nmos" }, new MosfetModelGenerator());

            // Register controls
            Controls.Bind("st_r", new StRegisterControl());
            Controls.Bind("step_r", new StepRegisterControl());
            Controls.Bind("param", new ParamControl());
            Controls.Bind("func", new FuncControl());
            Controls.Bind("global", new GlobalControl());
            Controls.Bind("connect", new ConnectControl());
            Controls.Bind("options", new OptionsControl());
            Controls.Bind("temp", new TempControl());
            Controls.Bind("st", new StControl());
            Controls.Bind("step", new StepControl());
            Controls.Bind("mc", new McControl());
            Controls.Bind("tran", new TransientControl());
            Controls.Bind("ac", new ACControl());
            Controls.Bind("dc", new DCControl());
            Controls.Bind("op", new OPControl());
            Controls.Bind("noise", new NoiseControl());
            Controls.Bind("let", new LetControl());
            Controls.Bind("save", new SaveControl(Exporters));
            Controls.Bind("plot", new PlotControl(Exporters));
            Controls.Bind("print", new PrintControl(Exporters));
            Controls.Bind("ic", new ICControl());
            Controls.Bind("nodeset", new NodeSetControl());

            // Register component generators
            Components.Bind(new string[] { "r", "l", "c", "k" }, new RLCGenerator());
            Components.Bind(new string[] { "v", "h", "e" }, new VoltageSourceGenerator());
            Components.Bind(new string[] { "i", "g", "f" }, new CurrentSourceGenerator());
            Components.Bind(new string[] { "s", "w" }, new SwitchGenerator());
            Components.Bind("q", new BipolarJunctionTransistorGenerator());
            Components.Bind("d", new DiodeGenerator());
            Components.Bind("m", new MosfetGenerator());
            Components.Bind("x", new SubCircuitGenerator());
        }

        public IRegistry<BaseControl> Controls { get; set; }

        public IRegistry<WaveformGenerator> WaveForms { get; set; }

        public IRegistry<Exporter> Exporters { get; set; }

        public IRegistry<EntityGenerator> Components { get; set; }

        public IRegistry<ModelGenerator> Models { get; set; }
    }
}
