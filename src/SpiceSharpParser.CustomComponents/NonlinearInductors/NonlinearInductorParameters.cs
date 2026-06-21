using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.ParameterSets;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;

namespace SpiceSharpParser.CustomComponents.NonlinearInductors
{
    /// <summary>
    /// Parameters for a <see cref="NonlinearInductor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class NonlinearInductorParameters : ParameterSet<NonlinearInductorParameters>
    {
        /// <summary>
        /// Gets or sets the LTspice-style flux expression. The variable x is the branch current.
        /// </summary>
        [ParameterName("flux"), ParameterInfo("Flux expression", Units = "Wb")]
        public string Expression { get; set; }

        /// <summary>
        /// Gets or sets the parser callback used to turn <see cref="Expression"/> into a behavioral expression tree.
        /// </summary>
        public Func<string, Node> ParseAction { get; set; }

        /// <summary>
        /// Gets or sets the initial current.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial current", Units = "A")]
        [Finite]
        private GivenParameter<double> _initialCondition = new GivenParameter<double>(0.0, false);

        /// <summary>
        /// Gets or sets the number of parallel inductor cells.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThan(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the number of series inductor cells.
        /// </summary>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0), Finite]
        private double _seriesMultiplier = 1.0;
    }
}
