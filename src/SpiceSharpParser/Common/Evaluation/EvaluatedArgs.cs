using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.Common.Evaluation
{
    public class EvaluatedArgs : EventArgs
    {
        public double NewValue { get; set; }
    }
}
