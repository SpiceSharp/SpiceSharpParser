using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceNetlistValidationResult
    {
        public SpiceNetlistValidationResult()
        {
            Exceptions = new List<SpiceSharpParserException>();
        }

        public List<SpiceSharpParserException> Exceptions { get; set; }

        public bool IsValid => Exceptions.Any() == false;
    }
}
