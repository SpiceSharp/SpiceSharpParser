using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharpMechanical2D.ApiProbe
{
    /// <summary>
    /// Transient behavior for <see cref="TransientApiProbe"/>.
    /// </summary>
    [GeneratedParameters]
    public sealed partial class TransientApiProbeBehavior : Behavior,
        ITransientApiProbeBehavior,
        IBiasingBehavior,
        ITimeBehavior,
        IParameterized<TransientApiProbeParameters>
    {
        private readonly ElementSet<double> _elements;
        private readonly double[] _elementValues = new double[6];
        private readonly IDerivative _aState;
        private readonly IDerivative _bState;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientApiProbeBehavior"/> class.
        /// </summary>
        /// <param name="context">Binding context.</param>
        /// <param name="linkedBehavior">Optional probe behavior resolved during setup.</param>
        public TransientApiProbeBehavior(
            IBindingContext context,
            ITransientApiProbeBehavior linkedBehavior)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<TransientApiProbeParameters>();
            LinkedBehavior = linkedBehavior;

            var biasing = context.GetState<IBiasingSimulationState>();
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();

            AVariable = biasing.CreatePrivateVariable(Name.Combine("a"), Units.Volt);
            BVariable = biasing.CreatePrivateVariable(Name.Combine("b"), Units.Volt);

            int a = biasing.Map[AVariable];
            int b = biasing.Map[BVariable];
            _elements = new ElementSet<double>(
                biasing.Solver,
                new[]
                {
                    new MatrixLocation(a, a),
                    new MatrixLocation(a, b),
                    new MatrixLocation(b, a),
                    new MatrixLocation(b, b),
                },
                new[] { a, b });

            _aState = method.CreateDerivative(true);
            _bState = method.CreateDerivative(true);
        }

        /// <inheritdoc/>
        public TransientApiProbeParameters Parameters { get; }

        /// <inheritdoc/>
        public IVariable<double> AVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> BVariable { get; }

        /// <inheritdoc/>
        [ParameterName("a"), ParameterInfo("Current value of state A")]
        public double A => AVariable.Value;

        /// <inheritdoc/>
        [ParameterName("b"), ParameterInfo("Current value of state B")]
        public double B => BVariable.Value;

        /// <inheritdoc/>
        public ITransientApiProbeBehavior LinkedBehavior { get; }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            _aState.Value = A;
            _bState.Value = B;
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
            {
                LoadInitialState();
                return;
            }

            LoadTransientState();
        }

        private void LoadInitialState()
        {
            _elementValues[0] = 1.0;
            _elementValues[1] = 0.0;
            _elementValues[2] = 0.0;
            _elementValues[3] = 1.0;
            _elementValues[4] = Parameters.InitialA;
            _elementValues[5] = Parameters.InitialB;
            _elements.Add(_elementValues);
        }

        private void LoadTransientState()
        {
            _aState.Value = A;
            _bState.Value = B;
            _aState.Derive();
            _bState.Derive();

            JacobianInfo aDerivative = _aState.GetContributions(1.0, A);
            JacobianInfo bDerivative = _bState.GetContributions(1.0, B);

            // dA/dt - B = 0 and dB/dt + A = 0. GetContributions returns
            // derivative = Jacobian * currentValue + Rhs, so each equation's
            // history term moves to the solver right-hand side with a minus sign.
            _elementValues[0] = aDerivative.Jacobian;
            _elementValues[1] = -1.0;
            _elementValues[2] = 1.0;
            _elementValues[3] = bDerivative.Jacobian;
            _elementValues[4] = -aDerivative.Rhs;
            _elementValues[5] = -bDerivative.Rhs;
            _elements.Add(_elementValues);
        }
    }
}
