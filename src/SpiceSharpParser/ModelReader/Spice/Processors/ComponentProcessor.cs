using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Registries;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.ModelReader.Spice.Processors
{
    /// <summary>
    /// Processes all supported <see cref="Component"/> from spice netlist object model.
    /// </summary>
    public class ComponentProcessor : StatementProcessor<Component>, IComponentProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentProcessor"/> class.
        /// </summary>
        /// <param name="componentRegistry">A component registry</param>
        public ComponentProcessor(IEntityGeneratorRegistry componentRegistry)
        {
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
        public override void Process(Component statement, IProcessingContext context)
        {
            string componentName = statement.Name;
            string componentType = componentName[0].ToString().ToLower();

            if (!ComponentRegistry.Supports(componentType))
            {
                throw new System.Exception("Unsupported component type");
            }

            var generator = ComponentRegistry.Get(componentType);

            Entity entity = generator.Generate(
                new StringIdentifier(context.ObjectNameGenerator.Generate(componentName)),
                componentName,
                componentType,
                statement.PinsAndParameters,
                context);

            if (entity != null)
            {
                context.Result.AddEntity(entity);
            }
        }
    }
}
