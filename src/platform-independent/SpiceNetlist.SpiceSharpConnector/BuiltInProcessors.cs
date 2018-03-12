using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Simulations;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Semiconductors;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;
using SpiceNetlist.SpiceSharpConnector.Registries;

namespace SpiceNetlist.SpiceSharpConnector
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

                var statementsProcessor = new StatementsProcessor(models, components, controls, waveForms);

                // Register waveform generators
                waveForms.Add(new SineGenerator());
                waveForms.Add(new PulseGenerator());

                // Register exporters
                exporters.Add(new VoltageExporter());
                exporters.Add(new CurrentExporter());

                // Register model generators
                models.Add(new RLCModelGenerator());
                models.Add(new DiodeModelGenerator());
                models.Add(new BipolarModelGenerator());
                models.Add(new SwitchModelGenerator());
                models.Add(new MosfetModelGenerator());

                // Register controls 
                controls.Add(new ParamControl());
                controls.Add(new OptionControl());
                controls.Add(new TransientControl());
                controls.Add(new ACControl());
                controls.Add(new DCControl());
                controls.Add(new OPControl());
                controls.Add(new NoiseControl());
                controls.Add(new SaveControl(exporters));
                controls.Add(new PlotControl(exporters));
                controls.Add(new ICControl());

                // Register component generators
                components.Add(new RLCGenerator());
                components.Add(new VoltageSourceGenerator(statementsProcessor.WaveformProcessor));
                components.Add(new CurrentSourceGenerator(statementsProcessor.WaveformProcessor));
                components.Add(new SwitchGenerator());
                components.Add(new BipolarJunctionTransistorGenerator());
                components.Add(new DiodeGenerator());
                components.Add(new MosfetGenerator());
                components.Add(new SubCircuitGenerator(statementsProcessor.ComponentProcessor, statementsProcessor.ModelProcessor));

                return statementsProcessor;
            }
        }
    }
}
