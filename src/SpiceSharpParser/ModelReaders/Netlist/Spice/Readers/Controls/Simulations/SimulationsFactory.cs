using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    public class SimulationsFactory : ISimulationsFactory
    {
        public SimulationsFactory(IMapper<Exporter> exporterMapper)
        {
            var temp = new CreateSimulationsForAllTemperaturesFactory();

            CreateSimulationsForAllParameterSweepsAndTemperaturesFactory = new CreateSimulationsForAllParameterSweepsAndTemperaturesFactory(temp);
            CreateSimulationsForMonteCarloFactory = new CreateSimulationsForMonteCarloFactory(CreateSimulationsForAllParameterSweepsAndTemperaturesFactory, new ExportFactory(), exporterMapper);
        }

        /// <summary>
        /// Gets the simulations factory that creates simulations parameter and temperature sweeps.
        /// </summary>
        protected ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory CreateSimulationsForAllParameterSweepsAndTemperaturesFactory { get; }

        /// <summary>
        /// Gets the simulations factory that creates simulations for Monte Carol simulation.
        /// </summary>
        protected ICreateSimulationsForMonteCarloFactory CreateSimulationsForMonteCarloFactory { get; }

        /// <summary>
        /// Creates simulations.
        /// </summary>
        /// <param name="statement">Simulation statement.</param>
        /// <param name="context">Context.</param>
        /// <param name="createSimulation">Simulation factory.</param>
        public void Create(Control statement, ICircuitContext context, Func<string, Control, ICircuitContext, Simulation> createSimulation)
        {
            if (!IsMonteCarloEnabledForSimulation(statement, context))
            {
                var simulations = CreateSimulationsForAllParameterSweepsAndTemperaturesFactory.CreateSimulations(statement, context, createSimulation);
                foreach (var simulation in simulations)
                {
                    var stochasticDecorator = new EnableStochasticModelsSimulationDecorator(context);
                    stochasticDecorator.Decorate(simulation);

                    context.SimulationPreparations.Prepare(simulation);
                }
            }
            else
            {
                var simulations = CreateSimulationsForMonteCarloFactory.Create(statement, context, createSimulation);
                foreach (var simulation in simulations)
                {
                    var stochasticDecorator = new EnableStochasticModelsSimulationDecorator(context);
                    stochasticDecorator.Decorate(simulation);

                    context.SimulationPreparations.Prepare(simulation);
                }
            }
        }

        protected static bool IsMonteCarloEnabledForSimulation(Control statement, ICircuitContext context)
        {
            return context.Result.SimulationConfiguration.MonteCarloConfiguration.Enabled
                   && statement.Name.ToLower() == context.Result.SimulationConfiguration.MonteCarloConfiguration.SimulationType.ToLower();
        }
    }
}