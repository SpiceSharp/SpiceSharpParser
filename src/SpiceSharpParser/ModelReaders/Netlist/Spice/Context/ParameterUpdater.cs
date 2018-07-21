using System.Collections.Generic;
using System.Globalization;
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
                        SetIndependentSource(simulation, context, paramToSet, @object);
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

        protected void SetDeviceParameter(BaseSimulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, ReferenceParameter rp)
        {
            string objectName = rp.Name;
            string paramName = rp.Argument;
            if (context.Result.FindObject(objectName, out Entity @object))
            {
                context.SimulationContexts.SetEntityParameter(paramName.ToLower(), @object, paramToSet.Value.ToString(CultureInfo.InvariantCulture), simulation);
            }
        }

        protected void SetModelParameter(BaseSimulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, BracketParameter bp)
        {
            string modelName = bp.Name;
            string paramName = bp.Parameters[0].Image;
            if (context.Result.FindObject(modelName, out Entity @model))
            {
                context.SimulationContexts.SetModelParameter(paramName.ToLower(), model, paramToSet.Value.ToString(CultureInfo.InvariantCulture), simulation);
            }
        }

        protected void SetSimulationParameter(BaseSimulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet)
        {
            context.SimulationContexts.SetParameter(paramToSet.Key.Image, paramToSet.Value, simulation);
        }

        protected void SetIndependentSource(BaseSimulation simulation, IReadingContext context, KeyValuePair<Models.Netlist.Spice.Objects.Parameter, double> paramToSet, Entity @object)
        {
            if (@object is CurrentSource || @object is VoltageSource)
            {
                context.SimulationContexts.SetEntityParameter("dc", @object, paramToSet.Value.ToString(CultureInfo.InvariantCulture), simulation);
            }
        }
    }
}
