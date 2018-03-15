using System;
using System.Runtime.Serialization;

namespace SpiceNetlist.SpiceSharpConnector.Exceptions
{
    /// <summary>
    /// Exception thrown when number of parameters for component is wrong
    /// </summary>
    public class WrongParametersCountException : Exception
    {
        public WrongParametersCountException()
        {
        }

        public WrongParametersCountException(string componentName, string message)
            : base(componentName + ":" + message)
        {
        }

        public WrongParametersCountException(string message)
            : base(message)
        {
        }

        public WrongParametersCountException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected WrongParametersCountException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
