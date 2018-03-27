using System;
using System.Runtime.Serialization;

namespace SpiceNetlist.SpiceSharpConnector.Exceptions
{
    /// <summary>
    /// Exception thrown when there is something wrong with parameter of component
    /// </summary>
    public class WrongParameterException : Exception
    {
        public WrongParameterException()
        {
        }

        public WrongParameterException(string componentName, string message)
            : base(componentName + "-" + message)
        {
        }

        public WrongParameterException(string message)
            : base(message)
        {
        }

        public WrongParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected WrongParameterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
