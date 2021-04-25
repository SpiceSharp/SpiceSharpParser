namespace SpiceSharpParser.Parsers.BusSuffix
{
    public class RangeNode : Node
    {
        public int Start { get; set; }

        public int Stop { get; set; }

        public int? Step { get; set; }

        public int? Multiply { get; set; }
    }
}
