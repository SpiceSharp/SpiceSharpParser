using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpParser.CustomComponents.NonlinearCapacitors
{
    /// <summary>
    /// Small-signal behavior for a <see cref="NonlinearCapacitor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class Frequency : Biasing,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly NonlinearCapacitorVariables<Complex> _complexVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();
            _complexVariables = new NonlinearCapacitorVariables<Complex>(_complex, context);
            _elements = new ElementSet<Complex>(
                _complex.Solver,
                _complexVariables.GetMatrixLocations(_complex.Map));
        }

        /// <summary>
        /// Gets the complex voltage across the nonlinear capacitor.
        /// </summary>
        [ParameterName("v"), ParameterName("vc"), ParameterInfo("The complex voltage across the nonlinear capacitor")]
        public Complex ComplexVoltage => _complexVariables.Positive.Value - _complexVariables.Negative.Value;

        /// <summary>
        /// Gets the complex current through the nonlinear capacitor.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The complex current through the nonlinear capacitor")]
        public Complex ComplexCurrent => _complex.Laplace * IncrementalCapacitance * ComplexVoltage;

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
            IncrementalCapacitance = EvaluateChargeDerivative(Voltage);
            Complex admittance = _complex.Laplace * IncrementalCapacitance;
            _elements.Add(
                admittance,
                -admittance,
                -admittance,
                admittance);
        }
    }
}
