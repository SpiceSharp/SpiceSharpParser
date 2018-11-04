using System;
using System.Collections.Concurrent;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationsUpdates
    {
        public SimulationsUpdates(ISimulationEvaluators evaluators, SimulationExpressionContexts contexts)
        {
            Contexts = contexts;
            Evaluators = evaluators;
            SpecificUpdates = new ConcurrentDictionary<Simulation, SimulationUpdates>();
            CommonUpdates = new SimulationUpdates();
        }

        protected ISimulationEvaluators Evaluators { get; set; }

        protected SimulationExpressionContexts Contexts { get; set; }

        protected ConcurrentDictionary<Simulation, SimulationUpdates> SpecificUpdates { get; set; }

        protected SimulationUpdates CommonUpdates { get; set; }

        public void Apply(BaseSimulation simulation)
        {
            // Apply common updates
            simulation.BeforeSetup += (object sender, EventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeSetup)
                {
                    pUpdate.Update(simulation, Evaluators, Contexts);
                }
            };

            simulation.BeforeTemperature += (object sender, LoadStateEventArgs args) =>
            {
                foreach (var pUpdate in CommonUpdates.ParameterUpdatesBeforeTemperature)
                {
                    pUpdate.Update(simulation, Evaluators, Contexts);
                }
            };

            // Apply simulation specific updates
            if (SpecificUpdates.ContainsKey(simulation))
            {
                var update = SpecificUpdates[simulation];

                simulation.BeforeSetup += (object sender, EventArgs args) =>
                {
                    foreach (var pUpdate in update.ParameterUpdatesBeforeSetup)
                    {
                        pUpdate.Update(simulation, Evaluators, Contexts);
                    }
                };

                simulation.BeforeTemperature += (object sender, LoadStateEventArgs args) =>
                {
                    foreach (var pUpdate in update.ParameterUpdatesBeforeTemperature)
                    {
                        pUpdate.Update(simulation, Evaluators, Contexts);
                    }
                };
            }
        }

        public void AddBeforeSetup(BaseSimulation simulation, Action<Simulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (SpecificUpdates.ContainsKey(simulation) == false)
            {
                SpecificUpdates[simulation] = new SimulationUpdates() { Simulation = simulation };
            }

            SpecificUpdates[simulation].ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = simulation, Update = update });
        }

        public void AddBeforeSetup(Action<Simulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            CommonUpdates.ParameterUpdatesBeforeSetup.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }

        public void AddBeforeTemperature(BaseSimulation simulation, Action<Simulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            if (SpecificUpdates.ContainsKey(simulation) == false)
            {
                SpecificUpdates[simulation] = new SimulationUpdates() { Simulation = simulation };
            }

            SpecificUpdates[simulation].ParameterUpdatesBeforeTemperature.Add(new SimulationUpdate() { Simulation = simulation, Update = update });
        }

        public void AddBeforeTemperature(Action<Simulation, ISimulationEvaluators, SimulationExpressionContexts> update)
        {
            CommonUpdates.ParameterUpdatesBeforeTemperature.Add(new SimulationUpdate() { Simulation = null, Update = update });
        }
    }
}
