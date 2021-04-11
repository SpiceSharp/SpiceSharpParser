using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;

namespace SpiceSharpParser.Common.Evaluation
{
    public class CustomVariable<T> : IVariable<T>
    {
        public T Value { get; set; }

        public string Name { get; set; }

        public IUnit Unit { get; set; }
    }
}
