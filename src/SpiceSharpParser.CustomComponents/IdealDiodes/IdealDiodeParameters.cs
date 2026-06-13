using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// Parameters for an <see cref="IdealDiode" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class IdealDiodeParameters : ParameterSet<IdealDiodeParameters>
    {
        private readonly ISet<string> _instanceOverrides = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Copies all parameters to another instance.
        /// </summary>
        /// <param name="target">The destination parameter set.</param>
        public void CopyTo(IdealDiodeParameters target)
        {
            target.Area = Area;
            target.Off = Off;
            target.Resistance = Resistance;
            target.ParallelMultiplier = ParallelMultiplier;
            target.SeriesMultiplier = SeriesMultiplier;
            target.OnResistance = OnResistance;

            if (OffResistance.Given)
            {
                target.OffResistance = OffResistance;
            }

            if (ForwardVoltage.Given)
            {
                target.ForwardVoltage = ForwardVoltage;
            }

            if (ReverseVoltage.Given)
            {
                target.ReverseVoltage = ReverseVoltage;
            }

            if (ReverseResistance.Given)
            {
                target.ReverseResistance = ReverseResistance;
            }

            if (ForwardCurrentLimit.Given)
            {
                target.ForwardCurrentLimit = ForwardCurrentLimit;
            }

            if (ReverseCurrentLimit.Given)
            {
                target.ReverseCurrentLimit = ReverseCurrentLimit;
            }

            if (ForwardEpsilon.Given)
            {
                target.ForwardEpsilon = ForwardEpsilon;
            }

            if (ReverseEpsilon.Given)
            {
                target.ReverseEpsilon = ReverseEpsilon;
            }
        }

        /// <summary>
        /// Marks an instance parameter as explicitly set by the netlist.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        public void MarkInstanceOverride(string parameterName)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                _instanceOverrides.Add(parameterName);
            }
        }

        /// <summary>
        /// Checks whether an instance parameter was explicitly set by the netlist.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns><c>true</c> if the parameter was explicitly set.</returns>
        public bool HasInstanceOverride(string parameterName)
        {
            return _instanceOverrides.Contains(parameterName);
        }

        /// <summary>
        /// Gets or sets the accepted diode area value. LTspice's idealized region-wise-linear diode ignores it electrically.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor")]
        [GreaterThan(0), Finite]
        private double _area = 1.0;

        /// <summary>
        /// Gets or sets whether the diode starts in the off state during junction initialization.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets or sets the accepted series resistance value. LTspice's idealized region-wise-linear diode ignores it electrically.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Parasitic series resistance", Units = "Ohm")]
        [GreaterThanOrEquals(0), Finite]
        private double _resistance;

        /// <summary>
        /// Gets or sets the number of ideal diodes in parallel.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThan(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the number of ideal diodes in series.
        /// </summary>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0), Finite]
        private double _seriesMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the forward on resistance.
        /// </summary>
        [ParameterName("ron"), ParameterInfo("Forward resistance", Units = "Ohm")]
        [GreaterThan(0), Finite]
        private double _onResistance = 1.0;

        /// <summary>
        /// Gets or sets the off resistance. If omitted, the simulation Gmin is used.
        /// </summary>
        [ParameterName("roff"), ParameterInfo("Off resistance", Units = "Ohm")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _offResistance = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the forward voltage.
        /// </summary>
        [ParameterName("vfwd"), ParameterInfo("Forward voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _forwardVoltage = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the reverse breakdown voltage magnitude.
        /// </summary>
        [ParameterName("vrev"), ParameterInfo("Reverse breakdown voltage", Units = "V")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _reverseVoltage = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the reverse breakdown resistance. If omitted, <see cref="OnResistance" /> is used.
        /// </summary>
        [ParameterName("rrev"), ParameterInfo("Reverse breakdown resistance", Units = "Ohm")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _reverseResistance = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the forward current limit.
        /// </summary>
        [ParameterName("ilimit"), ParameterInfo("Forward current limit", Units = "A")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _forwardCurrentLimit = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the reverse current limit.
        /// </summary>
        [ParameterName("revilimit"), ParameterInfo("Reverse current limit", Units = "A")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _reverseCurrentLimit = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the forward transition smoothing width.
        /// </summary>
        [ParameterName("epsilon"), ParameterInfo("Forward smoothing voltage", Units = "V")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _forwardEpsilon = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the reverse transition smoothing width.
        /// </summary>
        [ParameterName("revepsilon"), ParameterInfo("Reverse smoothing voltage", Units = "V")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _reverseEpsilon = new GivenParameter<double>(0.0, false);
    }
}
