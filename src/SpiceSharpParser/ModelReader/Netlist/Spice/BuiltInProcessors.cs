using SpiceSharpParser.ModelReader.Netlist.Spice.Processors;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Exporters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Simulations;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.EntityGenerators.Components;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.EntityGenerators.Components.Sources;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.EntityGenerators.Models;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Waveforms;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice
{
    public class BuiltInProcessors
    {
        public static IStatementsProcessor Default
        {
            get
            {
                // Create registries
                var controls = new ControlRegistry();
                var components = new EntityGeneratorRegistry();
                var models = new EntityGeneratorRegistry();
                var waveForms = new WaveformRegistry();
                var exporters = new ExporterRegistry();

                var statementsProcessor = new StatementsProcessor(models, components, controls, waveForms, exporters);

                // Register waveform generators
                waveForms.Add(new SineGenerator());
                waveForms.Add(new PulseGenerator());

                // Register exporters
                exporters.Add(new VoltageExporter());
                exporters.Add(new CurrentExporter());
                exporters.Add(new PropertyExporter());

                // Register model generators
                models.Add(new RLCModelGenerator());
                models.Add(new DiodeModelGenerator());
                models.Add(new BipolarModelGenerator());
                models.Add(new SwitchModelGenerator());
                models.Add(new MosfetModelGenerator());

                // Register controls
                controls.Add(new ParamControl());
                controls.Add(new GlobalControl());
                controls.Add(new OptionControl());
                controls.Add(new TempControl());
                controls.Add(new StControl());
                controls.Add(new TransientControl());
                controls.Add(new ACControl());
                controls.Add(new DCControl());
                controls.Add(new OPControl());
                controls.Add(new NoiseControl());
                controls.Add(new LetControl());
                controls.Add(new SaveControl(exporters));
                controls.Add(new PlotControl(exporters));
                controls.Add(new ICControl());
                controls.Add(new NodeSetControl());

                // Register component generators
                components.Add(new RLCGenerator());
                components.Add(new VoltageSourceGenerator(statementsProcessor.WaveformProcessor));
                components.Add(new CurrentSourceGenerator(statementsProcessor.WaveformProcessor));
                components.Add(new SwitchGenerator());
                components.Add(new BipolarJunctionTransistorGenerator());
                components.Add(new DiodeGenerator());
                components.Add(new MosfetGenerator());
                components.Add(new SubCircuitGenerator(
                    statementsProcessor.ComponentProcessor,
                    statementsProcessor.ModelProcessor,
                    statementsProcessor.ControlProcessor,
                    statementsProcessor.SubcircuitDefinitionProcessor));

                return statementsProcessor;
            }
        }
    }
}
