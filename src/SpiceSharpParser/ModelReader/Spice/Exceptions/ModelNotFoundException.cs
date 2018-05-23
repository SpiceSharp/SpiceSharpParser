using System;

namespace SpiceSharpParser.ModelReader.Spice.Exceptions
{
    /// <summary>
    ///  Exception thrown when the model for entity is not found
    /// </summary>
    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException()
        {
        }

        public ModelNotFoundException(string message)
            : base(message)
        {
        }

        public ModelNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
