using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

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
            BaseSimulation simulation,
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

        protected void UpdateSweep(BaseSimulation simulation, IReadingContext context, List<KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double>> parameterValues)
        {
            foreach (var paramToSet in parameterValues)
            {
                if (paramToSet.Key is WordParameter || paramToSet.Key is IdentifierParameter)
                {
                    if (context.Result.FindObject(paramToSet.Key.Image, out Entity @object))
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

        protected void SetDeviceParameter(BaseSimulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet, ReferenceParameter rp)
        {
            string objectName = rp.Name;
            string paramName = rp.Argument;
            if (context.Result.FindObject(objectName, out Entity @object))
            {
                context.SimulationPreparations.SetParameter(@object, simulation, paramName, paramToSet.Value, true, false);
            }
        }

        protected void SetModelParameter(BaseSimulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet, BracketParameter bp)
        {
            string modelName = bp.Name;
            string paramName = bp.Parameters[0].Image;
            if (context.Result.FindObject(modelName, out Entity @model))
            {
                context
                    .SimulationPreparations
                    .SetParameter(
                        model,
                        simulation,
                        paramName,
                        paramToSet.Value,
                        true,
                        false);
            }
        }

        protected void SetSimulationParameter(BaseSimulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            simulation.BeforeSetup += (object sender, EventArgs args) =>
            {
                    context.SimulationExpressionContexts.GetContext(simulation).SetParameter(
                        paramToSet.Key.Image,
                        paramToSet.Value);
            };
        }

        protected void SetIndependentSource(Entity @entity, BaseSimulation simulation, IReadingContext context, KeyValuePair<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            if (@entity is CurrentSource || @entity is VoltageSource)
            {
                context
                    .SimulationPreparations
                    .SetParameter(@entity, simulation, "dc", paramToSet.Value, true, false);
            }
        }
    }
}
