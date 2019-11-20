using System;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
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
            Mapper = mapper ?? throw new NullReferenceException(nameof(mapper));
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
        public override void Read(Component statement, ICircuitContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string componentName = statement.Name;

            IComponentGenerator generator = GetComponentGenerator(context, componentName, out string componentType);

            Entity entity = generator.Generate(
                context.NameGenerator.GenerateObjectName(componentName),
                componentName,
                componentType,
                statement.PinsAndParameters,
                context);

            if (entity != null)
            {
                context.Result.AddEntity(entity);
            }
        }

        private IComponentGenerator GetComponentGenerator(ICircuitContext context, string componentName, out string componentType)
        {
            foreach (var map in Mapper)
            {
                if (componentName.StartsWith(map.Key, context.CaseSensitivity.IsEntityNameCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
                {
                    componentType = map.Key;
                    return map.Value;
                }
            }

            throw new UnknownComponentException($"Unsupported component {componentName}");
        }
    }
}
