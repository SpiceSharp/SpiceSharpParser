namespace SpiceNetlist.SpiceSharpConnector
{
    public class Connector
    {
        public NetList Translate(SpiceNetlist.NetList netlist)
        {
            var processor = new Processor();
            return processor.Process(netlist);
        }
    }
}
