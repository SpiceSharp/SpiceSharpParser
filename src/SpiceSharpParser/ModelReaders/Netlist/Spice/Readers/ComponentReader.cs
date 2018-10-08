using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
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
        /// <param name="mapper">A component mapper.</param>
        public ComponentReader(IMapper<IComponentGenerator> mapper)
        {
            Mapper = mapper ?? throw new System.NullReferenceException(nameof(mapper));
        }

        /// <summary>
        /// Gets the component mapper.
        /// </summary>
        public IMapper<IComponentGenerator> Mapper { get; }

        /// <summary>
        /// Reads a component statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Component statement, IReadingContext context)
        {
            string componentName = statement.Name;
            string componentType = componentName[0].ToString();

            if (!Mapper.Contains(componentType, context.CaseSensitivity.IsEntityNameCaseSensitive))
            {
                throw new System.Exception("Unsupported component type");
            }

            var generator = Mapper.Get(componentType, context.CaseSensitivity.IsEntityNameCaseSensitive);

            Entity entity = generator.Generate(
                context.ComponentNameGenerator.Generate(componentName),
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
