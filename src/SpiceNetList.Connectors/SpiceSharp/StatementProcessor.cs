using SpiceNetlist.SpiceObjects;
using SpiceNetList.Connectors.SpiceSharp.Processors;
using System.Collections.Generic;

namespace SpiceNetList.Connectors.SpiceSharp
{
    public abstract class StatementProcessor
    {
        protected List<Generator> Generators = new List<Generator>();

        public abstract void Process(Statement statement, NetList netlist);

        public void Init()
        {
            RegisterGenerators();
        }

        protected abstract void RegisterGenerators();
    }
}
