using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .OPTIONS <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class OptionsControl : BaseControl
    {
        private static readonly HashSet<string> LtspiceWarningNoOpOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "plotwinsize",
            "plotreltol",
            "plotvntol",
            "plotabstol",
            "numdgt",
            "measdgt",
            "meascplxfmt",
            "baudrate",
            "fastaccess",
        };

        private static readonly HashSet<string> LtspiceBehaviorChangingOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cshunt",
            "gshunt",
            "srcsteps",
            "gminsteps",
            "trtol",
            "chgtol",
            "pivrel",
            "pivtol",
            "ptrantau",
        };

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is AssignmentParameter a)
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
                                context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, "CDFPOINTS needs to be greater than 3", statement.LineInfo);
                                return;
                            }

                            context.EvaluationContext.Randomizer.CdfPoints = points;
                            break;

                        case "normallimit":
                            context.EvaluationContext.Randomizer.NormalLimit = context.EvaluationContext.Evaluator.EvaluateDouble(value);
                            break;

                        default:
                            AddUnsupportedOption(name, statement, context);
                            break;
                    }
                }

                if (param is WordParameter w)
                {
                    if (w.Value.ToLower() == "keepopinfo")
                    {
                        context.SimulationConfiguration.KeepOpInfo = true;
                    }
                    else if (context.ReaderSettings.Compatibility.IsLTspice)
                    {
                        AddUnsupportedOption(w.Value, statement, context);
                    }
                }
            }
        }

        private static void AddUnsupportedOption(string name, Control statement, IReadingContext context)
        {
            if (context.ReaderSettings.Compatibility.IsLTspice)
            {
                if (LtspiceWarningNoOpOptions.Contains(name))
                {
                    context.Result.ValidationResult.AddWarning(
                        ValidationEntrySource.Reader,
                        $"Ignored LTspice option '{name}': output/viewer option is not used by SpiceSharpParser.",
                        statement.LineInfo);
                    return;
                }

                if (LtspiceBehaviorChangingOptions.Contains(name))
                {
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Unsupported LTspice option '{name}': behavior-changing solver option is not mapped yet.",
                        statement.LineInfo);
                    return;
                }
            }

            context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Unsupported option: {name}", statement.LineInfo);
        }
    }
}
