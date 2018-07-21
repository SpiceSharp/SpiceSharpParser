using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation readers.
    /// </summary>
    /// TODO: Add comments please ... and please, please refactor me. Please... Please ...
    public abstract class SimulationControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationControl"/> class.
        /// </summary>
        public SimulationControl()
        {
            ParameterUpdater = new ParameterSweepUpdater();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationControl"/> class.
        /// </summary>
        /// <param name="updater">The sweep parameter updater.</param>
        public SimulationControl(IParameterSweepUpdater updater)
        {
            ParameterUpdater = updater ?? throw new NullReferenceException(nameof(updater));
        }

        /// <summary>
        /// Gets the parameter updater.
        /// </summary>
        protected IParameterSweepUpdater ParameterUpdater { get; }

        /// <summary>
        /// Creates simulations.
        /// </summary>
        protected void CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            if (context.Result.SimulationConfiguration.ParameterSweeps.Count == 0)
            {
                CreateSimulationsForAllTemperatures(statement, context, createSimulation);
            }
            else
            {
                CreateSimulationsForAllParameterSweepsAndTemperatures(statement, context, createSimulation);
            }
        }

        protected void CreateSimulationsForAllParameterSweepsAndTemperatures(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
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
                int[] indexes = Numbers.GetSystemNumber(i, system);

                Func<string, Control, IReadingContext, BaseSimulation> modifiedCreateSimulation =
                    (name, control, modifiedContext) =>
                    {
                        List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> parameterValues = GetSweepParameterValues(context, sweeps, system, indexes);
                        string suffix = GetSimulationNameSuffix(parameterValues);

                        var simulation = createSimulation(name + " (" + suffix + ")", control, modifiedContext);
                        SetSweepSimulation(context, parameterValues, simulation);

                        return simulation;
                    };

                CreateSimulationsForAllTemperatures(
                    statement,
                    context,
                    modifiedCreateSimulation);
            }
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
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(Circuit.CelsiusKelvin + temp);
                }
            }
        }

        protected void SetSweepSimulation(IReadingContext context, List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> parameterValues, BaseSimulation simulation)
        {
            ParameterUpdater.Update(simulation, context, parameterValues);
        }

        protected string GetSimulationNameSuffix(List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> parameterValues)
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

        protected List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> GetSweepParameterValues(IReadingContext context, List<List<double>> sweeps, int[] system, int[] indexes)
        {
            List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> parameterValues = new List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>>();

            for (var j = 0; j < system.Length; j++)
            {
                Models.Netlist.Spice.Objects.Parameter parameter = context.Result.SimulationConfiguration.ParameterSweeps[j].Parameter;

                double parameterValue = sweeps[j][indexes[j]];
                parameterValues.Add(new KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>(parameter, parameterValue));
            }

            return parameterValues;
        }

        protected IEnumerable<BaseSimulation> CreateSimulationsForAllTemperatures(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            var result = new List<BaseSimulation>();

            if (context.Result.SimulationConfiguration.TemperaturesInKelvins.Count > 0)
            {
                foreach (double temp in context.Result.SimulationConfiguration.TemperaturesInKelvins)
                {
                    CreateSimulationForTemperature(statement, context, createSimulation, result, temp);
                }
            }
            else
            {
                CreateSimulationForTemperature(statement, context, createSimulation, result, null);
            }

            return result;
        }

        protected void CreateSimulationForTemperature(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation, List<BaseSimulation> result, double? temp)
        {
            var simulation = createSimulation(GetSimulationName(context, temp), statement, context);

            SetTempVariable(context, temp, simulation);
            SetSimulationTemperatures(simulation, temp, context.Result.SimulationConfiguration.NominalTemperatureInKelvins);

            result.Add(simulation);
        }

        protected void SetTempVariable(IReadingContext context, double? operatingTemperatureInKelvins, BaseSimulation simulation)
        {
            double temp = 0;
            if (operatingTemperatureInKelvins.HasValue)
            {
                temp = operatingTemperatureInKelvins.Value - Circuit.CelsiusKelvin;
            }
            else
            {
                temp = Circuit.ReferenceTemperature - Circuit.CelsiusKelvin;
            }

            simulation.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs e) =>
            {
                var evaluator = context.SimulationContexts.GetSimulationEvaluator(simulation);
                evaluator.SetParameter("TEMP", temp, simulation);
            };
        }

        protected string GetSimulationName(IReadingContext context, double? temperatureInKelvin = null)
        {
            if (temperatureInKelvin.HasValue)
            {
                return string.Format("#{0} {1} - at {2} Kelvins ({3} Celsius)", context.Result.Simulations.Count()+1, SpiceCommandName, temperatureInKelvin.Value, temperatureInKelvin.Value - SpiceSharp.Circuit.CelsiusKelvin);
            }

            return string.Format("#{0} {1}", context.Result.Simulations.Count() + 1, SpiceCommandName);
        }

        protected void SetSimulationTemperatures(BaseSimulation simulation, double? operatingTemperatureInKelvins, double? nominalTemperatureInKelvins)
        {
            if (operatingTemperatureInKelvins.HasValue)
            {
                CircuitTemperatureSimulationDecorator.Decorate(simulation, operatingTemperatureInKelvins.Value);
            }

            if (nominalTemperatureInKelvins.HasValue)
            {
                NominalTemperatureSimulationDecorator.Decorate(simulation, nominalTemperatureInKelvins.Value);
            }
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
