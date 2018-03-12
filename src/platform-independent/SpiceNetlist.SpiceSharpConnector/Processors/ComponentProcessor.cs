using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Registries;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processes all supported <see cref="Component"/> from spice netlist object model.
    /// </summary>
    public class ComponentProcessor : StatementProcessor<Component>
    {
        private ModelProcessor modelProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentProcessor"/> class.
        /// </summary>
        /// <param name="modelProcessor">A model processor</param>
        /// <param name="componentRegistry">A component registry</param>
        public ComponentProcessor(ModelProcessor modelProcessor, IEntityGeneratorRegistry componentRegistry)
        {
            this.modelProcessor = modelProcessor;
            ComponentRegistry = componentRegistry;
        }

        /// <summary>
        /// Gets the component registry
        /// </summary>
        public IEntityGeneratorRegistry ComponentRegistry { get; }

        /// <summary>
        /// Processes a component statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
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
