using System.Collections.Generic;

namespace SpiceSharpParser.Lexers.Netlist.Spice.BusSuffix
{
    public class Sufix
    {
        public List<SufixDimension> Dimensions { get; set; } = new List<SufixDimension>();

        public string Name { get; set; }
    }

    public class SufixDimension
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
    }

    public abstract class Node
    {
    }

    public class NumberNode : Node
    {
        public int Node { get; set; }
    }

    public class RangeNode : Node
    {
        public int Start { get; set; }

        public int Stop { get; set; }

        public int? Step { get; set; }

        public int? Multiply { get; set; }
    }

}
