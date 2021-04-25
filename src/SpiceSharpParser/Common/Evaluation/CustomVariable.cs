using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.Common.Evaluation
{
    public class CustomVariable<T> : IVariable<T>
    {
        public T Value { get; set; }

        public string Name { get; set; }

        public IUnit Unit { get; set; }

        public bool Constant { get; set; }
        public Node VariableNode { get; internal set; }
    }
}
