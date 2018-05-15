using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharp.IntegrationMethods;
using SpiceSharp;

namespace SpiceSharpParser.Connector.Processors.Controls
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
        public override void Process(Control statement, IProcessingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is Model.SpiceObjects.Parameters.AssignmentParameter a)
                {
                    string name = a.Name.ToLower();
                    string value = a.Value;

                    switch (name)
                    {
                        case "abstol":
                            context.Result.SimulationConfiguration.AbsoluteTolerance = context.ParseDouble(value); break;
                        case "reltol":
                            context.Result.SimulationConfiguration.RelTolerance = context.ParseDouble(value); break;
                        case "gmin":
                            context.Result.SimulationConfiguration.Gmin = context.ParseDouble(value); break;
                        case "itl1":
                            context.Result.SimulationConfiguration.DCMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl2":
                            context.Result.SimulationConfiguration.SweepMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl4":
                            context.Result.SimulationConfiguration.TranMaxIterations = (int)context.ParseDouble(value); break;
                        case "itl5":
                            // TODO: ????
                            break;
                        case "temp":
                            context.Result.SimulationConfiguration.TemperatureInKelvins = context.ParseDouble(value) + Circuit.CelsiusKelvin; break;
                        case "tnom":
                            context.Result.SimulationConfiguration.NominalTemperatureInKelvins = context.ParseDouble(value) + Circuit.CelsiusKelvin; break;
                        case "method":
                            switch (value.ToLower())
                            {
                                case "trap":
                                case "trapezoidal":
                                    context.Result.SimulationConfiguration.Method = new Trapezoidal();
                                    break;
                            }

                            break;
                        default:
                            context.Result.AddWarning("Unsupported option: " + name);
                            break;
                    }
                }

                if (param is Model.SpiceObjects.Parameters.WordParameter w)
                {
                    if (w.Image.ToLower() == "keepopinfo")
                    {
                        context.Result.SimulationConfiguration.KeepOpInfo = true;
                    }
                }
            }
        }
    }
}
