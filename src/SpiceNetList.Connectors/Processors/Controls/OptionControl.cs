using System;
using SpiceNetlist.SpiceObjects;
using SpiceSharp.IntegrationMethods;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    class OptionControl : SingleControlProcessor
    {
        public override void Process(Control statement, ProcessingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is SpiceObjects.Parameters.AssignmentParameter a)
                {
                    string name = a.Name.ToLower();
                    string value = a.Value;

                    switch (name)
                    {
                        case "abstol":
                            if (context.BaseConfiguration == null) context.BaseConfiguration = new SpiceSharp.Simulations.BaseConfiguration();
                            context.BaseConfiguration.AbsoluteTolerance = context.ParseDouble(value); break;
                        case "reltol":
                            if (context.BaseConfiguration == null) context.BaseConfiguration = new SpiceSharp.Simulations.BaseConfiguration();
                            context.BaseConfiguration.AbsoluteTolerance = context.ParseDouble(value); break;
                        case "gmin":
                            if (context.BaseConfiguration == null) context.BaseConfiguration = new SpiceSharp.Simulations.BaseConfiguration();
                            context.BaseConfiguration.Gmin = context.ParseDouble(value); break;
                        case "itl1":
                            if (context.BaseConfiguration == null) context.BaseConfiguration = new SpiceSharp.Simulations.BaseConfiguration();
                            context.BaseConfiguration.DCMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl2":
                            if (context.DCConfiguration == null) context.DCConfiguration = new SpiceSharp.Simulations.DCConfiguration();
                            context.DCConfiguration.SweepMaxIterations = (int)context.ParseDouble(value); break;

                        case "itl4":
                            if (context.TimeConfiguration == null) context.TimeConfiguration = new SpiceSharp.Simulations.TimeConfiguration();
                            context.TimeConfiguration.TranMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl5":
                            //TODO: ????
                            break;
                        case "temp":
                            // TODO: Set current temperature
                            break;

                        case "tnom":
                            // TODO: Set nominal temperature
                            break;

                        case "method":
                            switch (value.ToLower())
                            {
                                case "trap":
                                case "trapezoidal":
                                    if (context.TimeConfiguration == null) context.TimeConfiguration = new SpiceSharp.Simulations.TimeConfiguration();
                                    context.TimeConfiguration.Method = new Trapezoidal();
                                    break;
                            }
                            break;

                        default:
                            throw new Exception();
                    }
                }

                if (param is SpiceObjects.Parameters.WordParameter w)
                {
                    if (w.RawValue.ToLower() == "keepopinfo")
                    {
                        if (context.FrequencyConfiguration == null) context.FrequencyConfiguration = new SpiceSharp.Simulations.FrequencyConfiguration();
                        context.FrequencyConfiguration.KeepOpInfo = true;
                    }
                }
            }
        }
    }
}
