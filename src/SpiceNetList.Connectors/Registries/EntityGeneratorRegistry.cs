using System;
using SpiceNetlist.SpiceSharpConnector.Processors;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class EntityGeneratorRegistry : BaseRegistry<EntityGenerator>
    {
        public EntityGeneratorRegistry()
        {
        }

        public override void Add(EntityGenerator generator)
        {
            foreach (var type in generator.GetGeneratedTypes())
            {
                if (elementsByType.ContainsKey(type))
                {
                    throw new Exception("Conflict in geneators");
                }

                elementsByType[type] = generator;
            }

            elements.Add(generator);
        }
    }
}
