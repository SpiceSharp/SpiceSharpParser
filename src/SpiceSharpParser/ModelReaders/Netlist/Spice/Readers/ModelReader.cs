using SpiceSharp.Circuits;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all supported <see cref="Model"/> from spice netlist object model.
    /// </summary>
    public class ModelReader : StatementReader<Model>, IModelReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelReader"/> class.
        /// </summary>
        /// <param name="registry">The registry</param>
        public ModelReader(IEntityGeneratorRegistry registry)
        {
            Registry = registry ?? throw new System.ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Gets the registry
        /// </summary>
        public IEntityGeneratorRegistry Registry { get; }

        /// <summary>
        /// Returns whether reader can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the reader can process given statement.
        /// </returns>
        public override bool CanRead(Statement statement)
        {
            return statement is Model;
        }

        /// <summary>
        /// Reads a model statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Read(Model statement, IReadingContext context)
        {
            string name = statement.Name;

            if (statement.Parameters.Count > 0)
            {
                if (statement.Parameters[0] is BracketParameter b)
                {
                    var type = b.Name.ToLower();

                    if (!Registry.Supports(type))
                    {
                        throw new GeneralReaderException("Unsupported model type: " + type);
                    }

                    var generator = Registry.Get(type);

                    Entity spiceSharpModel = generator.Generate(
                        new SpiceSharp.StringIdentifier(context.ObjectNameGenerator.Generate(name)),
                        name,
                        type,
                        b.Parameters,
                        context);

                    if (spiceSharpModel != null)
                    {
                        context.Result.AddEntity(spiceSharpModel);
                    }
                }

                if (statement.Parameters[0] is SingleParameter single)
                {
                    var type = single.Image.ToLower();

                    if (!Registry.Supports(type))
                    {
                        throw new GeneralReaderException("Unsupported model type: " + type);
                    }

                    var generator = Registry.Get(type);
                    Entity spiceSharpModel = generator.Generate(new SpiceSharp.StringIdentifier(context.ObjectNameGenerator.Generate(name)), name, type, statement.Parameters.Skip(1), context);

                    if (spiceSharpModel != null)
                    {
                        context.Result.AddEntity(spiceSharpModel);
                    }
                }
            }
        }
    }
}
