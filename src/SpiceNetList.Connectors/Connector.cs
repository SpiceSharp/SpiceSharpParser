using SpiceSharp;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class Connector
    {
        public NetList Translate(SpiceNetlist.NetList netlist)
        {
            NetList result = new NetList
            {
                Circuit = new Circuit(),
                Title = netlist.Title
            };

            var processor = new Processor();
            var processingContext = new ProcessingContext(string.Empty, result);

            processor.Process(netlist.Statements, processingContext);

            return result;
        }
    }
}
