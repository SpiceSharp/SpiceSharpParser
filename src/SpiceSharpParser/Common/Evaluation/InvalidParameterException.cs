using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Common.Evaluation
{
    public class InvalidParameterException : SpiceSharpParserException
    {
        public InvalidParameterException(string name, SpiceLineInfo lineInfo)
            : base($"Invalid parameter {name}", lineInfo)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of unknown parameter.
        /// </summary>
        public string Name { get; }
    }
}