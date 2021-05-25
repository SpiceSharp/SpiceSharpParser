using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.BusPrefix
{
    public class Prefix : Node
    {
        public int? Value { get; set; }

        public IEnumerable<Node> Nodes { get; set; }
    }
}
