using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations
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
            Func<string, Control, IReadingContext, BaseSimulation> simulationWithStochasticModels = CreateSimulationWithStochasticModelsDecorator.Decorate(context, createSimulation);

            if (!IsMonteCarloEnabledForSimulation(statement, context))
            {
                if (context.Result.SimulationConfiguration.ParameterSweeps.Count == 0)
                {
                    CreateSimulationsForAllTemperaturesFactory.CreateSimulations(statement, context, simulationWithStochasticModels);
                }
                else
                {
                    CreateSimulationsForAllParameterSweepsAndTemperaturesFactory.CreateSimulations(statement, context, simulationWithStochasticModels);
                }
            }
            else
            {
                CreateSimulationsForMonteCarloFactory.Create(statement, context, simulationWithStochasticModels);
            }
        }

        protected static bool IsMonteCarloEnabledForSimulation(Control statement, IReadingContext context)
        {
            return context.Result.SimulationConfiguration.MonteCarloConfiguration.Enabled
                && statement.Name.ToLower() == context.Result.SimulationConfiguration.MonteCarloConfiguration.SimulationType.ToLower();
        }

        /// <summary>
        /// Sets the base parameters.
        /// </summary>
        /// <param name="baseConfiguration">The configuration to set.</param>
        /// <param name="context">The reading context.</param>
        protected void SetBaseConfiguration(BaseConfiguration baseConfiguration, IReadingContext context)
        {
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
        }
    }
}
