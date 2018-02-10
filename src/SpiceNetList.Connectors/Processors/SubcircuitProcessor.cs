using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    class SubcircuitProcessor : StatementProcessor
    {
        public SubcircuitProcessor()
        {
        }

        public override void Init()
        {
        }

        public override void Process(Statement statement, NetList netlist)
        {
            var sub = statement as SubCircuit;
            netlist.Definitions.Add(sub);
        }
    }
}
