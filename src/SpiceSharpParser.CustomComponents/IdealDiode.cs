using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.IdealDiodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// A standalone LTspice-style ideal diode component.
    /// </summary>
    /// <seealso cref="Component" />
    /// <seealso cref="IParameterized{P}" />
    /// <seealso cref="IdealDiodeParameters" />
    [Pin(0, "D+"), Pin(1, "D-")]
    public class IdealDiode : Component<IdealDiodeParameters>
    {
        private readonly ConcurrentDictionary<ISimulation, IdealDiodeParameters> _simulationModelParameters = new ConcurrentDictionary<ISimulation, IdealDiodeParameters>();
        private readonly ConcurrentDictionary<ISimulation, ConcurrentDictionary<string, double>> _simulationModelParameterOverrides =
            new ConcurrentDictionary<ISimulation, ConcurrentDictionary<string, double>>();

        /// <summary>
        /// The pin count for ideal diodes.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int PinCount = 2;

        internal IdealDiodeParameters ModelParameters { get; set; }

        internal void SetModelParameters(ISimulation simulation, IdealDiodeParameters parameters)
        {
            if (simulation == null)
            {
                ModelParameters = parameters;
                return;
            }

            _simulationModelParameters[simulation] = parameters;
        }

        internal IdealDiodeParameters GetModelParameters(ISimulation simulation)
        {
            if (simulation != null && _simulationModelParameters.TryGetValue(simulation, out var parameters))
            {
                return parameters;
            }

            return ModelParameters;
        }

        internal void SetModelParameterOverride(ISimulation simulation, string parameterName, double value)
        {
            if (simulation == null)
            {
                return;
            }

            var overrides = _simulationModelParameterOverrides.GetOrAdd(
                simulation,
                _ => new ConcurrentDictionary<string, double>(StringComparer.OrdinalIgnoreCase));
            overrides[parameterName] = value;
        }

        internal IEnumerable<KeyValuePair<string, double>> GetModelParameterOverrides(ISimulation simulation)
        {
            if (simulation != null && _simulationModelParameterOverrides.TryGetValue(simulation, out var overrides))
            {
                return overrides;
            }

            return Array.Empty<KeyValuePair<string, double>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdealDiode"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public IdealDiode(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdealDiode"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="anode">The anode.</param>
        /// <param name="cathode">The cathode.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public IdealDiode(string name, string anode, string cathode)
            : this(name)
        {
            Connect(anode, cathode);
        }

        /// <inheritdoc />
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors);

            if (simulation.UsesBehaviors<IFrequencyBehavior>())
            {
                behaviors.Add(new Frequency(context, this, simulation));
            }
            else if (simulation.UsesBehaviors<IBiasingBehavior>())
            {
                behaviors.Add(new Biasing(context, this, simulation));
            }

            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
