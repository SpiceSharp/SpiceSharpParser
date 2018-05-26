using System.Collections.Generic;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation
{
    public interface ISpiceEvaluator : IEvaluator
    {
        ISpiceEvaluator CreateChildEvaluator();
    }
}
