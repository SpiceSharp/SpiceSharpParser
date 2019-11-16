using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class UnknownParameterException : Exception
    {

        public UnknownParameterException(string name) : base($"Unknown parameter {name}")
        {
            Name = name;
        }
        /// <summary>
        /// Gets or sets the name of unknown parameter.
        /// </summary>
        public string Name { get; }
    }
}
