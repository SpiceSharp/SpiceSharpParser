using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ComponentProcessor : StatementProcessor<Component>
    {
        protected EntityGeneratorRegistry registry = new EntityGeneratorRegistry();
        private ModelProcessor modelProcessor;

        public ComponentProcessor(ModelProcessor modelProcessor)
        {
            this.modelProcessor = modelProcessor;

            var waveFormsGenerators = new EntityGenerators.Components.Waveforms.WaveformsGenerator();
            registry.Add(new RLCGenerator());
            registry.Add(new VoltageSourceGenerator(waveFormsGenerators));
            registry.Add(new BipolarJunctionTransistorGenerator());
            registry.Add(new DiodeGenerator());
            registry.Add(new SubCircuitGenerator(this, modelProcessor));
            registry.Add(new CurrentSourceGenerator(waveFormsGenerators));
            registry.Add(new SwitchGenerator());
        }

        public override void Process(Component statement, ProcessingContext context)
        {
            string componentName = statement.Name.ToLower();
            string componentType = componentName[0].ToString();

            if (!registry.Supports(componentType))
            {
                throw new System.Exception("Unsupported component type");
            }

            var generator = registry.GetGenerator(componentType);

            Entity entity = generator.Generate(
                new Identifier(context.GenerateObjectName(componentName)),
                componentName,
                componentType,
                statement.PinsAndParameters,
                context);

            if (entity != null)
            {
                context.AddEntity(entity);
            }
        }
    }
}
