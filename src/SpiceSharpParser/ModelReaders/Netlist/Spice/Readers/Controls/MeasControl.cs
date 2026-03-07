using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .MEAS/.MEASURE <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class MeasControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        /// <param name="exportFactory">The export factory.</param>
        public MeasControl(IMapper<Exporter> mapper, IExportFactory exportFactory)
            : base(mapper, exportFactory)
        {
        }

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A reading context.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (statement.Parameters.Count < 3)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".MEAS statement requires at least analysis type, measurement name, and measurement specification");
                return;
            }

            var definition = ParseDefinition(statement, context);
            if (definition == null)
            {
                return;
            }

            foreach (var simulation in FilterSimulations(context.Result.Simulations, definition.AnalysisType))
            {
                SetupMeasurement(definition, simulation, context);
            }
        }

        private IEnumerable<ISimulationWithEvents> FilterSimulations(IEnumerable<ISimulationWithEvents> simulations, string type)
        {
            var typeLowered = type.ToLower();
            bool matched = false;

            foreach (var simulation in simulations)
            {
                if ((simulation is DC && typeLowered == "dc")
                    || (simulation is Transient && typeLowered == "tran")
                    || (simulation is AC && typeLowered == "ac")
                    || (simulation is OP && typeLowered == "op")
                    || (simulation is Noise && typeLowered == "noise"))
                {
                    matched = true;
                    yield return simulation;
                }
            }

            if (!matched && typeLowered != "dc" && typeLowered != "tran" && typeLowered != "ac" && typeLowered != "op" && typeLowered != "noise")
            {
                // Unknown analysis type — likely a typo
                yield break;
            }
        }

        private void SetupMeasurement(MeasurementDefinition definition, ISimulationWithEvents simulation, IReadingContext context)
        {
            var evaluator = new MeasurementEvaluator(definition);

            // Create exports for the signal(s) being measured
            Export primaryExport = null;
            Export findExport = null;

            if (definition.Type == MeasType.Param)
            {
                // PARAM measurements don't need exports - computed from other measurements
                simulation.EventAfterExecute += (_, __) =>
                {
                    ComputeParamMeasurement(definition, simulation.Name, context);
                };
                return;
            }

            // Determine which signal parameter to use for the primary data collection
            Parameter signalParam = GetPrimarySignalParameter(definition);

            if (signalParam != null)
            {
                primaryExport = GenerateExport(signalParam, context, simulation);
            }

            if (definition.Type == MeasType.FindWhen && definition.FindSignal != null)
            {
                findExport = GenerateExport(definition.FindSignal, context, simulation);
            }

            // For TRIG/TARG, we might need separate exports for trigger and target signals
            Export trigExport = null;
            Export targExport = null;

            if (definition.Type == MeasType.TrigTarg)
            {
                if (definition.TrigSignal != null)
                {
                    trigExport = GenerateExport(definition.TrigSignal, context, simulation);
                }

                if (definition.TargSignal != null && definition.TargSignal != definition.TrigSignal)
                {
                    targExport = GenerateExport(definition.TargSignal, context, simulation);
                }
                else
                {
                    targExport = trigExport;
                }
            }

            // Hook into simulation events for data collection
            if (definition.Type == MeasType.TrigTarg)
            {
                var trigData = new List<(double X, double Y)>();
                var targData = new List<(double X, double Y)>();

                simulation.EventExportData += (_, __) =>
                {
                    double x = GetIndependentVariable(simulation);
                    if (trigExport != null)
                    {
                        try { trigData.Add((x, trigExport.Extract())); }
                        catch { trigData.Add((x, double.NaN)); }
                    }

                    if (targExport != null && targExport != trigExport)
                    {
                        try { targData.Add((x, targExport.Extract())); }
                        catch { targData.Add((x, double.NaN)); }
                    }
                };

                simulation.EventAfterExecute += (_, __) =>
                {
                    var data = trigExport == targExport ? trigData : targData;

                    double? trigX = MeasurementEvaluator.FindCrossing(
                        trigData, definition.TrigVal, definition.TrigEdge, definition.TrigEdgeNumber, definition.TrigTd, null, null);
                    double? targX = MeasurementEvaluator.FindCrossing(
                        data, definition.TargVal, definition.TargEdge, definition.TargEdgeNumber, definition.TargTd, null, null);

                    MeasurementResult result;
                    if (trigX.HasValue && targX.HasValue)
                    {
                        result = new MeasurementResult(definition.Name, targX.Value - trigX.Value, true, "TRIG_TARG", simulation.Name);
                    }
                    else
                    {
                        result = new MeasurementResult(definition.Name, double.NaN, false, "TRIG_TARG", simulation.Name);
                    }

                    AddMeasurementResult(context, result);
                };
            }
            else
            {
                simulation.EventExportData += (_, __) =>
                {
                    double x = GetIndependentVariable(simulation);

                    if (primaryExport != null)
                    {
                        try { evaluator.CollectDataPoint(x, primaryExport.Extract()); }
                        catch { evaluator.CollectDataPoint(x, double.NaN); }
                    }

                    if (findExport != null)
                    {
                        try { evaluator.CollectFindDataPoint(x, findExport.Extract()); }
                        catch { evaluator.CollectFindDataPoint(x, double.NaN); }
                    }
                };

                simulation.EventAfterExecute += (_, __) =>
                {
                    var result = evaluator.ComputeResult(simulation.Name);
                    AddMeasurementResult(context, result);
                };
            }
        }

        private static Parameter GetPrimarySignalParameter(MeasurementDefinition definition)
        {
            switch (definition.Type)
            {
                case MeasType.When:
                    return definition.WhenSignal;
                case MeasType.FindWhen:
                    return definition.WhenSignal;
                case MeasType.Min:
                case MeasType.Max:
                case MeasType.Avg:
                case MeasType.Rms:
                case MeasType.Pp:
                case MeasType.Integ:
                case MeasType.Deriv:
                    return definition.Signal;
                default:
                    return null;
            }
        }

        private static double GetIndependentVariable(ISimulationWithEvents simulation)
        {
            if (simulation is Transient t)
            {
                return t.Time;
            }

            if (simulation is AC a)
            {
                return a.Frequency;
            }

            if (simulation is DC d)
            {
                var sweepValues = d.GetCurrentSweepValue();
                if (sweepValues != null && sweepValues.Any())
                {
                    return sweepValues.Last();
                }

                return 0;
            }

            return 0;
        }

        private static void AddMeasurementResult(IReadingContext context, MeasurementResult result)
        {
            context.Result.Measurements.AddOrUpdate(
                result.Name,
                _ => new List<MeasurementResult> { result },
                (_, list) =>
                {
                    lock (list)
                    {
                        list.Add(result);
                    }

                    return list;
                });
        }

        private void ComputeParamMeasurement(MeasurementDefinition definition, string simulationName, IReadingContext context)
        {
            try
            {
                // Set measurement values as parameters in the evaluation context
                var simContext = context.EvaluationContext;
                foreach (var kvp in context.Result.Measurements)
                {
                    var results = kvp.Value;

                    // Find the latest result for this simulation name
                    MeasurementResult matchingResult = null;
                    lock (results)
                    {
                        matchingResult = results.LastOrDefault(r => r.SimulationName == simulationName)
                                         ?? results.LastOrDefault();
                    }

                    if (matchingResult != null && matchingResult.Success)
                    {
                        simContext.SetParameter(kvp.Key, matchingResult.Value);
                    }
                }

                double value = context.Evaluator.EvaluateDouble(definition.ParamExpression);
                var result = new MeasurementResult(definition.Name, value, true, "PARAM", simulationName);
                AddMeasurementResult(context, result);
            }
            catch (Exception ex)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $".MEAS PARAM '{definition.Name}': Expression evaluation failed — {ex.Message}");
                var result = new MeasurementResult(definition.Name, double.NaN, false, "PARAM", simulationName);
                AddMeasurementResult(context, result);
            }
        }

        private MeasurementDefinition ParseDefinition(Control statement, IReadingContext context)
        {
            // Syntax: .MEAS <analysis_type> <name> <measurement_spec>
            // Parameters[0] = analysis type (TRAN, AC, DC, OP, NOISE)
            // Parameters[1] = measurement name
            // Parameters[2..] = measurement specification

            string analysisType = statement.Parameters[0].Value.ToUpper();
            string measName = statement.Parameters[1].Value;

            var definition = new MeasurementDefinition
            {
                Name = measName,
                AnalysisType = analysisType,
            };

            // Parse the measurement specification starting at parameter index 2
            var specParams = new List<Parameter>();
            for (int i = 2; i < statement.Parameters.Count; i++)
            {
                specParams.Add(statement.Parameters[i]);
            }

            if (specParams.Count == 0)
            {
                return null;
            }

            // Detect keyword: for AssignmentParameter like PARAM='expr', the Name is the keyword
            string keyword;
            if (specParams[0] is AssignmentParameter assignParam)
            {
                keyword = assignParam.Name.ToUpper();
            }
            else
            {
                keyword = specParams[0].Value.ToUpper();
            }

            switch (keyword)
            {
                case "TRIG":
                    return ParseTrigTarg(definition, specParams, context);
                case "WHEN":
                    return ParseWhen(definition, specParams, context);
                case "FIND":
                    return ParseFindWhen(definition, specParams, context);
                case "MIN":
                    return ParseStatistical(definition, MeasType.Min, specParams, context);
                case "MAX":
                    return ParseStatistical(definition, MeasType.Max, specParams, context);
                case "AVG":
                    return ParseStatistical(definition, MeasType.Avg, specParams, context);
                case "RMS":
                    return ParseStatistical(definition, MeasType.Rms, specParams, context);
                case "PP":
                    return ParseStatistical(definition, MeasType.Pp, specParams, context);
                case "INTEG":
                    return ParseStatistical(definition, MeasType.Integ, specParams, context);
                case "DERIV":
                    return ParseDeriv(definition, specParams, context);
                case "PARAM":
                    return ParseParam(definition, specParams);
                default:
                    context.Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $".MEAS: Unrecognized measurement type '{keyword}'");
                    return null;
            }
        }

        private MeasurementDefinition ParseTrigTarg(MeasurementDefinition definition, List<Parameter> specParams, IReadingContext context)
        {
            // .MEAS TRAN name TRIG <signal> VAL=<v> [RISE|FALL|CROSS=<n>] [TD=<t>]
            //                  TARG <signal> VAL=<v> [RISE|FALL|CROSS=<n>] [TD=<t>]
            definition.Type = MeasType.TrigTarg;

            int targIndex = -1;
            for (int i = 1; i < specParams.Count; i++)
            {
                if (specParams[i].Value.ToUpper() == "TARG")
                {
                    targIndex = i;
                    break;
                }
            }

            if (targIndex < 0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".MEAS TRIG/TARG: Missing TARG keyword");
                return null;
            }

            // Parse trigger section: TRIG <signal> [VAL=<v>] [RISE|FALL|CROSS=<n>] [TD=<t>]
            var trigParams = specParams.GetRange(1, targIndex - 1);
            ParseThresholdSection(trigParams, out Parameter trigSignal, out double trigVal, out EdgeType trigEdge, out int trigEdgeNum, out double? trigTd, context);

            definition.TrigSignal = trigSignal;
            definition.TrigVal = trigVal;
            definition.TrigEdge = trigEdge;
            definition.TrigEdgeNumber = trigEdgeNum;
            definition.TrigTd = trigTd;

            // Parse target section: TARG <signal> [VAL=<v>] [RISE|FALL|CROSS=<n>] [TD=<t>]
            var targParams = specParams.GetRange(targIndex + 1, specParams.Count - targIndex - 1);
            ParseThresholdSection(targParams, out Parameter targSignal, out double targVal, out EdgeType targEdge, out int targEdgeNum, out double? targTd, context);

            definition.TargSignal = targSignal;
            definition.TargVal = targVal;
            definition.TargEdge = targEdge;
            definition.TargEdgeNumber = targEdgeNum;
            definition.TargTd = targTd;

            return definition;
        }

        private void ParseThresholdSection(
            List<Parameter> parameters,
            out Parameter signal,
            out double val,
            out EdgeType edge,
            out int edgeNumber,
            out double? td,
            IReadingContext context)
        {
            signal = null;
            val = 0;
            edge = EdgeType.Cross;
            edgeNumber = 1;
            td = null;

            foreach (var param in parameters)
            {
                if (param is AssignmentParameter ap)
                {
                    switch (ap.Name.ToUpper())
                    {
                        case "VAL":
                            val = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                        case "RISE":
                            edge = EdgeType.Rise;
                            edgeNumber = Math.Max(1, (int)context.Evaluator.EvaluateDouble(ap.Value));
                            break;
                        case "FALL":
                            edge = EdgeType.Fall;
                            edgeNumber = Math.Max(1, (int)context.Evaluator.EvaluateDouble(ap.Value));
                            break;
                        case "CROSS":
                            edge = EdgeType.Cross;
                            edgeNumber = Math.Max(1, (int)context.Evaluator.EvaluateDouble(ap.Value));
                            break;
                        case "TD":
                            td = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                    }
                }
                else if (param is BracketParameter || param is ReferenceParameter)
                {
                    signal = param;
                }
                else if (signal == null)
                {
                    signal = param;
                }
            }
        }

        private MeasurementDefinition ParseWhen(MeasurementDefinition definition, List<Parameter> specParams, IReadingContext context)
        {
            // .MEAS TRAN name WHEN V(out)=0.5 [RISE|FALL|CROSS=<n>]
            // .MEAS TRAN name WHEN V(out) VAL=0.5 [RISE|FALL|CROSS=<n>]
            definition.Type = MeasType.When;

            ParseWhenCondition(specParams, 1, definition, context);

            return definition;
        }

        private MeasurementDefinition ParseFindWhen(MeasurementDefinition definition, List<Parameter> specParams, IReadingContext context)
        {
            // .MEAS TRAN name FIND V(out) WHEN V(in)=0.5 [RISE|FALL|CROSS=<n>]
            definition.Type = MeasType.FindWhen;

            // Find the WHEN keyword
            int whenIndex = -1;
            for (int i = 1; i < specParams.Count; i++)
            {
                if (specParams[i].Value.ToUpper() == "WHEN")
                {
                    whenIndex = i;
                    break;
                }
            }

            if (whenIndex < 0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".MEAS FIND: Missing WHEN keyword");
                return null;
            }

            // The FIND signal is between FIND keyword and WHEN keyword
            for (int i = 1; i < whenIndex; i++)
            {
                if (specParams[i] is BracketParameter || specParams[i] is ReferenceParameter)
                {
                    definition.FindSignal = specParams[i];
                    break;
                }
                else if (definition.FindSignal == null)
                {
                    definition.FindSignal = specParams[i];
                }
            }

            if (definition.FindSignal == null)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".MEAS FIND/WHEN: Missing signal between FIND and WHEN keywords");
                return null;
            }

            // Parse the WHEN condition
            var whenParams = specParams.GetRange(whenIndex, specParams.Count - whenIndex);
            ParseWhenCondition(whenParams, 1, definition, context);

            return definition;
        }

        private void ParseWhenCondition(List<Parameter> specParams, int startIndex, MeasurementDefinition definition, IReadingContext context)
        {
            if (startIndex >= specParams.Count)
            {
                return;
            }

            var param = specParams[startIndex];

            // Combined syntax: WHEN V(out)=0.5 — parsed as AssignmentParameter
            if (param is AssignmentParameter ap)
            {
                // AssignmentParameter: Name = "V", Arguments = ["out"], Values = ["0.5"]
                if (ap.HasFunctionSyntax || (ap.Arguments != null && ap.Arguments.Count > 0))
                {
                    // Reconstruct signal as a BracketParameter for export generation
                    var signalParams = new ParameterCollection(new List<Parameter>());
                    if (ap.Arguments != null)
                    {
                        foreach (var arg in ap.Arguments)
                        {
                            signalParams.Add(new WordParameter(arg, null));
                        }
                    }

                    definition.WhenSignal = new BracketParameter(ap.Name, signalParams, null);
                    definition.WhenVal = context.Evaluator.EvaluateDouble(ap.Value);
                }
                else
                {
                    // Simple assignment: just treat name as signal name
                    definition.WhenSignal = new WordParameter(ap.Name, null);
                    definition.WhenVal = context.Evaluator.EvaluateDouble(ap.Value);
                }
            }
            else if (param is BracketParameter bp)
            {
                // Separate syntax: WHEN V(out) VAL=0.5 CROSS=1
                definition.WhenSignal = bp;
            }
            else
            {
                // Fallback: treat as word parameter for signal name
                definition.WhenSignal = param;
            }

            // Parse edge/window qualifiers
            for (int i = startIndex + 1; i < specParams.Count; i++)
            {
                if (specParams[i] is AssignmentParameter edgeAp)
                {
                    switch (edgeAp.Name.ToUpper())
                    {
                        case "VAL":
                            definition.WhenVal = context.Evaluator.EvaluateDouble(edgeAp.Value);
                            break;
                        case "RISE":
                            definition.WhenEdge = EdgeType.Rise;
                            definition.WhenEdgeNumber = Math.Max(1, (int)context.Evaluator.EvaluateDouble(edgeAp.Value));
                            break;
                        case "FALL":
                            definition.WhenEdge = EdgeType.Fall;
                            definition.WhenEdgeNumber = Math.Max(1, (int)context.Evaluator.EvaluateDouble(edgeAp.Value));
                            break;
                        case "CROSS":
                            definition.WhenEdge = EdgeType.Cross;
                            definition.WhenEdgeNumber = Math.Max(1, (int)context.Evaluator.EvaluateDouble(edgeAp.Value));
                            break;
                        case "FROM":
                            definition.From = context.Evaluator.EvaluateDouble(edgeAp.Value);
                            break;
                        case "TO":
                            definition.To = context.Evaluator.EvaluateDouble(edgeAp.Value);
                            break;
                    }
                }
            }
        }

        private MeasurementDefinition ParseStatistical(MeasurementDefinition definition, MeasType type, List<Parameter> specParams, IReadingContext context)
        {
            // .MEAS TRAN name MAX V(out) [FROM=<t1>] [TO=<t2>]
            definition.Type = type;

            // Signal is the first non-keyword parameter
            for (int i = 1; i < specParams.Count; i++)
            {
                var param = specParams[i];
                if (param is AssignmentParameter ap)
                {
                    switch (ap.Name.ToUpper())
                    {
                        case "FROM":
                            definition.From = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                        case "TO":
                            definition.To = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                    }
                }
                else if (definition.Signal == null)
                {
                    definition.Signal = param;
                }
            }

            if (definition.Signal == null)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $".MEAS {type}: Missing signal parameter");
                return null;
            }

            return definition;
        }

        private MeasurementDefinition ParseDeriv(MeasurementDefinition definition, List<Parameter> specParams, IReadingContext context)
        {
            // .MEAS TRAN name DERIV V(out) AT=<t>
            definition.Type = MeasType.Deriv;

            for (int i = 1; i < specParams.Count; i++)
            {
                var param = specParams[i];
                if (param is AssignmentParameter ap)
                {
                    switch (ap.Name.ToUpper())
                    {
                        case "AT":
                            definition.At = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                        case "FROM":
                            definition.From = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                        case "TO":
                            definition.To = context.Evaluator.EvaluateDouble(ap.Value);
                            break;
                    }
                }
                else if (definition.Signal == null)
                {
                    definition.Signal = param;
                }
            }

            if (definition.Signal == null)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".MEAS DERIV: Missing signal parameter");
                return null;
            }

            if (!definition.At.HasValue)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    ".MEAS DERIV: Missing required AT= parameter");
                return null;
            }

            return definition;
        }

        private MeasurementDefinition ParseParam(MeasurementDefinition definition, List<Parameter> specParams)
        {
            // .MEAS TRAN name PARAM='expression' or PARAM {expression}
            definition.Type = MeasType.Param;

            // Case 1: PARAM='expr' parsed as AssignmentParameter — expression is in the Value
            if (specParams[0] is AssignmentParameter ap)
            {
                string expr = ap.Value;
                if (expr.StartsWith("'") && expr.EndsWith("'"))
                {
                    expr = expr.Substring(1, expr.Length - 2);
                }

                definition.ParamExpression = expr;
            }
            else if (specParams.Count > 1)
            {
                // Case 2: PARAM 'expression' — separate parameters
                string expr = specParams[1].Value;
                if (expr.StartsWith("'") && expr.EndsWith("'"))
                {
                    expr = expr.Substring(1, expr.Length - 2);
                }

                definition.ParamExpression = expr;
            }

            if (string.IsNullOrWhiteSpace(definition.ParamExpression))
            {
                return null;
            }

            return definition;
        }
    }
}
