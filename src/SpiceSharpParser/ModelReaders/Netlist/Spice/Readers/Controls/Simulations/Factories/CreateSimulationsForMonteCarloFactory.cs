using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForMonteCarloFactory : ICreateSimulationsForMonteCarloFactory
    {
        public CreateSimulationsForMonteCarloFactory(ICreateSimulationsForAllTemperaturesFactory allTemperatures, ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory allTemperaturesAndSweeps)
        {
            AllTemperaturesAndSweeps = allTemperaturesAndSweeps;
            AllTemperatures = allTemperatures;
        }

        public ICreateSimulationsForAllTemperaturesFactory AllTemperatures { get; }

        public ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory AllTemperaturesAndSweeps { get; }

        public List<BaseSimulation> Create(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> simulationWithStochasticModels)
        {
            context.Result.MonteCarlo.Enabled = true;
            context.Result.MonteCarlo.RandomSeed = context.Result.SimulationConfiguration.Seed;
            context.Result.MonteCarlo.VariableName = context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable;
            context.Result.MonteCarlo.Function = context.Result.SimulationConfiguration.MonteCarloConfiguration.Function;

            var result = new List<BaseSimulation>();

            if (context.Result.SimulationConfiguration.ParameterSweeps.Count == 0)
            {
                for (var i = 0; i < context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
                {
                    var simulations = AllTemperatures.CreateSimulations(statement, context, simulationWithStochasticModels);
                    AttachMonteCarloDataGathering(context, simulations);

                    result.AddRange(simulations);
                }
            }
            else
            {
                for (var i = 0; i < context.Result.SimulationConfiguration.MonteCarloConfiguration.Runs; i++)
                {
                    var simulations = AllTemperaturesAndSweeps.CreateSimulations(statement, context, simulationWithStochasticModels);
                    AttachMonteCarloDataGathering(context, simulations);
                    result.AddRange(simulations);
                }
            }

            return result;
        }

        protected static void AttachMonteCarloDataGathering(IReadingContext context, IEnumerable<BaseSimulation> simulations)
        {
            foreach (var simulation in simulations)
            {
                AttachMonteCarloDataGatheringForSimulation(context, simulation);
            }
        }

        protected static void AttachMonteCarloDataGatheringForSimulation(IReadingContext context, BaseSimulation simulation)
        {
            Export export = null;

            simulation.BeforeExecute += (object sender, BeforeExecuteEventArgs args) =>
            {
                export = context.Result.Exports.SingleOrDefault(e => e.Simulation == simulation && e.Name.ToLower() == context.Result.SimulationConfiguration.MonteCarloConfiguration.OutputVariable.ToLower());
            };

            simulation.ExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                if (export != null)
                {
                    var value = export.Extract();
                    context.Result.MonteCarlo.Collect(simulation, value);
                }
            };
        }
    }
}
