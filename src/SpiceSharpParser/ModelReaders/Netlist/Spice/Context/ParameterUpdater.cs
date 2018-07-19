using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class ParameterUpdater : IParameterUpdater
    {
        public void Update(IReadingContext context, List<KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double>> parameterValues, BaseSimulation simulation)
        {
            foreach (var paramToSet in parameterValues)
            {
                if (paramToSet.Key is WordParameter || paramToSet.Key is IdentifierParameter)
                {
                    if (context.Result.FindObject(paramToSet.Key.Image, out Entity @object))
                    {
                        SetIndependentSource(simulation, paramToSet, @object);
                    }
                }

                if (paramToSet.Key is ReferenceParameter rp)
                {
                    UpdateDeviceParameter(simulation, context, paramToSet, rp);
                }

                if (paramToSet.Key is BracketParameter bp)
                {
                    UpdateModelParameter(simulation, context, paramToSet, bp);
                }

                UpdateSimulationParameter(simulation, context, paramToSet);
            }
        }

        protected void UpdateDeviceParameter(Simulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, ReferenceParameter rp)
        {
            string objectName = rp.Name;
            string paramName = rp.Argument;
            if (context.Result.FindObject(objectName, out Entity @object))
            {
                context.SetEntityParameter(@object, paramName, paramToSet.Value.ToString(), simulation);
            }
        }

        protected void UpdateModelParameter(Simulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, BracketParameter bp)
        {
            string modelName = bp.Name;
            string paramName = bp.Parameters[0].Image;
            if (context.Result.FindObject(modelName, out Entity @model))
            {
                context.SetEntityParameter(model, paramName, paramToSet.Value.ToString(), simulation);
            }
        }

        protected void UpdateSimulationParameter(Simulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            context.GetSimulationEvaluator(simulation).SetParameter(paramToSet.Key.Image, paramToSet.Value, simulation);
        }

        protected void SetIndependentSource(Simulation simulation, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, Entity @object)
        {
            if (@object is VoltageSource vs)
            {
                simulation.EntityParameters.GetEntityParameters(@object.Name).SetParameter("dc", paramToSet.Value);
            }

            if (@object is CurrentSource cs)
            {
                simulation.EntityParameters.GetEntityParameters(@object.Name).SetParameter("dc", paramToSet.Value);
            }
        }
    }
}
