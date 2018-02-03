using SpiceNetlist.SpiceObjects;
using SpiceNetlist.Connectors.SpiceSharpConnector.Processors;
using System.Collections.Generic;

namespace SpiceNetlist.Connectors.SpiceSharpConnector
{
    public abstract class StatementProcessor
    {
        protected List<Generator> Generators = new List<Generator>();

        public abstract void Process(Statement statement, NetList netlist);

        public abstract void Init();
    }
}
