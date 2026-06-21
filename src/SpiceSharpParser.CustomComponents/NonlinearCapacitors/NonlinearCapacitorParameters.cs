using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpParser.CustomComponents.NonlinearCapacitors
{
    /// <summary>
    /// Parameters for a <see cref="NonlinearCapacitor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class NonlinearCapacitorParameters : ParameterSet<NonlinearCapacitorParameters>
    {
        /// <summary>
        /// Gets or sets the LTspice-style charge expression. The variable x is the terminal voltage.
        /// </summary>
        [ParameterName("q"), ParameterInfo("Charge expression", Units = "C")]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the parser callback used to turn <see cref="Expression"/> into a behavioral expression tree.
        /// </summary>
        public Func<string, Node> ParseAction { get; set; }

        /// <summary>
        /// Gets or sets the initial voltage.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _initialCondition = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the number of parallel capacitor cells.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThan(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the number of series capacitor cells.
        /// </summary>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0), Finite]
        private double _seriesMultiplier = 1.0;
    }
}
