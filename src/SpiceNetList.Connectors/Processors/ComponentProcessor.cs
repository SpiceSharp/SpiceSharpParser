using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ComponentProcessor : StatementProcessor
    {
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();

        public override void Init()
        {
            Generators.Add(new RLCGenerator());
            Generators.Add(new VoltageSourceGenerator(new EntityGenerators.Components.Waveforms.WaveformsGenerator()));
            Generators.Add(new BipolarJunctionTransistorGenerator());
            Generators.Add(new SubCircuitGenerator(this));
        }

        public override void Process(Statement statement, ProcessingContext context)
        {
            Component c = statement as Component;
            string name = c.Name.ToLower();
            string type = name[0].ToString();

            foreach (var generator in Generators)
            {
                if (generator.GetGeneratedTypes().Contains(type))
                {
                    Entity entity = generator.Generate(
                        new Identifier(context.GenerateObjectName(name)),
                        name,
                        type,
                        c.PinsAndParameters,
                        context);

                    if (entity != null)
                    {
                        context.AddEntity(entity);
                    }
                }
            }
        }
    }
}
