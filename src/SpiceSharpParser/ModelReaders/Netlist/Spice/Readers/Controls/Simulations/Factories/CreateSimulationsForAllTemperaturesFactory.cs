using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForAllTemperaturesFactory : ICreateSimulationsForAllTemperaturesFactory
    {
        public List<Simulation> CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, Simulation> createSimulation)
        {
            var result = new List<Simulation>();

            if (context.SimulationConfiguration.TemperaturesInKelvins.Count > 0)
            {
                foreach (double temp in context.SimulationConfiguration.TemperaturesInKelvins)
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

        protected Simulation CreateSimulationForTemperature(Control statement, IReadingContext context, Func<string, Control, IReadingContext, Simulation> createSimulation, double? temp)
        {
            var simulation = createSimulation(GetSimulationName(context, statement, temp), statement, context);

            SetTempVariable(context, temp, simulation);
            SetSimulationTemperatures(simulation, temp, context.SimulationConfiguration.NominalTemperatureInKelvins);

            return simulation;
        }

        protected void SetTempVariable(IReadingContext context, double? operatingTemperatureInKelvins, Simulation simulation)
        {
            double temp;
            if (operatingTemperatureInKelvins.HasValue)
            {
                temp = operatingTemperatureInKelvins.Value - Constants.CelsiusKelvin;
            }
            else
            {
                temp = Constants.ReferenceTemperature - Constants.CelsiusKelvin;
            }

            if (simulation is BiasingSimulation biasingSimulation)
            {
                biasingSimulation.BeforeTemperature += (_, _) =>
                {
                    context.EvaluationContext.SetParameter("TEMP", temp, simulation);
                };
            }
        }

        protected void SetSimulationTemperatures(Simulation simulation, double? operatingTemperatureInKelvins, double? nominalTemperatureInKelvins)
        {
            if (operatingTemperatureInKelvins.HasValue)
            {
                var decorator = new CircuitTemperatureSimulationDecorator(operatingTemperatureInKelvins.Value);
                decorator.Decorate(simulation);
            }

            if (nominalTemperatureInKelvins.HasValue)
            {
                var decorator = new NominalTemperatureSimulationDecorator(nominalTemperatureInKelvins.Value);
                decorator.Decorate(simulation);
            }
        }

        protected string GetSimulationName(IReadingContext context, Control statement, double? temperatureInKelvin = null)
        {
            if (temperatureInKelvin.HasValue)
            {
                return string.Format("#{0} {1} - at {2} Kelvins ({3} Celsius)", context.Result.Simulations.Count() + 1, statement.Name, temperatureInKelvin.Value, temperatureInKelvin.Value - Constants.CelsiusKelvin);
            }

            return string.Format("#{0} {1}", context.Result.Simulations.Count() + 1, statement.Name);
        }
    }
}