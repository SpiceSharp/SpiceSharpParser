using SpiceSharp;
using SpiceSharp.IntegrationMethods;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .OPTIONS <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class OptionsControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.AssignmentParameter a)
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
                            double temp = context.ParseDouble(value) + Circuit.CelsiusKelvin;
                            context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions = temp;
                            context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(temp); break;
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
                        case "seed":
                            context.Result.SimulationConfiguration.Seed = int.Parse(value);
                            context.Result.Seed = context.Result.SimulationConfiguration.Seed.Value;
                            context.ReadingEvaluator.Seed = context.Result.Seed; 
                            break;
                        default:
                            context.Result.AddWarning("Unsupported option: " + name);
                            break;
                    }
                }

                if (param is Models.Netlist.Spice.Objects.Parameters.WordParameter w)
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
