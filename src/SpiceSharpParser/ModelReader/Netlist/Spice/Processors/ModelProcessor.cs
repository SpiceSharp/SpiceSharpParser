using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    /// <summary>
    /// Processes all supported <see cref="Model"/> from spice netlist object model.
    /// </summary>
    public class ModelProcessor : StatementProcessor<SpiceSharpParser.Model.Netlist.Spice.Objects.Model>, IModelProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelProcessor"/> class.
        /// </summary>
        /// <param name="registry">The registry</param>
        public ModelProcessor(IEntityGeneratorRegistry registry)
        {
            Registry = registry ?? throw new System.ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Gets the registry
        /// </summary>
        public IEntityGeneratorRegistry Registry { get; }

        /// <summary>
        /// Returns whether processor can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the processor can process given statement.
        /// </returns>
        public override bool CanProcess(Statement statement)
        {
            return statement is SpiceSharpParser.Model.Netlist.Spice.Objects.Model;
        }

        /// <summary>
        /// Processes a model statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Process(SpiceSharpParser.Model.Netlist.Spice.Objects.Model statement, IProcessingContext context)
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
                    var type = single.Image;

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
