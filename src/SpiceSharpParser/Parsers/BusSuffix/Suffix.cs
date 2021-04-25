using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.BusSuffix
{
    public class Suffix
    {
        public List<SuffixDimension> Dimensions { get; set; } = new List<SuffixDimension>();

        public string Name { get; set; }
    }
}
