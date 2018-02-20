using SpiceNetlist.SpiceObjects;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ComponentProcessor : StatementProcessor<Component>
    {
        private ModelProcessor modelProcessor;

        public ComponentProcessor(ModelProcessor modelProcessor, WaveformProcessor waveformGenerator, EntityGeneratorRegistry componentRegistry)
        {
            this.modelProcessor = modelProcessor;
            ComponentRegistry = componentRegistry;
        }

        public EntityGeneratorRegistry ComponentRegistry { get; }

        public override void Process(Component statement, ProcessingContext context)
        {
            string componentName = statement.Name.ToLower();
            string componentType = componentName[0].ToString();

            if (!ComponentRegistry.Supports(componentType))
            {
                throw new System.Exception("Unsupported component type");
            }

            var generator = ComponentRegistry.Get(componentType);

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
