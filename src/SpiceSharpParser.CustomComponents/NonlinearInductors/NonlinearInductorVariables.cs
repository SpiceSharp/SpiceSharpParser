using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.CustomComponents.NonlinearInductors
{
    /// <summary>
    /// Variables for a <see cref="NonlinearInductor" />.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct NonlinearInductorVariables<T>
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
        /// The branch current variable.
        /// </summary>
        public readonly IVariable<T> Branch;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearInductorVariables{T}" /> struct.
        /// </summary>
        /// <param name="name">The device name.</param>
        /// <param name="factory">The variable factory.</param>
        /// <param name="context">The component binding context.</param>
        public NonlinearInductorVariables(string name, IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(2);

            Positive = factory.GetSharedVariable(context.Nodes[0]);
            Negative = factory.GetSharedVariable(context.Nodes[1]);
            Branch = factory.CreatePrivateVariable(name.Combine("branch"), Units.Ampere);
        }

        /// <summary>
        /// Gets the DC matrix locations.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The matrix locations.</returns>
        public MatrixLocation[] GetBiasingMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];
            int branch = map[Branch];

            return new[]
            {
                new MatrixLocation(pos, branch),
                new MatrixLocation(neg, branch),
                new MatrixLocation(branch, neg),
                new MatrixLocation(branch, pos),
            };
        }

        /// <summary>
        /// Gets the transient matrix locations.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The matrix locations.</returns>
        public MatrixLocation[] GetTimeMatrixLocations(IVariableMap map)
        {
            int branch = map[Branch];

            return new[]
            {
                new MatrixLocation(branch, branch),
            };
        }

        /// <summary>
        /// Gets the frequency-domain matrix locations.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The matrix locations.</returns>
        public MatrixLocation[] GetFrequencyMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int neg = map[Negative];
            int branch = map[Branch];

            return new[]
            {
                new MatrixLocation(pos, branch),
                new MatrixLocation(neg, branch),
                new MatrixLocation(branch, neg),
                new MatrixLocation(branch, pos),
                new MatrixLocation(branch, branch),
            };
        }

        /// <summary>
        /// Gets the transient right-hand-side vector indices.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The right-hand-side vector indices.</returns>
        public int[] GetTimeRhsIndices(IVariableMap map)
        {
            return new[] { map[Branch] };
        }
    }
}
