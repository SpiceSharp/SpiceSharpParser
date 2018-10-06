using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IFunctionFactory
    {
        Function Create(string name, List<string> arguments, string functionBody);
    }
}