using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// DC biasing behavior for an <see cref="IdealDiode" />.
    /// </summary>
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IConvergenceBehavior" />
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IConvergenceBehavior,
        IParameterized<IdealDiodeParameters>
    {
        private readonly IIterationSimulationState _iteration;
        private readonly IdealDiode _diode;
        private readonly ISimulation _simulation;
        private readonly IdealDiodeParameters _instanceParameters;
        private readonly IdealDiodeParameters _modelParameters;

        /// <summary>
        /// Gets the simulation biasing parameters.
        /// </summary>
        protected BiasingParameters BiasingParameters { get; }

        /// <summary>
        /// The variables used by the behavior.
        /// </summary>
        protected IdealDiodeVariables<double> Variables { get; }

        /// <summary>
        /// The matrix elements.
        /// </summary>
        protected ElementSet<double> Elements { get; }

        /// <inheritdoc />
        public IdealDiodeParameters Parameters { get; private set; }

        /// <summary>
        /// Gets the terminal voltage across the ideal diode branch.
        /// </summary>
        [ParameterName("v"), ParameterName("vd"), ParameterInfo("The terminal voltage across the ideal diode")]
        public double Voltage => Variables.Positive.Value - Variables.Negative.Value;

        /// <summary>
        /// Gets the voltage across the internal ideal diode cells.
        /// </summary>
        [ParameterName("vj"), ParameterName("vdiode"), ParameterInfo("The internal voltage across the ideal diode cells")]
        public double JunctionVoltage => LocalVoltage * Parameters.SeriesMultiplier;

        /// <summary>
        /// Gets the current through all ideal diodes in parallel.
        /// </summary>
        [ParameterName("i"), ParameterName("id"), ParameterName("c"), ParameterInfo("The current through the ideal diode")]
        public double Current => Variables.Branch.Value;

        /// <summary>
        /// Gets the small-signal conductance.
        /// </summary>
        [ParameterName("gd"), ParameterInfo("Terminal small-signal conductance")]
        public double Conductance => GetTerminalConductance();

        /// <summary>
        /// Gets the dissipated power in the ideal diode branch.
        /// </summary>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("The dissipated power")]
        public double Power => Current * Voltage;

        /// <summary>
        /// The voltage across one ideal diode.
        /// </summary>
        protected double LocalVoltage;

        /// <summary>
        /// The current through one ideal diode.
        /// </summary>
        protected double LocalCurrent;

        /// <summary>
        /// The conductance through one ideal diode.
        /// </summary>
        protected double LocalConductance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context" /> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context, IdealDiode diode = null, ISimulation simulation = null)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            var state = context.GetState<IBiasingSimulationState>();
            _iteration = context.GetState<IIterationSimulationState>();

            BiasingParameters = context.GetSimulationParameterSet<BiasingParameters>();
            _diode = diode;
            _simulation = simulation;
            _instanceParameters = context.GetParameterSet<IdealDiodeParameters>();
            _modelParameters = TryGetModelParameters(context);
            RefreshEffectiveParameters();
            Variables = new IdealDiodeVariables<double>(Name, state, context);
            Elements = new ElementSet<double>(
                state.Solver,
                Variables.GetMatrixLocations(state.Map),
                Variables.GetRhsIndices(state.Map));
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand-side vector.
        /// </summary>
        protected virtual void Load()
        {
            RefreshEffectiveParameters();
            Initialize(out double vd);

            IdealDiodeEquation.Evaluate(
                Parameters,
                BiasingParameters,
                vd,
                Parameters.Area,
                out double cd,
                out double gd);

            LocalVoltage = vd;
            LocalCurrent = cd;
            LocalConductance = gd;

            double cdeq = cd - (gd * vd);

            gd *= Parameters.ParallelMultiplier / Parameters.SeriesMultiplier;
            cdeq *= Parameters.ParallelMultiplier;
            Elements.Add(
                gd, gd, -gd, -gd,
                1.0, -1.0, 1.0, -1.0, -GetEffectiveSeriesResistance(),
                cdeq, -cdeq);
        }

        /// <inheritdoc />
        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Initializes the diode voltage based on the current iteration state.
        /// </summary>
        /// <param name="vd">The initialized diode voltage.</param>
        protected void Initialize(out double vd)
        {
            if (_iteration.Mode == IterationModes.Junction)
            {
                vd = Parameters.Off ? 0.0 : Parameters.ForwardVoltage.Value;
            }
            else if (_iteration.Mode == IterationModes.Fix && Parameters.Off)
            {
                vd = 0.0;
            }
            else
            {
                vd = (Variables.PosPrime.Value - Variables.Negative.Value) / Parameters.SeriesMultiplier;
            }
        }

        /// <inheritdoc />
        bool IConvergenceBehavior.IsConvergent()
        {
            RefreshEffectiveParameters();
            double vd = (Variables.PosPrime.Value - Variables.Negative.Value) / Parameters.SeriesMultiplier;

            double delvd = vd - LocalVoltage;
            double cdhat = LocalCurrent + (LocalConductance * delvd);
            double cd = LocalCurrent;

            double tol = (BiasingParameters.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(cd)))
                + BiasingParameters.AbsoluteTolerance;
            if (Math.Abs(cdhat - cd) > tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the effective series resistance seen by the total branch current.
        /// </summary>
        /// <returns>The effective series resistance.</returns>
        protected double GetEffectiveSeriesResistance()
        {
            if (Parameters.Resistance <= 0.0)
                return 0.0;

            double scale = Parameters.Area * Parameters.ParallelMultiplier;
            if (scale <= 0.0)
                throw new InvalidOperationException("Ideal diode area and parallel multiplier must be greater than zero when Rs is positive.");

            return Parameters.Resistance * Parameters.SeriesMultiplier / scale;
        }

        /// <summary>
        /// Gets the small-signal conductance seen at the external terminals.
        /// </summary>
        /// <returns>The terminal conductance.</returns>
        protected double GetTerminalConductance()
        {
            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;
            double diodeConductance = LocalConductance * m / n;

            if (Parameters.Resistance <= 0.0)
                return diodeConductance;

            double seriesResistance = GetEffectiveSeriesResistance();
            if (diodeConductance <= 0.0 || seriesResistance <= 0.0)
                return 0.0;

            return diodeConductance / (1.0 + (diodeConductance * seriesResistance));
        }

        private static IdealDiodeParameters TryGetModelParameters(IComponentBindingContext context)
        {
            try
            {
                return context.ModelBehaviors.GetParameterSet<IdealDiodeParameters>();
            }
            catch
            {
                return null;
            }
        }

        protected void RefreshEffectiveParameters()
        {
            var effectiveParameters = new IdealDiodeParameters();

            var modelParameters = _diode?.GetModelParameters(_simulation) ?? _modelParameters;
            if (modelParameters == null)
            {
                _instanceParameters.CopyTo(effectiveParameters);
                Parameters = effectiveParameters;
                return;
            }

            modelParameters.CopyTo(effectiveParameters);
            if (_diode != null)
            {
                foreach (var modelParameterOverride in _diode.GetModelParameterOverrides(_simulation))
                {
                    effectiveParameters.SetParameter(modelParameterOverride.Key, modelParameterOverride.Value);
                }
            }

            effectiveParameters.Area = _instanceParameters.Area;
            effectiveParameters.Off = _instanceParameters.Off;
            effectiveParameters.ParallelMultiplier = _instanceParameters.ParallelMultiplier;
            effectiveParameters.SeriesMultiplier = _instanceParameters.SeriesMultiplier;

            if (_instanceParameters.HasInstanceOverride("rs"))
            {
                effectiveParameters.Resistance = _instanceParameters.Resistance;
            }

            if (_instanceParameters.HasInstanceOverride("ron"))
            {
                effectiveParameters.OnResistance = _instanceParameters.OnResistance;
            }

            if (_instanceParameters.HasInstanceOverride("roff"))
            {
                effectiveParameters.OffResistance = _instanceParameters.OffResistance;
            }

            if (_instanceParameters.HasInstanceOverride("vfwd"))
            {
                effectiveParameters.ForwardVoltage = _instanceParameters.ForwardVoltage;
            }

            if (_instanceParameters.HasInstanceOverride("vrev"))
            {
                effectiveParameters.ReverseVoltage = _instanceParameters.ReverseVoltage;
            }

            if (_instanceParameters.HasInstanceOverride("rrev"))
            {
                effectiveParameters.ReverseResistance = _instanceParameters.ReverseResistance;
            }

            if (_instanceParameters.HasInstanceOverride("ilimit"))
            {
                effectiveParameters.ForwardCurrentLimit = _instanceParameters.ForwardCurrentLimit;
            }

            if (_instanceParameters.HasInstanceOverride("revilimit"))
            {
                effectiveParameters.ReverseCurrentLimit = _instanceParameters.ReverseCurrentLimit;
            }

            if (_instanceParameters.HasInstanceOverride("epsilon"))
            {
                effectiveParameters.ForwardEpsilon = _instanceParameters.ForwardEpsilon;
            }

            if (_instanceParameters.HasInstanceOverride("revepsilon"))
            {
                effectiveParameters.ReverseEpsilon = _instanceParameters.ReverseEpsilon;
            }

            Parameters = effectiveParameters;
        }
    }
}
