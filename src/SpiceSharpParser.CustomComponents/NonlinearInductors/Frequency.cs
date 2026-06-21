using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpParser.CustomComponents.NonlinearInductors
{
    /// <summary>
    /// Small-signal behavior for a <see cref="NonlinearInductor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior,
        IBranchedBehavior<Complex>
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly NonlinearInductorVariables<Complex> _complexVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _complexVariables = new NonlinearInductorVariables<Complex>(Name, _complex, context);
            ComplexBranch = _complexVariables.Branch;
            _elements = new ElementSet<Complex>(
                _complex.Solver,
                _complexVariables.GetFrequencyMatrixLocations(_complex.Map));
        }

        /// <inheritdoc />
        IVariable<Complex> IBranchedBehavior<Complex>.Branch => ComplexBranch;

        /// <summary>
        /// Gets the complex branch current variable.
        /// </summary>
        protected IVariable<Complex> ComplexBranch { get; }

        /// <summary>
        /// Gets the complex voltage across the nonlinear inductor.
        /// </summary>
        [ParameterName("v"), ParameterName("vl"), ParameterInfo("The complex voltage across the nonlinear inductor")]
        public Complex ComplexVoltage => _complexVariables.Positive.Value - _complexVariables.Negative.Value;

        /// <summary>
        /// Gets the complex current through the nonlinear inductor.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current through the nonlinear inductor")]
        public Complex ComplexCurrent => ComplexBranch.Value;

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("The complex power")]
        public Complex ComplexPower => ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <inheritdoc />
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <inheritdoc />
        void IFrequencyBehavior.Load()
        {
            IncrementalInductance = EvaluateFluxDerivative(Current);
            Complex impedance = _complex.Laplace * IncrementalInductance;
            _elements.Add(
                Complex.One,
                -Complex.One,
                -Complex.One,
                Complex.One,
                -impedance);
        }
    }
}
