using SpiceSharp;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharpParser.Common.Validation;
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
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
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
                            context.SimulationConfiguration.AbsoluteTolerance = context.EvaluationContext.Evaluator.EvaluateDouble(value); break;
                        case "reltol":
                            context.SimulationConfiguration.RelTolerance = context.EvaluationContext.Evaluator.EvaluateDouble(value); break;
                        case "gmin":
                            context.SimulationConfiguration.Gmin = context.EvaluationContext.Evaluator.EvaluateDouble(value); break;
                        case "itl1":
                            context.SimulationConfiguration.DCMaxIterations = (int)context.EvaluationContext.Evaluator.EvaluateDouble(value); break;
                        case "itl2":
                            context.SimulationConfiguration.SweepMaxIterations = (int)context.EvaluationContext.Evaluator.EvaluateDouble(value); break;
                        case "itl4":
                            context.SimulationConfiguration.TransientConfiguration.TranMaxIterations = (int)context.EvaluationContext.Evaluator.EvaluateDouble(value); break;
                        case "itl5":
                            // TODO: ????
                            break;

                        case "temp":
                            double temp = context.EvaluationContext.Evaluator.EvaluateDouble(value) + Constants.CelsiusKelvin;
                            context.SimulationConfiguration.TemperaturesInKelvinsFromOptions = temp;
                            context.SimulationConfiguration.TemperaturesInKelvins.Add(temp); break;
                        case "tnom":
                            context.SimulationConfiguration.NominalTemperatureInKelvins = context.EvaluationContext.Evaluator.EvaluateDouble(value) + Constants.CelsiusKelvin; break;
                        case "method":
                            switch (value.ToLower())
                            {
                                case "trap":
                                case "trapezoidal":
                                    context.SimulationConfiguration.TransientConfiguration.Type = typeof(Trapezoidal);
                                    context.SimulationConfiguration.TimeParametersFactory = (config) => new Trapezoidal()
                                    {
                                        StartTime = config.Start ?? 0.0,
                                        StopTime = config.Final ?? 0.0,
                                        MaxStep = config.MaxStep ?? 0.0,
                                        InitialStep = config.Step ?? 0.0,
                                        UseIc = config.UseIc ?? false,
                                        AbsoluteTolerance = context.SimulationConfiguration.AbsoluteTolerance ?? 1e-12,
                                        RelativeTolerance = context.SimulationConfiguration.RelTolerance ?? 1e-3,
                                    };
                                    break;

                                case "gear":
                                    context.SimulationConfiguration.TransientConfiguration.Type = typeof(Gear);
                                    context.SimulationConfiguration.TimeParametersFactory = (config) => new Gear()
                                    {
                                        StartTime = config.Start ?? 0.0,
                                        StopTime = config.Final ?? 0.0,
                                        MaxStep = config.MaxStep ?? 0.0,
                                        InitialStep = config.Step ?? 0.0,
                                        UseIc = config.UseIc ?? false,
                                        AbsoluteTolerance = context.SimulationConfiguration.AbsoluteTolerance ?? 1e-12,
                                        RelativeTolerance = context.SimulationConfiguration.RelTolerance ?? 1e-3,
                                    };
                                    break;

                                case "euler":
                                    context.SimulationConfiguration.TransientConfiguration.Type = typeof(FixedEuler);
                                    context.SimulationConfiguration.TimeParametersFactory = (config) => new FixedEuler()
                                    {
                                        StartTime = config.Start ?? 0.0,
                                        StopTime = config.Final ?? 0.0,
                                        Step = config.Step ?? 0.0,
                                        UseIc = config.UseIc ?? false,
                                    };
                                    break;
                            }

                            break;

                        case "seed":
                            var seed = int.Parse(value);
                            context.SimulationConfiguration.Seed = seed;
                            context.EvaluationContext.Seed = seed;
                            break;

                        case "distribution":
                            context.EvaluationContext.Randomizer.CurrentPdfName = value;
                            break;

                        case "localsolver":
                            context.SimulationConfiguration.LocalSolver = value == "on";
                            break;

                        case "cdfpoints":
                            var points = (int)context.EvaluationContext.Evaluator.EvaluateDouble(value);

                            if (points < 4)
                            {
                                context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "CDFPOINTS needs to be greater than 3", statement.LineInfo));
                                return;
                            }

                            context.EvaluationContext.Randomizer.CdfPoints = points;
                            break;

                        case "normallimit":
                            context.EvaluationContext.Randomizer.NormalLimit = context.EvaluationContext.Evaluator.EvaluateDouble(value);
                            break;

                        default:
                            context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, $"Unsupported option: {name}", statement.LineInfo));
                            break;
                    }
                }

                if (param is Models.Netlist.Spice.Objects.Parameters.WordParameter w)
                {
                    if (w.Value.ToLower() == "keepopinfo")
                    {
                        context.SimulationConfiguration.KeepOpInfo = true;
                    }
                }
            }
        }
    }
}