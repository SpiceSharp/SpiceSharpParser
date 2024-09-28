using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForAllParameterSweepsAndTemperaturesFactory : ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory
    {
        public CreateSimulationsForAllParameterSweepsAndTemperaturesFactory(ICreateSimulationsForAllTemperaturesFactory allTemperaturesFactory)
        {
            AllTemperaturesFactory = allTemperaturesFactory ?? throw new ArgumentNullException(nameof(allTemperaturesFactory));
            ParameterUpdater = new ParameterSweepUpdater();
        }

        /// <summary>
        /// Gets the simulations factory.
        /// </summary>
        public ICreateSimulationsForAllTemperaturesFactory AllTemperaturesFactory { get; }

        /// <summary>
        /// Gets the parameter updater.
        /// </summary>
        protected IParameterSweepUpdater ParameterUpdater { get; }

        /// <summary>
        /// Creates simulations for all parameter and temperature sweeps.
        /// </summary>
        /// <param name="statement">Statement.</param>
        /// <param name="context">Context.</param>
        /// <param name="createSimulation">Create simulation factory.</param>
        public List<ISimulationWithEvents> CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, ISimulationWithEvents> createSimulation)
        {
            var result = new List<ISimulationWithEvents>();

            ProcessTempParameterSweep(context);

            if (context.SimulationConfiguration.ParameterSweeps.Count == 0)
            {
                result.AddRange(AllTemperaturesFactory.CreateSimulations(
                    statement,
                    context,
                    createSimulation));

                return result;
            }

            List<List<double>> sweeps = new List<List<double>>();
            int productCount = 1;

            for (var i = 0; i < context.SimulationConfiguration.ParameterSweeps.Count; i++)
            {
                var sweepValues = context.SimulationConfiguration.ParameterSweeps[i].Sweep.ToList();
                sweeps.Add(sweepValues);
                productCount *= sweepValues.Count;
            }

            int[] system = sweeps.Select(s => s.Count()).ToArray();

            for (var i = 0; i < productCount; i++)
            {
                int[] indexes = NumberSystem.GetValueInSystem(i, system);

                Func<string, Control, IReadingContext, ISimulationWithEvents> createSimulationWithSweepParametersFactory =
                    (name, control, modifiedContext) =>
                    {
                        List<KeyValuePair<Parameter, double>> parameterValues =
                            GetSweepParameterValues(context, sweeps, system, indexes);
                        string suffix = GetSimulationNameSuffix(parameterValues);

                        var simulation = createSimulation($"{name} ({suffix})", control, modifiedContext);
                        SetSweepSimulation(context, parameterValues, simulation);

                        return simulation;
                    };

                result.AddRange(AllTemperaturesFactory.CreateSimulations(
                    statement,
                    context,
                    createSimulationWithSweepParametersFactory));
            }

            return result;
        }

        protected string GetSimulationNameSuffix(List<KeyValuePair<Parameter, double>> parameterValues)
        {
            string result = string.Empty;

            for (var i = 0; i < parameterValues.Count; i++)
            {
                result += $"{parameterValues[i].Key.Value}={parameterValues[i].Value}";

                if (i != parameterValues.Count - 1)
                {
                    result += ", ";
                }
            }

            return result;
        }

        protected List<KeyValuePair<Parameter, double>> GetSweepParameterValues(IReadingContext context, List<List<double>> sweeps, int[] system, int[] indexes)
        {
            List<KeyValuePair<Parameter, double>> parameterValues = new List<KeyValuePair<Parameter, double>>();

            for (var j = 0; j < system.Length; j++)
            {
                Parameter parameter = context.SimulationConfiguration.ParameterSweeps[j].Parameter;

                double parameterValue = sweeps[j][indexes[j]];
                parameterValues.Add(new KeyValuePair<Parameter, double>(parameter, parameterValue));
            }

            return parameterValues;
        }

        protected void ProcessTempParameterSweep(IReadingContext context)
        {
            var tempSweep = context.SimulationConfiguration.ParameterSweeps.SingleOrDefault(sweep => sweep.Parameter.Value.ToUpper() == "TEMP");

            if (tempSweep != null)
            {
                context.SimulationConfiguration.ParameterSweeps.Remove(tempSweep);
                var tempValues = tempSweep.Sweep.ToList();

                if (context.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
                {
                    context.SimulationConfiguration.TemperaturesInKelvins.Remove(context.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
                }

                context.SimulationConfiguration.TemperaturesInKelvins.Clear();

                foreach (var temp in tempValues)
                {
                    context.SimulationConfiguration.TemperaturesInKelvins.Add(Constants.CelsiusKelvin + temp);
                }
            }
        }

        protected void SetSweepSimulation(IReadingContext context, List<KeyValuePair<Parameter, double>> parameterValues, ISimulationWithEvents simulation)
        {
            ParameterUpdater.Update(simulation, context, parameterValues);
        }
    }
}