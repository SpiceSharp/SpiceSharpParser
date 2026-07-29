using SpiceSharp.Simulations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// Stores the model selected for each simulation and its stepped overrides.
    /// </summary>
    /// <remarks>
    /// Simulation keys are weak so a reusable component does not keep completed
    /// simulations alive. The default model is used before simulation-specific
    /// model selection runs.
    /// </remarks>
    internal sealed class IdealDiodeModelState
    {
        private readonly ConditionalWeakTable<ISimulation, SimulationState> _simulations =
            new ConditionalWeakTable<ISimulation, SimulationState>();
        private IdealDiodeParameters _defaultModelParameters;

        public void SetModelParameters(ISimulation simulation, IdealDiodeParameters parameters)
        {
            if (simulation == null)
            {
                _defaultModelParameters = parameters;
                return;
            }

            _simulations.GetOrCreateValue(simulation).ModelParameters = parameters;
        }

        public IdealDiodeParameters GetModelParameters(ISimulation simulation)
        {
            if (simulation != null
                && _simulations.TryGetValue(simulation, out SimulationState state)
                && state.ModelParameters != null)
            {
                return state.ModelParameters;
            }

            return _defaultModelParameters;
        }

        public void SetModelParameterOverride(
            ISimulation simulation,
            string parameterName,
            double value)
        {
            if (simulation == null)
            {
                return;
            }

            _simulations.GetOrCreateValue(simulation).ModelParameterOverrides[parameterName] = value;
        }

        public IEnumerable<KeyValuePair<string, double>> GetModelParameterOverrides(
            ISimulation simulation)
        {
            if (simulation != null
                && _simulations.TryGetValue(simulation, out SimulationState state))
            {
                return state.ModelParameterOverrides;
            }

            return Array.Empty<KeyValuePair<string, double>>();
        }

        private sealed class SimulationState
        {
            public IdealDiodeParameters ModelParameters { get; set; }

            public ConcurrentDictionary<string, double> ModelParameterOverrides { get; } =
                new ConcurrentDictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
