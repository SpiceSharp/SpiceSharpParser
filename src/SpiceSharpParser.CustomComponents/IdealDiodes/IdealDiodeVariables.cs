using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// Variables for an <see cref="IdealDiode" />.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public readonly struct IdealDiodeVariables<T>
    {
        /// <summary>
        /// The positive node.
        /// </summary>
        public readonly IVariable<T> Positive;

        /// <summary>
        /// The internal positive node.
        /// </summary>
        public readonly IVariable<T> PosPrime;

        /// <summary>
        /// The negative node.
        /// </summary>
        public readonly IVariable<T> Negative;

        /// <summary>
        /// The current through the series branch from the positive node to the internal positive node.
        /// </summary>
        public readonly IVariable<T> Branch;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdealDiodeVariables{T}" /> struct.
        /// </summary>
        /// <param name="name">The device name.</param>
        /// <param name="factory">The variable factory.</param>
        /// <param name="context">The component binding context.</param>
        public IdealDiodeVariables(string name, IVariableFactory<IVariable<T>> factory, IComponentBindingContext context)
        {
            context.Nodes.CheckNodes(2);

            Positive = factory.GetSharedVariable(context.Nodes[0]);
            Negative = factory.GetSharedVariable(context.Nodes[1]);
            PosPrime = factory.CreatePrivateVariable(name.Combine("pos"), Units.Volt);
            Branch = factory.CreatePrivateVariable(name.Combine("branch"), Units.Ampere);
        }

        /// <summary>
        /// Gets the matrix locations.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The matrix locations.</returns>
        public MatrixLocation[] GetMatrixLocations(IVariableMap map)
        {
            int pos = map[Positive];
            int posPrime = map[PosPrime];
            int neg = map[Negative];
            int branch = map[Branch];

            return new[]
            {
                new MatrixLocation(neg, neg),
                new MatrixLocation(posPrime, posPrime),
                new MatrixLocation(neg, posPrime),
                new MatrixLocation(posPrime, neg),
                new MatrixLocation(pos, branch),
                new MatrixLocation(posPrime, branch),
                new MatrixLocation(branch, pos),
                new MatrixLocation(branch, posPrime),
                new MatrixLocation(branch, branch),
            };
        }

        /// <summary>
        /// Gets the right-hand-side vector indices.
        /// </summary>
        /// <param name="map">The variable map.</param>
        /// <returns>The right-hand-side vector indices.</returns>
        public int[] GetRhsIndices(IVariableMap map)
        {
            return new[] { map[Negative], map[PosPrime] };
        }
    }
}
