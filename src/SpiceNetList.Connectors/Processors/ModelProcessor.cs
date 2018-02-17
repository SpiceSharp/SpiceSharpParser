using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ModelProcessor : StatementProcessor<Model>
    {
        // TODO: Refactor
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();
        protected Dictionary<string, EntityGenerator> GeneratorsByType = new Dictionary<string, EntityGenerator>();

        public ModelProcessor()
        {
            Generators.Add(new RLCModelGenerator());
            Generators.Add(new DiodeModelGenerator());
            Generators.Add(new BipolarModelGenerator());
            Generators.Add(new SwitchModelGenerator());

            foreach (var generator in Generators)
            {
                foreach (var type in generator.GetGeneratedTypes())
                {
                    GeneratorsByType[type] = generator;
                }
            }
        }

        public override void Process(Model statement, ProcessingContext context)
        {
            string name = statement.Name.ToLower();
            if (statement.Parameters.Count > 0)
            {
                if (statement.Parameters[0] is BracketParameter b)
                {
                    var type = b.Name.ToLower();

                    if (!GeneratorsByType.ContainsKey(type))
                    {
                        throw new System.Exception("Unsupported model type");
                    }

                    var generator = GeneratorsByType[type];

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

                    if (!GeneratorsByType.ContainsKey(type))
                    {
                        throw new System.Exception("Unsupported model type");
                    }

                    var generator = GeneratorsByType[type];

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
