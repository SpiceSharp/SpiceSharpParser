using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ModelProcessor : StatementProcessor<Model>
    {
        public ModelProcessor(EntityGeneratorRegistry registry)
        {
            Registry = registry;
        }

        public EntityGeneratorRegistry Registry { get; }

        public override void Process(Model statement, ProcessingContext context)
        {
            string name = statement.Name.ToLower();
            if (statement.Parameters.Count > 0)
            {
                if (statement.Parameters[0] is BracketParameter b)
                {
                    var type = b.Name.ToLower();

                    if (!Registry.Supports(type))
                    {
                        throw new System.Exception("Unsupported model type");
                    }

                    var generator = Registry.GetGenerator(type);

                    Entity spiceSharpModel = generator.Generate(
                        new SpiceSharp.Identifier(context.GenerateObjectName(name)),
                        name,
                        type,
                        b.Parameters,
                        context);

                    if (statement != null)
                    {
                        context.AddEntity(spiceSharpModel);
                    }
                }

                if (statement.Parameters[0] is SingleParameter single)
                {
                    var type = single.Image;

                    if (!Registry.Supports(type))
                    {
                        throw new System.Exception("Unsupported model type");
                    }

                    var generator = Registry.GetGenerator(type);

                    Entity spiceSharpModel = generator.Generate(new SpiceSharp.Identifier(context.GenerateObjectName(name)), name, type, statement.Parameters.Skip(1), context);

                    if (statement != null)
                    {
                        context.AddEntity(spiceSharpModel);
                    }
                }
            }
        }
    }
}
