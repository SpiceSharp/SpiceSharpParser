using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForAllTemperaturesFactory : ICreateSimulationsForAllTemperaturesFactory
    {
        public IEnumerable<BaseSimulation> CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation)
        {
            var result = new List<BaseSimulation>();

            if (context.Result.SimulationConfiguration.TemperaturesInKelvins.Count > 0)
            {
                foreach (double temp in context.Result.SimulationConfiguration.TemperaturesInKelvins)
                {
                    result.Add(CreateSimulationForTemperature(statement, context, createSimulation, temp));
                }
            }
            else
            {
                result.Add(CreateSimulationForTemperature(statement, context, createSimulation, null));
            }

            return result;
        }

        protected BaseSimulation CreateSimulationForTemperature(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation, double? temp)
        {
            var simulation = createSimulation(GetSimulationName(context, statement, temp), statement, context);

            SetTempVariable(context, temp, simulation);
            SetSimulationTemperatures(simulation, temp, context.Result.SimulationConfiguration.NominalTemperatureInKelvins);

            return simulation;
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

            simulation.BeforeTemperature += (object sender, LoadStateEventArgs e) =>
            {
                var evaluator = context.SimulationContexts.GetSimulationEvaluator(simulation);
                evaluator.SetParameter("TEMP", temp);
            };
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

        protected string GetSimulationName(IReadingContext context, Control statement, double? temperatureInKelvin = null)
        {
            if (temperatureInKelvin.HasValue)
            {
                return string.Format("#{0} {1} - at {2} Kelvins ({3} Celsius)", context.Result.Simulations.Count() + 1, statement.Name, temperatureInKelvin.Value, temperatureInKelvin.Value - SpiceSharp.Circuit.CelsiusKelvin);
            }

            return string.Format("#{0} {1}", context.Result.Simulations.Count() + 1, statement.Name);
        }
    }
}
