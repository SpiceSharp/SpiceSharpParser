using SpiceNetlist.SpiceObjects;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceNetList.Connectors.SpiceSharp
{
    public class SpiceSharpConnector
    {
        public Tuple<Circuit,Simulation> Translate(SpiceNetlist.NetList netlist)
        {
            Circuit circuit = new Circuit();

            foreach (Statement statement in netlist.Statements.List)
            {
                var spiceSharpObject = Translate(statement);
                if (spiceSharpObject != null)
                {
                    circuit.Objects.Add(spiceSharpObject);
                }
            }

            return new Tuple<Circuit, Simulation>(circuit, null);
        }

        private Entity Translate(Statement statement)
        {
            //TODO Refactor ..........!!!!!!!
            if (statement is SpiceNetlist.SpiceObjects.Component c)
            {
                if (c.Name.StartsWith("r", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Resistor(c.Name);
                }

                if (c.Name.StartsWith("l", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Inductor(c.Name);
                }

                if (c.Name.StartsWith("c", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Capacitor(c.Name);
                }

                if (c.Name.StartsWith("m", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new MutualInductance(c.Name);
                }

                if (c.Name.StartsWith("v", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new VoltageSource(c.Name);
                }
            }

            return null;
        }
    }
}
