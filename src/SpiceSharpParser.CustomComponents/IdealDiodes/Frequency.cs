using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// Small-signal behavior for an <see cref="IdealDiode" />.
    /// </summary>
    /// <seealso cref="Biasing" />
    /// <seealso cref="IFrequencyBehavior" />
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;

        /// <summary>
        /// The complex variables used by the behavior.
        /// </summary>
        protected IdealDiodeVariables<Complex> ComplexVariables { get; }

        /// <summary>
        /// Gets the complex terminal voltage across the ideal diode branch.
        /// </summary>
        [ParameterName("v"), ParameterName("vd"), ParameterInfo("The complex terminal voltage across the ideal diode")]
        public Complex ComplexVoltage => ComplexVariables.Positive.Value - ComplexVariables.Negative.Value;

        /// <summary>
        /// Gets the complex voltage across the internal ideal diode cells.
        /// </summary>
        [ParameterName("vj"), ParameterName("vdiode"), ParameterInfo("The complex internal voltage across the ideal diode cells")]
        public Complex ComplexJunctionVoltage => ComplexVariables.PosPrime.Value - ComplexVariables.Negative.Value;

        /// <summary>
        /// Gets the complex current through all ideal diodes in parallel.
        /// </summary>
        [ParameterName("i"), ParameterName("id"), ParameterName("c"), ParameterInfo("The complex current through the ideal diode")]
        public Complex ComplexCurrent => ComplexVariables.Branch.Value;

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("The complex power")]
        public Complex ComplexPower => ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context" /> is <c>null</c>.</exception>
        public Frequency(IComponentBindingContext context, IdealDiode diode = null, ISimulation simulation = null)
            : base(context, diode, simulation)
        {
            var state = context.GetState<IComplexSimulationState>();
            ComplexVariables = new IdealDiodeVariables<Complex>(Name, state, context);
            _elements = new ElementSet<Complex>(
                state.Solver,
                ComplexVariables.GetMatrixLocations(state.Map));
        }

        /// <inheritdoc />
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc />
        void IFrequencyBehavior.Load()
        {
            RefreshEffectiveParameters();

            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;
            double gd = LocalConductance * m / n;
            var geq = new Complex(gd, 0.0);
            var series = new Complex(GetEffectiveSeriesResistance(), 0.0);

            _elements.Add(
                geq,
                geq,
                -geq,
                -geq,
                Complex.One,
                -Complex.One,
                Complex.One,
                -Complex.One,
                -series);
        }
    }
}
