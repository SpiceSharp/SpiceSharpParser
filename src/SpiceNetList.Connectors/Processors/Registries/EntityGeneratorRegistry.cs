using System;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class EntityGeneratorRegistry
    {
        private List<EntityGenerator> generators = new List<EntityGenerator>();
        private Dictionary<string, EntityGenerator> generatorsByType = new Dictionary<string, EntityGenerator>();

        public EntityGeneratorRegistry()
        {
        }

        public void Add(EntityGenerator generator)
        {
            foreach (var type in generator.GetGeneratedTypes())
            {
                if (generatorsByType.ContainsKey(type))
                {
                    throw new Exception("Conflict in geneators");
                }

                generatorsByType[type] = generator;
            }

            generators.Add(generator);
        }

        public bool Supports(string type)
        {
            return generatorsByType.ContainsKey(type);
        }

        internal EntityGenerator GetGenerator(string type)
        {
            return generatorsByType[type];
        }
    }
}
