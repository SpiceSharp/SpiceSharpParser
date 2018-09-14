using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class UnknownParameterException : Exception
    {
        /// <summary>
        /// Gets or sets the name of unknown parameter.
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return "Unknown parameter: " + Name;
        }
    }
}
