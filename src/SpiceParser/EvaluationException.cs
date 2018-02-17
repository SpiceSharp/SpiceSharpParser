using System;

namespace SpiceParser
{
    public class EvaluationException : Exception
    {
        public EvaluationException(string message)
            : base(message)
        {
        }
    }
}
