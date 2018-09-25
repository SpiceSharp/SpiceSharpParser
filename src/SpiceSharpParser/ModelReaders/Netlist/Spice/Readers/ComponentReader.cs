using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Component"/> from SPICE netlist object model.
    /// </summary>
    public class ComponentReader : StatementReader<Component>, IComponentReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentReader"/> class.
        /// </summary>
        /// <param name="mapper">A component registry.</param>
        public ComponentReader(IMapper<EntityGenerator> mapper)
        {
            Mapper = mapper;
        }

        /// <summary>
        /// Gets the component mapper.
        /// </summary>
        public IMapper<EntityGenerator> Mapper { get; }

        /// <summary>
        /// Reads a component statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Read(Component statement, IReadingContext context)
        {
            string componentName = statement.Name;
            string componentType = componentName[0].ToString().ToLower();

            if (!Mapper.Contains(componentType))
            {
                throw new System.Exception("Unsupported component type");
            }

            var generator = Mapper.Get(componentType);

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
