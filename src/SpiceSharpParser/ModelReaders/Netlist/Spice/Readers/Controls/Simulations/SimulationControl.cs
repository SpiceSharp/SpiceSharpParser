using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation readers.
    /// </summary>
    public abstract class SimulationControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationControl"/> class.
        /// </summary>
        public SimulationControl()
        {
            CreateSimulationsForAllTemperaturesFactory = new CreateSimulationsForAllTemperaturesFactory();
            CreateSimulationsForAllParameterSweepsAndTemperaturesFactory = new CreateSimulationsForAllParameterSweepsAndTemperaturesFactory(CreateSimulationsForAllTemperaturesFactory);
            CreateSimulationsForMonteCarloFactory = new CreateSimulationsForMonteCarloFactory(CreateSimulationsForAllTemperaturesFactory, CreateSimulationsForAllParameterSweepsAndTemperaturesFactory);
        }

        public ICreateSimulationsForAllTemperaturesFactory CreateSimulationsForAllTemperaturesFactory { get; private set; }

        public ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory CreateSimulationsForAllParameterSweepsAndTemperaturesFactory { get; private set; }

        public ICreateSimulationsForMonteCarloFactory CreateSimulationsForMonteCarloFactory { get; }

        /// <summary>
        /// Creates simulations.
        /// </summary>
        protected void CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            if (context.ModelsRegistry is StochasticModelsRegistry)
            {
                createSimulation = CreateSimulationWithStochasticModelsDecorator.Decorate(context, createSimulation);
            }

            if (!IsMonteCarloEnabledForSimulation(statement, context))
            {
                if (context.Result.SimulationConfiguration.ParameterSweeps.Count == 0)
                {
                    var simulations = CreateSimulationsForAllTemperaturesFactory.CreateSimulations(statement, context, createSimulation);

                    foreach (var simulation in simulations)
                    {
                        context.SimulationsParameters.Prepare(simulation);
                    }
                }
                else
                {
                    var simulations = CreateSimulationsForAllParameterSweepsAndTemperaturesFactory.CreateSimulations(statement, context, createSimulation);
                    foreach (var simulation in simulations)
                    {
                        context.SimulationsParameters.Prepare(simulation);
                    }
                }
            }
            else
            {
                var simulations = CreateSimulationsForMonteCarloFactory.Create(statement, context, createSimulation);

                foreach (var simulation in simulations)
                {
                    context.SimulationsParameters.Prepare(simulation);
                }
            }
        }

        protected static bool IsMonteCarloEnabledForSimulation(Control statement, IReadingContext context)
        {
            return context.Result.SimulationConfiguration.MonteCarloConfiguration.Enabled
                && statement.Name.ToLower() == context.Result.SimulationConfiguration.MonteCarloConfiguration.SimulationType.ToLower();
        }

        /// <summary>
        /// Sets the base parameters of a simulation.
        /// </summary>
        /// <param name="baseSimulation">The simulation to configure.</param>
        /// <param name="context">The reading context.</param>
        protected void ConfigureCommonSettings(BaseSimulation baseSimulation, IReadingContext context)
        {
            var baseConfiguration = baseSimulation.Configurations.Get<BaseConfiguration>();
            if (context.Result.SimulationConfiguration.Gmin.HasValue)
            {
                baseConfiguration.Gmin = context.Result.SimulationConfiguration.Gmin.Value;
            }

            if (context.Result.SimulationConfiguration.AbsoluteTolerance.HasValue)
            {
                baseConfiguration.AbsoluteTolerance = context.Result.SimulationConfiguration.AbsoluteTolerance.Value;
            }

            if (context.Result.SimulationConfiguration.RelTolerance.HasValue)
            {
                baseConfiguration.RelativeTolerance = context.Result.SimulationConfiguration.RelTolerance.Value;
            }

            if (context.Result.SimulationConfiguration.DCMaxIterations.HasValue)
            {
                baseConfiguration.DcMaxIterations = context.Result.SimulationConfiguration.DCMaxIterations.Value;
            }

            baseSimulation.Configurations.Add(
                new CollectionConfiguration()
                {
                    EntityComparer = StringComparerProvider.Get(context.CaseSensitivity.IsEntityNameCaseSensitive),
                    VariableComparer = StringComparerProvider.Get(context.CaseSensitivity.IsNodeNameCaseSensitive)
                });
        }
    }
}
