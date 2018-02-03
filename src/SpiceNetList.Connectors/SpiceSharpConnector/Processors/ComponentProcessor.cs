using SpiceNetlist.SpiceObjects;
using SpiceNetlist.Connectors.SpiceSharpConnector.Generators.Components;
using SpiceSharp.Circuits;

namespace SpiceNetlist.Connectors.SpiceSharpConnector.Processors
{
    class ComponentProcessor : StatementProcessor
    {
        public override void Init()
        {
            Generators.Add(new RLCGenerator());
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
