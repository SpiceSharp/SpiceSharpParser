using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Connector.Registries;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.Connector.Processors
{
    /// <summary>
    /// Processes all supported <see cref="Model"/> from spice netlist object model.
    /// </summary>
    public class ModelProcessor : StatementProcessor<SpiceSharpParser.Model.SpiceObjects.Model>, IModelProcessor
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
        /// Processes a model statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public override void Process(SpiceSharpParser.Model.SpiceObjects.Model statement, IProcessingContext context)
        {
            string name = statement.Name;

            if (statement.Parameters.Count > 0)
            {
                if (statement.Parameters[0] is BracketParameter b)
                {
                    var type = b.Name.ToLower();

                    if (!Registry.Supports(type))
                    {
                        throw new GeneralConnectorException("Unsupported model type: " + type);
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
                        throw new GeneralConnectorException("Unsupported model type: " + type);
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
