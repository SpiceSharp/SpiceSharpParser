using System;

namespace SpiceSharpParser.ModelReader.Spice.Exceptions
{
    /// <summary>
    /// Exception thrown when type of parameter for component is wrong
    /// </summary>
    public class WrongParameterTypeException : Exception
    {
        public WrongParameterTypeException()
        {
        }

        public WrongParameterTypeException(string componentName, string message)
            : base(componentName + "-" + message)
        {
        }

        public WrongParameterTypeException(string message)
            : base(message)
        {
        }

        public WrongParameterTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
