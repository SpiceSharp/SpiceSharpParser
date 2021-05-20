using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Distributed;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;

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
            Waveforms.Map("SINE", new SineGenerator());
            Waveforms.Map("SIN", new SineGenerator());
            Waveforms.Map("PULSE", new PulseGenerator());
            Waveforms.Map("PWL", new PwlGenerator());
            Waveforms.Map("AM", new AMGenerator());
            Waveforms.Map("SFFM", new SFFMGenerator());
            Waveforms.Map("WAVE", new WaveGenerator());
            Waveforms.Map("wavefile", new WaveGenerator());

            // Register exporters
            Exporters.Map(new[] { "V", "VI", "VR", "VM", "VDB", "VPH", "VP" }, new VoltageExporter());
            Exporters.Map(new[] { "I", "IR", "II", "IM", "IDB", "IP" }, new CurrentExporter());
            Exporters.Map("@", new PropertyExporter());

            // Register model generators
            Models.Map(new[] { "R", "C", "RES" }, new RLCModelGenerator());
            Models.Map("D", new DiodeModelGenerator());
            Models.Map(new[] { "NPN", "PNP" }, new BipolarModelGenerator());
            Models.Map(new[] { "SW", "CSW", "VSWITCH", "ISWITCH" }, new SwitchModelGenerator());
            Models.Map(new[] { "PMOS", "NMOS" }, new MosfetModelGenerator());
            Models.Map(new[] { "NJF", "PJF" }, new JFETModelGenerator());

            // Register controls
            Controls.Map("ST_R", new StRegisterControl());
            Controls.Map("STEP_R", new StepRegisterControl());
            Controls.Map("PARAM", new ParamControl());
            Controls.Map("SPARAM", new SParamControl());
            Controls.Map("FUNC", new FuncControl());
            Controls.Map("GLOBAL", new GlobalControl());
            Controls.Map("CONNECT", new ConnectControl());
            Controls.Map("DISTRIBUTION", new DistributionControl());
            Controls.Map("OPTIONS", new OptionsControl());
            Controls.Map("TEMP", new TempControl());
            Controls.Map("ST", new StControl());
            Controls.Map("STEP", new StepControl());
            Controls.Map("MC", new McControl());
            Controls.Map("TRAN", new TransientControl(Exporters));
            Controls.Map("AC", new ACControl(Exporters));
            Controls.Map("DC", new DCControl(Exporters));
            Controls.Map("OP", new OPControl(Exporters));
            Controls.Map("NOISE", new NoiseControl(Exporters));
            Controls.Map("LET", new LetControl());
            Controls.Map("SAVE", new SaveControl(Exporters, new ExportFactory()));
            Controls.Map("PLOT", new PlotControl(Exporters, new ExportFactory()));
            Controls.Map("PRINT", new PrintControl(Exporters, new ExportFactory()));
            Controls.Map("IC", new ICControl());
            Controls.Map("NODESET", new NodeSetControl());
            Controls.Map("WAVE", new WaveControl(Exporters, new ExportFactory()));

            // Register component generators
            Components.Map(new[] { "R", "L", "C", "K" }, new RLCKGenerator());
            Components.Map(new[] { "B" }, new ArbitraryBehavioralGenerator());
            Components.Map(new[] { "V", "H", "E" }, new VoltageSourceGenerator());
            Components.Map(new[] { "I", "G", "F" }, new CurrentSourceGenerator());
            Components.Map(new[] { "S", "W" }, new SwitchGenerator());
            Components.Map("Q", new BipolarJunctionTransistorGenerator());
            Components.Map("D", new DiodeGenerator());
            Components.Map("M", new MosfetGenerator());
            Components.Map("X", new SubCircuitGenerator());
            Components.Map("J", new JFETGenerator());
            Components.Map("T", new LosslessTransmissionLineGenerator());
            Components.Map("BVDelay", new VoltageDelayGenerator());
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