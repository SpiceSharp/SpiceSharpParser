using System;
using System.Runtime.Serialization;

namespace SpiceSharpParser.Connector.Exceptions
{
    /// <summary>
    /// General connector exception
    /// </summary>
    public class GeneralConnectorException : Exception
    {
        public GeneralConnectorException()
        {
        }

        public GeneralConnectorException(string message)
            : base(message)
        {
        }

        public GeneralConnectorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GeneralConnectorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
