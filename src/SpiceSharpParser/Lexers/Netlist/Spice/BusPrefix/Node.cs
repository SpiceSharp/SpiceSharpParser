using System.Collections.Generic;

namespace SpiceSharpParser.Lexers.Netlist.Spice.BusPrefix
{
    public abstract class Node
    {
    }

    public class Prefix : Node
    {
        public int? Value { get; set; }

        public IEnumerable<Node> Nodes { get; set; }
    }

    public class PrefixNodeName : Node
    {
        public string Name { get; set; }
    }
}
