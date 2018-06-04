using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Base for all control simulation readers.
    /// </summary>
    /// TODO: Add comments please ...
    public abstract class SimulationControl : BaseControl
    {
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
            var tempSweep = context.Result.SimulationConfiguration.ParameterSweeps.SingleOrDefault(sweep => sweep.Parameter.Image == "TEMP");

            if (tempSweep != null)
            {
                context.Result.SimulationConfiguration.ParameterSweeps.Remove(tempSweep);
                var tempValues = tempSweep.Sweep.Points.ToList();

                //TODO: Clean it please
                if (context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.HasValue)
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Remove(context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions.Value);
                }
                context.Result.SimulationConfiguration.TemperaturesInKelvins.Clear(); //TODO: Add some checks ...

                foreach (var temp in tempValues)
                {
                    context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(Circuit.CelsiusKelvin + temp);
                }
            }

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
                        SetSimulation(context, parameterValues, simulation);

                        return simulation;
                    };

                CreateSimulationsForAllTemperatures(
                    statement,
                    context,
                    modifiedCreateSimulation);
            }
        }

        protected void SetSimulation(IReadingContext context, List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> parameterValues, BaseSimulation simulation)
        {
            simulation.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs e) =>
            {
                foreach (var paramToSet in parameterValues)
                {
                    if (paramToSet.Key is WordParameter || paramToSet.Key is IdentifierParameter)
                    {
                        if (context.Result.FindObject(paramToSet.Key.Image, out Entity @object))
                        {
                            SetIndependentSource(paramToSet, @object);
                        }
                        else
                        {
                            UpdateSimulationParameter(context, paramToSet);
                        }
                    }

                    if (paramToSet.Key is ReferenceParameter rp)
                    {
                        UpdateDeviceParameter(context, paramToSet, rp);
                    }

                    if (paramToSet.Key is BracketParameter bp)
                    {
                        UpdateModelParameter(context, paramToSet, bp);
                    }
                }
            };
        }

        protected void UpdateDeviceParameter(IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, ReferenceParameter rp)
        {
            string objectName = rp.Name;
            string paramName = rp.Argument;
            if (context.Result.FindObject(objectName, out Entity @object))
            {
                context.SetParameter(@object, paramName, paramToSet.Value.ToString());
            }
        }

        protected void UpdateModelParameter(IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, BracketParameter bp)
        {
            string modelName = bp.Name;
            string paramName = bp.Parameters[0].Image;
            if (context.Result.FindObject(modelName, out Entity @model))
            {
                context.SetParameter(model, paramName, paramToSet.Value.ToString());
            }
        }

        protected void UpdateSimulationParameter(IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            if (context.Evaluator.GetParameterNames().Contains(paramToSet.Key.Image))
            {
                context.Evaluator.SetParameter(paramToSet.Key.Image, paramToSet.Value);
            }
            else
            {
                throw new Exception("Unknown parameter");
            }
        }

        protected void SetIndependentSource(KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, Entity @object)
        {
            if (@object is VoltageSource vs)
            {
                vs.SetParameter("dc", paramToSet.Value); //TODO add ac magnitude
            }

            if (@object is CurrentSource cs)
            {
                cs.SetParameter("dc", paramToSet.Value); //TODO add ac magnitude
            }
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
                    var simulation = createSimulation(GetSimulationName(context, temp), statement, context);

                    SetTempVariable(context, temp, simulation);
                    SetSimulationTemperatures(simulation, temp, context.Result.SimulationConfiguration.NominalTemperatureInKelvins);

                    result.Add(simulation);
                }
            }
            else
            {
                var simulation = createSimulation(GetSimulationName(context, null), statement, context);

                SetTempVariable(context, null, simulation);
                SetSimulationTemperatures(simulation, null, context.Result.SimulationConfiguration.NominalTemperatureInKelvins);
                result.Add(simulation);
            }

            return result;
        }

        protected void SetTempVariable(IReadingContext context, double? operatingTemperatureInKelvins, BaseSimulation sim)
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

            sim.OnBeforeTemperatureCalculations += (object sender, LoadStateEventArgs e) =>
            {
                context.Evaluator.SetParameter("TEMP", temp);
            };
        }

        protected string GetSimulationName(IReadingContext context, double? temperatureInKelvin = null)
        {
            if (temperatureInKelvin.HasValue)
            {
                return string.Format("{0} - {1} - at {2} Kelvins ({3} Celsius)", context.Result.Simulations.Count()+1, SpiceName, temperatureInKelvin.Value, temperatureInKelvin.Value - SpiceSharp.Circuit.CelsiusKelvin);
            }

            return string.Format("{0} - {1}", context.Result.Simulations.Count() + 1, SpiceName);
        }

        protected void SetSimulationTemperatures(BaseSimulation simulation, double? operatingTemperatureInKelvins, double? nominalTemperatureInKelvins)
        {
            if (operatingTemperatureInKelvins.HasValue)
            {
                SetCircuitTemperature(simulation, operatingTemperatureInKelvins.Value);
            }

            if (nominalTemperatureInKelvins.HasValue)
            {
                SetCircuitNominalTemperature(simulation, nominalTemperatureInKelvins.Value);
            }
        }

        /// <summary>
        /// Sets the nominal temperature of the simulation.
        /// </summary>
        /// <param name="simulation">The simulation to set.</param>
        /// <param name="nominalTemperatureInKelvins">Nominal temperature</param>
        protected static void SetCircuitNominalTemperature(BaseSimulation simulation, double nominalTemperatureInKelvins)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is RealState rs)
                {
                    rs.NominalTemperature = nominalTemperatureInKelvins;

                }
                //TODO: What to do with complex state?
            };

            simulation.OnBeforeTemperatureCalculations += setState;
        }

        /// <summary>
        /// Sets the temperature of the simulation.
        /// </summary>
        /// <param name="simulation">The simulation to set.</param>
        /// <param name="operatingTemperatureInKelvins">Circuit temperature</param>
        protected static void SetCircuitTemperature(BaseSimulation simulation, double operatingTemperatureInKelvins)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is RealState rs)
                {
                    rs.Temperature = operatingTemperatureInKelvins;
                }

                //TODO: What to do with complex state?
            };

            simulation.OnBeforeTemperatureCalculations += setState;
        }

        /// <summary>
        /// Sets the base parameters.
        /// </summary>
        /// <param name="baseConfiguration">The configuration to set.</param>
        /// <param name="context">The processing context.</param>
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
