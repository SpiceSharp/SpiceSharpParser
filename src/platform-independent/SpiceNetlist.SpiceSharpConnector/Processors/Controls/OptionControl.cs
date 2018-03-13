using SpiceNetlist.SpiceObjects;
using SpiceSharp.IntegrationMethods;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    /// <summary>
    /// Processes .OPTIONS <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class OptionControl : BaseControl
    {
        public override string TypeName => "options";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, ProcessingContextBase context)
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
                            context.SimulationConfiguration.AbsoluteTolerance = context.ParseDouble(value); break;
                        case "reltol":
                            context.SimulationConfiguration.RelTolerance = context.ParseDouble(value); break;
                        case "gmin":
                            context.SimulationConfiguration.Gmin = context.ParseDouble(value); break;
                        case "itl1":
                            context.SimulationConfiguration.DCMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl2":
                            context.SimulationConfiguration.SweepMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl4":
                            context.SimulationConfiguration.TranMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl5":
                            // TODO: ????
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
                                    context.SimulationConfiguration.Method = new Trapezoidal();
                                    break;
                            }

                            break;
                        default:
                            context.Adder.AddWarning("Unsupported option: " + name);
                            break;
                    }
                }

                if (param is SpiceObjects.Parameters.WordParameter w)
                {
                    if (w.Image.ToLower() == "keepopinfo")
                    {
                        context.SimulationConfiguration.KeepOpInfo = true;
                    }
                }
            }
        }
    }
}
