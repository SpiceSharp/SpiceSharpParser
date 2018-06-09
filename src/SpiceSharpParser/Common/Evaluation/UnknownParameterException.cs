using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.Common.Evaluation
{
    public class UnknownParameterException : Exception
    {
        public string Name { get; set; }
    }
}
