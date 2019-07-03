using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForAllParameterSweepsAndTemperaturesFactory : ICreateSimulationsForAllParameterSweepsAndTemperaturesFactory
    {
        public CreateSimulationsForAllParameterSweepsAndTemperaturesFactory(ICreateSimulationsForAllTemperaturesFactory allTemperaturesFactory)
        {
            AllTemperaturesFactory = allTemperaturesFactory;

            ParameterUpdater = new ParameterSweepUpdater();
        }

        public ICreateSimulationsForAllTemperaturesFactory AllTemperaturesFactory { get; }

        /// <summary>
        /// Gets the parameter updater.
        /// </summary>
        protected IParameterSweepUpdater ParameterUpdater { get; }

        public List<BaseSimulation> CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            var result = new List<BaseSimulation>();

            ProcessTempParameterSweep(context);

            List<List<double>> sweeps = new List<List<double>>();
            int productCount = 1;

            for (var i = 0; i < context.Result.SimulationConfiguration.ParameterSweeps.Count; i++)
            {
                var sweepValues = context.Result.SimulationConfiguration.ParameterSweeps[i].Sweep.Points.ToList();
                sweeps.Add(sweepValues);
                productCount *= sweepValues.Count;
            }

            int[] system = sweeps.Select(s => s.Count()).ToArray();

            for (var i = 0; i < productCount; i++)
            {
                int[] indexes = NumberSystem.GetValueInSystem(i, system);

                Func<string, Control, IReadingContext, BaseSimulation> createSimulationWithSweepParametersFactory =
                    (name, control, modifiedContext) =>
                    {
                        List<KeyValuePair<Parameter, double>> parameterValues = GetSweepParameterValues(context, sweeps, system, indexes);
                        string suffix = GetSimulationNameSuffix(parameterValues);

                        var simulation = createSimulation(name + " (" + suffix + ")", control, modifiedContext);
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
                result += parameterValues[i].Key.Image + "=" + parameterValues[i].Value;

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
                Parameter parameter = context.Result.SimulationConfiguration.ParameterSweeps[j].Parameter;

                double parameterValue = sweeps[j][indexes[j]];
                parameterValues.Add(new KeyValuePair<Parameter, double>(parameter, parameterValue));
            }

            return parameterValues;
        }

        protected void ProcessTempParameterSweep(IReadingContext context)
        {
            var tempSweep = context.Result.SimulationConfiguration.ParameterSweeps.SingleOrDefault(sweep => sweep.Parameter.Image == "TEMP");

            if (tempSweep != null)
            {
                context.Result.SimulationConfiguration.ParameterSweeps.Remove(tempSweep);
                var tempValues = tempSweep.Sweep.Points.ToList();

                if (context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Remove(context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
                }

                context.Result.SimulationConfiguration.TemperaturesInKelvins.Clear();

                foreach (var temp in tempValues)
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(Constants.CelsiusKelvin + temp);
                }
            }
        }

        protected void SetSweepSimulation(IReadingContext context, List<KeyValuePair<Parameter, double>> parameterValues, BaseSimulation simulation)
        {
            ParameterUpdater.Update(simulation, context, parameterValues);
        }
    }
}
