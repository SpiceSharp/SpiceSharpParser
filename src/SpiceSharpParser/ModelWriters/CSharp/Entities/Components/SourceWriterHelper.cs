using SpiceSharp.Components;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public static class SourceWriterHelper
    {
        public static void CreateBehavioralCurrentSource(List<CSharpStatement> result, string name, ParameterCollection pins, string expression, IWriterContext context)
        {
            var currentSourceId = context.GetNewIdentifier(name);
            result.Add(new CSharpNewStatement(currentSourceId, $@"new BehavioralCurrentSource(""{name}"")"));
            result.Add(new CSharpCallStatement(currentSourceId, $@"Connect(""{pins[0].Value}"", ""{pins[1].Value}"")"));
            var transformed = context.EvaluationContext.Transform(expression);
            result.Add(new CSharpAssignmentStatement($@"{currentSourceId}.Parameters.Expression", @$"$""{transformed}"""));
        }

        public static void CreateBehavioralVoltageSource(List<CSharpStatement> result, string name, ParameterCollection pins, string expression, IWriterContext context)
        {
            var currentSourceId = context.GetNewIdentifier(name);
            result.Add(new CSharpNewStatement(currentSourceId, $@"new BehavioralVoltageSource(""{name}"")"));
            result.Add(new CSharpCallStatement(currentSourceId, $@"Connect(""{pins[0].Value}"", ""{pins[1].Value}"")"));
            var transformed = context.EvaluationContext.Transform(expression);

            result.Add(new CSharpAssignmentStatement($@"{currentSourceId}.Parameters.Expression", @$"$""{transformed}"""));
        }

        public static void CreateCustomCurrentSource(List<CSharpStatement> result, string name, ParameterCollection parameters, IWriterContext context, bool isVoltageControlled)
        {
            var resultIntialCount = result.Count;

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                string expression = valueParameter.Value;

                CreateBehavioralCurrentSource(result, name, parameters, expression, context);
            }

            if (parameters.Any(p => p is WordParameter ap && ap.Value.ToLower() == "value"))
            {
                var expressionParameter = parameters.FirstOrDefault(p => p is ExpressionParameter);
                if (expressionParameter != null)
                {
                    var expression = expressionParameter.Value;

                    CreateBehavioralCurrentSource(result, name, parameters, expression, context);
                }
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Value.ToLower() == "poly"))
            {
                var dimension = 1;
                var expression = CreatePolyExpression(dimension, parameters.Skip(BehavioralCurrentSource.BehavioralCurrentSourcePinCount + 1), isVoltageControlled, context.EvaluationContext);
                CreateBehavioralCurrentSource(result, name, parameters, expression, context);
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var polyParameter = (BracketParameter)parameters.Single(p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    result.Add(new CSharpComment("Error: POLY(n) expects one argument => dimension, " + name + " " + parameters));
                }

                var dimension = (int)context.EvaluationContext.Evaluate(polyParameter.Parameters[0].Value);
                var expression = CreatePolyExpression(dimension, parameters.Skip(BehavioralCurrentSource.BehavioralCurrentSourcePinCount + 1), isVoltageControlled, context.EvaluationContext);
                CreateBehavioralCurrentSource(result, name, parameters, expression, context);
            }

            var tableParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "table");
            if (tableParameter != null)
            {
                int tableParameterPosition = parameters.IndexOf(tableParameter);
                if (tableParameterPosition == parameters.Count - 1)
                {
                    result.Add(new CSharpComment("Error: TABLE expects expression parameter, " + name + " " + parameters));
                }

                var nextParameter = parameters[tableParameterPosition + 1];

                if (nextParameter is ExpressionEqualParameter eep)
                {
                    var expression = ExpressionFactory.CreateTableExpression(eep.Expression, eep.Points);
                    CreateBehavioralCurrentSource(result, name, parameters, expression, context);
                }
                else
                {
                    result.Add(new CSharpComment("Error: TABLE expects expression parameter, " + name + " " + parameters));
                }
            }

            if (result.Count == resultIntialCount)
            {
                result.Add(new CSharpComment("Skipped, wrong parameter count, " + name + " " + parameters));
            }
        }

        public static void CreateCustomVoltageSource(List<CSharpStatement> result, string name, ParameterCollection parameters, IWriterContext context, bool isVoltageControlled)
        {
            var resultIntialCount = result.Count;

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                string expression = valueParameter.Value;

                CreateBehavioralVoltageSource(result, name, parameters, expression, context);
            }

            if (parameters.Any(p => p is WordParameter ap && ap.Value.ToLower() == "value"))
            {
                var expressionParameter = parameters.FirstOrDefault(p => p is ExpressionParameter);
                if (expressionParameter != null)
                {
                    var expression = expressionParameter.Value;

                    CreateBehavioralVoltageSource(result, name, parameters, expression, context);
                }
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Value.ToLower() == "poly"))
            {
                var dimension = 1;
                var expression = CreatePolyExpression(dimension, parameters.Skip(BehavioralCurrentSource.BehavioralCurrentSourcePinCount + 1), isVoltageControlled, context.EvaluationContext);
                CreateBehavioralVoltageSource(result, name, parameters, expression, context);
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var polyParameter = (BracketParameter)parameters.Single(p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    result.Add(new CSharpComment("Error: POLY(n) expects one argument => dimension, " + name + " " + parameters));
                }

                var dimension = (int)context.EvaluationContext.Evaluate(polyParameter.Parameters[0].Value);
                var expression = CreatePolyExpression(dimension, parameters.Skip(BehavioralCurrentSource.BehavioralCurrentSourcePinCount + 1), isVoltageControlled, context.EvaluationContext);
                CreateBehavioralVoltageSource(result, name, parameters, expression, context);
            }

            var tableParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "table");
            if (tableParameter != null)
            {
                int tableParameterPosition = parameters.IndexOf(tableParameter);
                if (tableParameterPosition == parameters.Count - 1)
                {
                    result.Add(new CSharpComment("Error: TABLE expects expression parameter, " + name + " " + parameters));
                }

                var nextParameter = parameters[tableParameterPosition + 1];

                if (nextParameter is ExpressionEqualParameter eep)
                {
                    var expression = ExpressionFactory.CreateTableExpression(eep.Expression, eep.Points);
                    CreateBehavioralVoltageSource(result, name, parameters, expression, context);
                }
                else
                {
                    result.Add(new CSharpComment("Error: TABLE expects expression parameter, " + name + " " + parameters));
                }
            }

            if (result.Count == resultIntialCount)
            {
                result.Add(new CSharpComment("Skipped, wrong parameter count, " + name + " " + parameters));
            }
        }

        public static void SetSourceParameters(BaseWriter writer, WaveformWriter waveformWriter, List<CSharpStatement> result, string sourceId, ParameterCollection parameters, IWriterContext context, bool isCurrentSource)
        {
            var acParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "ac");
            if (acParameter != null)
            {
                int acParameterIndex = parameters.IndexOf(acParameter);

                if (acParameterIndex != parameters.Count - 1)
                {
                    var acParameterValue = parameters.Get(acParameterIndex + 1);
                    result.Add(writer.SetParameter(sourceId, "acmag", acParameterValue.Value, context));

                    if (acParameterIndex + 1 != parameters.Count - 1)
                    {
                        // Check first if next parameter is waveform
                        var acPhaseCandidate = parameters[acParameterIndex + 2].Value;
                        if (parameters[acParameterIndex + 2] is SingleParameter
                            && !waveformWriter.IsWaveFormSupported(acPhaseCandidate)
                            && acPhaseCandidate.ToLower() != "dc")
                        {
                            var acPhaseParameterValue = parameters.Get(acParameterIndex + 2);
                            result.Add(writer.SetParameter(sourceId, "acphase", acPhaseParameterValue.Value, context));

                            parameters.RemoveAt(acParameterIndex + 2);
                        }
                    }

                    parameters.RemoveAt(acParameterIndex + 1);
                }

                parameters.RemoveAt(acParameterIndex);
            }

            // 2. Set DC
            var dcParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "dc");
            if (dcParameter != null)
            {
                int dcParameterIndex = parameters.IndexOf(dcParameter);
                if (dcParameterIndex != parameters.Count - 1)
                {
                    var dcParameterValue = parameters.Get(dcParameterIndex + 1);
                    result.Add(writer.SetParameter(sourceId, "dc", dcParameterValue.Value, context));

                    parameters.RemoveAt(dcParameterIndex + 1);
                }

                parameters.RemoveAt(dcParameterIndex);
            }
            else
            {
                if (parameters.Count > 0
                    && parameters[0] is SingleParameter sp
                    && !waveformWriter.IsWaveFormSupported(sp.Value)
                    && parameters[0].Value.ToLower() != "value")
                {
                    result.Add(writer.SetParameter(sourceId, "dc", sp.Value, context));

                    parameters.RemoveAt(0);
                }
            }

            // 3. Set up waveform
            if (parameters.Count > 0)
            {
                var firstParameter = parameters[0];

                if (firstParameter is BracketParameter bp)
                {
                    if (waveformWriter.IsWaveFormSupported(bp.Name))
                    {
                        var wavefromLines = waveformWriter.GenerateWaveform(bp.Name, bp.Parameters, out string waveFormId, context);
                        result.AddRange(wavefromLines);
                        result.Add(new CSharpCallStatement(sourceId, @$"SetParameter(""waveform"", {waveFormId})"));
                    }
                }
                else
                {
                    if (firstParameter is WordParameter wp && wp.Value.ToLower() != "value")
                    {
                        if (waveformWriter.IsWaveFormSupported(wp.Value))
                        {
                            var waveformLines = waveformWriter.GenerateWaveform(wp.Value, parameters.Skip(1), out string waveFormId, context);
                            result.AddRange(waveformLines);
                            result.Add(new CSharpCallStatement(sourceId, @$"SetParameter(""waveform"", {waveFormId})"));
                        }
                    }

                    if (firstParameter is AssignmentParameter assignmentParameter)
                    {
                        if (waveformWriter.IsWaveFormSupported(assignmentParameter.Name))
                        {
                            var waveformLines = waveformWriter.GenerateWaveform(assignmentParameter.Name, parameters, out var waveFormId, context);
                            result.AddRange(waveformLines);
                            result.Add(new CSharpCallStatement(sourceId, @$"SetParameter(""waveform"", {waveFormId})"));
                        }
                    }
                }

                if (firstParameter is AssignmentParameter ap && ap.Name.ToLower() == "value")
                {
                    result.Add(writer.SetParameter(sourceId, "dc", ap.Value, context));
                }

                if (parameters.Count >= 2
                    && parameters[0].Value.ToLower() == "value"
                    && parameters[1] is SingleParameter)
                {
                    result.Add(writer.SetParameter(sourceId, "dc", parameters[1].Value, context));
                }
            }

            if (isCurrentSource && parameters.Any(p => p is AssignmentParameter mParameter && mParameter.Name.ToLower() == "m"))
            {
                writer.SetParallelParameter(result, sourceId, parameters, context);
            }
        }

        public static string CreatePolyExpression(int dimension, ParameterCollection parameters, bool isVoltageControlled, IEvaluationContext context)
        {
            if (isVoltageControlled)
            {
                return ExpressionFactory.CreatePolyVoltageExpression(dimension, parameters, context);
            }

            return ExpressionFactory.CreatePolyCurrentExpression(dimension, parameters, context);
        }
    }
}
