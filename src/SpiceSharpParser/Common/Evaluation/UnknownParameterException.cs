using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class UnknownParameterException : Exception
    {
        /// <summary>
        /// Gets or set the name of unknown parameter.
        /// </summary>
        public string Name { get; set; }
    }
}
