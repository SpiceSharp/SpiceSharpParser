using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    class ModelProcessor : StatementProcessor
    {
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();

        public override void Init()
        {
            Generators.Add(new RLCModelGenerator());
            Generators.Add(new DiodeModelGenerator());
            Generators.Add(new BipolarModelGenerator());
            Generators.Add(new SwitchModelGenerator());
        }

        public override void Process(Statement statement, ProcessingContext context)
        {
            Model model = statement as Model;
            string name = model.Name.ToLower();
            if (model.Parameters.Count > 0)
            {
                if (model.Parameters[0] is ComplexParameter complex)
                {
                    var type = complex.Name.ToLower();

                    foreach (var generator in Generators)
                    {
                        if (generator.GetGeneratedTypes().Contains(type))
                        {
                            Entity spiceSharpModel = generator.Generate(new SpiceSharp.Identifier(context.GenerateObjectName(name)), name, type, complex.Parameters, context);

                            if (model != null)
                            {
                                context.AddEntity(spiceSharpModel);
                            }
                        }
                    }
                }

                if (model.Parameters[0] is SingleParameter single)
                {
                    var type = single.RawValue;
                    foreach (var generator in Generators)
                    {
                        if (generator.GetGeneratedTypes().Contains(type))
                        {
                            Entity spiceSharpModel = generator.Generate(new SpiceSharp.Identifier(context.GenerateObjectName(name)), name, type, model.Parameters.Skip(1), context);

                            if (model != null)
                            {
                                context.AddEntity(spiceSharpModel);
                            }
                        }
                    }
                }
            }
        }
    }
}
