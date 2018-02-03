using SpiceNetlist.SpiceObjects;
using SpiceSharp.Circuits;
using SpiceNetlist.Connectors.SpiceSharpConnector.EntityGenerators.Components;
using System.Collections.Generic;
using SpiceNetlist.Connectors.SpiceSharpConnector.Processors.EntityGenerators.Components;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors
{
    class ComponentProcessor : StatementProcessor
    {
        protected List<EntityGenerator> Generators = new List<EntityGenerator>();

        public override void Init()
        {
            Generators.Add(new RLCGenerator());
            Generators.Add(new VoltageSourceGenerator());
        }

        public override void Process(Statement statement, NetList netlist)
        {
            Component c = statement as Component;
            string name = c.Name.ToLower();
            string type = name[0].ToString();

            foreach (var generator in Generators)
            {
                if (generator.GetGeneratedTypes().Contains(type))
                {
                    Entity entity = generator.Generate(name, type, c.Parameters, netlist);

                    if (entity != null)
                    {
                        netlist.Circuit.Objects.Add(entity);
                    }
                }
            }
        }
    }
}
