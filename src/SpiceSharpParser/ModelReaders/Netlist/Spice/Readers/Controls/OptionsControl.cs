using SpiceSharp;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .OPTIONS <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class OptionsControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
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
                            context.Result.SimulationConfiguration.AbsoluteTolerance = context.Evaluator.EvaluateDouble(value); break;
                        case "reltol":
                            context.Result.SimulationConfiguration.RelTolerance = context.Evaluator.EvaluateDouble(value); break;
                        case "gmin":
                            context.Result.SimulationConfiguration.Gmin = context.Evaluator.EvaluateDouble(value); break;
                        case "itl1":
                            context.Result.SimulationConfiguration.DCMaxIterations = (int)context.Evaluator.EvaluateDouble(value); break;
                        case "itl2":
                            context.Result.SimulationConfiguration.SweepMaxIterations = (int)context.Evaluator.EvaluateDouble(value); break;
                        case "itl4":
                            context.Result.SimulationConfiguration.TransientConfiguration.TranMaxIterations = (int)context.Evaluator.EvaluateDouble(value); break;
                        case "itl5":
                            // TODO: ????
                            break;

                        case "temp":
                            double temp = context.Evaluator.EvaluateDouble(value) + Constants.CelsiusKelvin;
                            context.Result.SimulationConfiguration.TemperaturesInKelvinsFromOptions = temp;
                            context.Result.SimulationConfiguration.TemperaturesInKelvins.Add(temp); break;
                        case "tnom":
                            context.Result.SimulationConfiguration.NominalTemperatureInKelvins = context.Evaluator.EvaluateDouble(value) + Constants.CelsiusKelvin; break;
                        case "method":
                            switch (value.ToLower())
                            {
                                case "trap":
                                case "trapezoidal":
                                    context.Result.SimulationConfiguration.TransientConfiguration.Type = typeof(Trapezoidal);
                                    context.Result.SimulationConfiguration.TimeParametersFactory = (TransientConfiguration config) => new Trapezoidal()
                                    {
                                        StopTime = config.Final ?? 0.0,
                                        MaxStep = config.MaxStep ?? 0.0,
                                        InitialStep = config.Step ?? 0.0,
                                        UseIc = config.UseIc ?? false,
                                        AbsoluteTolerance = context.Result.SimulationConfiguration.AbsoluteTolerance ?? 1e-12,
                                        RelativeTolerance = context.Result.SimulationConfiguration.RelTolerance ?? 1e-3,
                                    };
                                    break;

                                case "gear":
                                    context.Result.SimulationConfiguration.TransientConfiguration.Type = typeof(Gear);
                                    context.Result.SimulationConfiguration.TimeParametersFactory = (TransientConfiguration config) => new Gear()
                                    {
                                        StopTime = config.Final ?? 0.0,
                                        MaxStep = config.MaxStep ?? 0.0,
                                        InitialStep = config.Step ?? 0.0,
                                        UseIc = config.UseIc ?? false,
                                        AbsoluteTolerance = context.Result.SimulationConfiguration.AbsoluteTolerance ?? 1e-12,
                                        RelativeTolerance = context.Result.SimulationConfiguration.RelTolerance ?? 1e-3,
                                    };
                                    break;

                                case "euler":
                                    context.Result.SimulationConfiguration.TransientConfiguration.Type = typeof(FixedEuler);
                                    context.Result.SimulationConfiguration.TimeParametersFactory = (TransientConfiguration config) => new FixedEuler()
                                    {
                                        StopTime = config.Final ?? 0.0,
                                        Step = config.Step ?? 0.0,
                                        UseIc = config.UseIc ?? false,
                                    };
                                    break;
                            }

                            break;

                        case "seed":
                            var seed = int.Parse(value);
                            context.Result.SimulationConfiguration.Seed = seed;
                            context.Evaluator.Seed = seed;
                            break;

                        case "distribution":
                            context.Evaluator.GetEvaluationContext().Randomizer.CurrentPdfName = value;
                            break;

                        case "cdfpoints":
                            var points = (int)context.Evaluator.EvaluateDouble(value);

                            if (points < 4)
                            {
                                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "CDFPOINTS needs to be greater than 3", statement.LineInfo));
                                return;
                            }

                            context.Evaluator.GetEvaluationContext().Randomizer.CdfPoints = points;
                            break;

                        case "normallimit":
                            context.Evaluator.GetEvaluationContext().Randomizer.NormalLimit = context.Evaluator.EvaluateDouble(value);
                            break;

                        default:
                            context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported option: {name}", statement.LineInfo));
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