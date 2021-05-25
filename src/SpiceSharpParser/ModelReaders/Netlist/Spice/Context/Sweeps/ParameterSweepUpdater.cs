using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Sweeps
{
    public class ParameterSweepUpdater : IParameterSweepUpdater
    {
        /// <summary>
        /// Sets sweep parameters for the simulation.
        /// </summary>
        /// <param name="simulation">Simulation to set.</param>
        /// <param name="context">Reading context.</param>
        /// <param name="parameterValues">Parameter values.</param>
        public void Update(
            Simulation simulation,
            IReadingContext context,
            List<KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double>> parameterValues)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (parameterValues == null)
            {
                throw new ArgumentNullException(nameof(parameterValues));
            }

            UpdateSweep(simulation, context, parameterValues);
        }

        protected void UpdateSweep(Simulation simulation, IReadingContext context, List<KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double>> parameterValues)
        {
            foreach (var paramToSet in parameterValues)
            {
                if (paramToSet.Key is WordParameter || paramToSet.Key is IdentifierParameter)
                {
                    if (context.FindObject(paramToSet.Key.Value, out IEntity @object))
                    {
                        SetIndependentSource(@object, simulation, context, paramToSet);
                    }
                }

                if (paramToSet.Key is ReferenceParameter rp)
                {
                    SetDeviceParameter(simulation, context, paramToSet, rp);
                }

                if (paramToSet.Key is BracketParameter bp)
                {
                    SetModelParameter(simulation, context, paramToSet, bp);
                }

                SetSimulationParameter(simulation, context, paramToSet);
            }
        }

        protected void SetDeviceParameter(Simulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet, ReferenceParameter rp)
        {
            string objectName = rp.Name;
            string paramName = rp.Argument;
            if (context.FindObject(objectName, out IEntity @object))
            {
                context.SimulationPreparations.SetParameterBeforeTemperature(@object, paramName, paramToSet.Value, simulation);
            }
        }

        protected void SetModelParameter(Simulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet, BracketParameter bp)
        {
            string modelName = bp.Name;
            string paramName = bp.Parameters[0].Value;
            if (context.FindObject(modelName, out IEntity @model))
            {
                context
                    .SimulationPreparations
                    .SetParameterBeforeTemperature(
                        model,
                        paramName,
                        paramToSet.Value,
                        simulation);
            }
        }

        protected void SetSimulationParameter(Simulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            simulation.BeforeSetup += (_, _) =>
            {
                context.EvaluationContext.SetParameter(paramToSet.Key.Value, paramToSet.Value, simulation);
            };
        }

        protected void SetIndependentSource(IEntity @entity, Simulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            if (@entity is CurrentSource || @entity is VoltageSource)
            {
                context
                    .SimulationPreparations
                    .SetParameterBeforeTemperature(@entity, "dc", paramToSet.Value, simulation);
            }
        }
    }
}