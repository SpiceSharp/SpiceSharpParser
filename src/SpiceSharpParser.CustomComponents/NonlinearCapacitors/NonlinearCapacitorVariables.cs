using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.CustomComponents.NonlinearCapacitors
{
    /// <summary>
    /// Variables for a <see cref="NonlinearCapacitor" />.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct NonlinearCapacitorVariables<T>
    {
        /// <summary>
        /// The positive node.
        /// </summary>
        public readonly IVariable<T> Positive;

        /// <summary>
        /// The negative node.
        /// </summary>
        public readonly IVariable<T> Negative;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearCapacitorVariables{T}" /> struct.
        /// </summary>
        /// <param name="factory">The variable factory.</param>
        /// <param name="context">The component binding context.</param>
        public NonlinearCapacitorVariables(IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(2);

            Positive = factory.GetSharedVariable(context.Nodes[0]);
            Negative = factory.GetSharedVariable(context.Nodes[1]);
        }

        /// <summary>
        /// Gets the nodal matrix locations.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The matrix locations.</returns>
        public MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];

            return new[]
            {
                new MatrixLocation(pos, pos),
                new MatrixLocation(pos, neg),
                new MatrixLocation(neg, pos),
                new MatrixLocation(neg, neg),
            };
        }

        /// <summary>
        /// Gets the transient right-hand-side vector indices.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The right-hand-side vector indices.</returns>
        public int[] GetRhsIndices(IVariableMap map)
        {
            return new[] { map[Positive], map[Negative] };
        }
    }
}
