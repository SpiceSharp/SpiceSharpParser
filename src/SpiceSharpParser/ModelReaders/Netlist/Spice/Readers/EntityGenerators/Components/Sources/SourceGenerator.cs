using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Component = SpiceSharp.Components.Component;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public abstract class SourceGenerator : ComponentGenerator
    {
        protected void SetSourceParameters(
            ParameterCollection parameters,
            IReadingContext context,
            Component component,
            bool isCurrentSource)
        {
            parameters = parameters.Skip(VoltageSource.PinCount);

            var acParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "ac");
            if (acParameter != null)
            {
                int acParameterIndex = parameters.IndexOf(acParameter);

                if (acParameterIndex != parameters.Count - 1)
                {
                    var acParameterValue = parameters.Get(acParameterIndex + 1);
                    context.SetParameter(component, "acmag", acParameterValue);

                    if (acParameterIndex + 1 != parameters.Count - 1)
                    {
                        // Check first if next parameter is waveform
                        var acPhaseCandidate = parameters[acParameterIndex + 2].Value;
                        if (parameters[acParameterIndex + 2] is SingleParameter
                            && !context.WaveformReader.Supports(acPhaseCandidate, context)
                            && acPhaseCandidate.ToLower() != "dc")
                        {
                            var acPhaseParameterValue = parameters.Get(acParameterIndex + 2);
                            context.SetParameter(component, "acphase", acPhaseParameterValue);

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
                    context.SetParameter(component, "dc", dcParameterValue);
                    parameters.RemoveAt(dcParameterIndex + 1);
                }

                parameters.RemoveAt(dcParameterIndex);
            }
            else
            {
                if (parameters.Count > 0
                    && parameters[0] is SingleParameter sp
                    && !context.WaveformReader.Supports(sp.Value, context)
                    && parameters[0].Value.ToLower() != "value")
                {
                    context.SetParameter(component, "dc", sp);
                    parameters.RemoveAt(0);
                }
            }

            // 3. Set up waveform
            if (parameters.Count > 0)
            {
                var firstParameter = parameters[0];

                if (firstParameter is BracketParameter bp)
                {
                    if (context.WaveformReader.Supports(bp.Name, context))
                    {
                        var waveform = context.WaveformReader.Generate(bp.Name, bp.Parameters, context);
                        if (waveform != null)
                        {
                            component.SetParameter("waveform", waveform);
                        }
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Unsupported waveform: {bp.Name}", bp.LineInfo);
                    }
                }
                else
                {
                    if (firstParameter is WordParameter wp && wp.Value.ToLower() != "value")
                    {
                        if (context.WaveformReader.Supports(wp.Value, context))
                        {
                            var waveform = context.WaveformReader.Generate(wp.Value, parameters.Skip(1), context);
                            if (waveform != null)
                            {
                                component.SetParameter("waveform", waveform);
                            }
                        }
                        else
                        {
                            context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Unsupported waveform: {wp}", wp.LineInfo);
                        }
                    }

                    if (firstParameter is AssignmentParameter assignmentParameter)
                    {
                        if (context.WaveformReader.Supports(assignmentParameter.Name, context))
                        {
                            var waveform = context.WaveformReader.Generate(assignmentParameter.Name, parameters, context);
                            if (waveform != null)
                            {
                                component.SetParameter("waveform", waveform);
                            }
                        }
                        else
                        {
                            context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, $"Unsupported waveform: {assignmentParameter.Name}", assignmentParameter.LineInfo);
                        }
                    }
                }

                if (firstParameter is AssignmentParameter ap && ap.Name.ToLower() == "value")
                {
                    context.SetParameter(component, "dc", ap.Value);
                }

                if (parameters.Count >= 2
                    && parameters[0].Value.ToLower() == "value"
                    && parameters[1] is SingleParameter)
                {
                    context.SetParameter(component, "dc", parameters[1].Value);
                }
            }

            if (isCurrentSource && parameters.Any(p => p is AssignmentParameter mParameter && mParameter.Name.ToLower() == "m"))
            {
                var mParameter = parameters.First(p => p is AssignmentParameter m && m.Name.ToLower() == "m");
                context.SetParameter(component, "m", mParameter);
            }
        }

        protected string CreatePolyExpression(int dimension, ParameterCollection parameters, bool isVoltageControlled, IEvaluationContext context)
        {
            if (isVoltageControlled)
            {
                return ExpressionFactory.CreatePolyVoltageExpression(dimension, parameters, context);
            }

            return ExpressionFactory.CreatePolyCurrentExpression(dimension, parameters, context);
        }

        private protected bool TryCreateLaplaceFunctionSource(
            string name,
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            LaplaceOutputKind outputKind,
            string expression,
            Parameter expressionParameter,
            out IEntity entity,
            params Parameter[] extraExcludedParameters)
        {
            entity = null;

            if (AddUnsupportedLtspiceExpressionDiagnostics(expression, expressionParameter?.LineInfo ?? parameters.LineInfo, context))
            {
                return true;
            }

            var excludedParameters = new List<Parameter>();
            if (expressionParameter != null)
            {
                excludedParameters.Add(expressionParameter);
            }

            if (extraExcludedParameters != null)
            {
                excludedParameters.AddRange(extraExcludedParameters.Where(parameter => parameter != null));
            }

            var lowerer = new LaplaceFunctionExpressionLowerer(
                context.EvaluationContext,
                context.Evaluator.EvaluateDouble,
                (message, lineInfo, exception) => context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    message,
                    lineInfo,
                    exception),
                localEntityName => context.ReaderSettings.ExpandSubcircuits
                    ? context.NameGenerator.GenerateObjectName(localEntityName)
                    : localEntityName,
                entityName => context.ContextEntities != null
                    && context.ContextEntities.Any(existing => existing.Name == entityName),
                expressionParameter?.LineInfo ?? parameters.LineInfo);

            var result = lowerer.Lower(
                name,
                originalName ?? name,
                parameters[0].Value,
                parameters[1].Value,
                expression,
                GetExtraParameters(parameters, excludedParameters),
                outputKind);

            if (!result.IsHandled)
            {
                return false;
            }

            if (result.HasErrors)
            {
                return true;
            }

            foreach (var inputHelperDefinition in result.InputHelperDefinitions)
            {
                var inputHelperEntity = CreateLaplaceInputHelper(inputHelperDefinition, context);
                context.ContextEntities?.Add(inputHelperEntity);
            }

            if (result.IsDirect)
            {
                entity = CreateLaplaceSource(result.DirectDefinition, outputKind, context);
                return true;
            }

            foreach (var helperDefinition in result.HelperDefinitions)
            {
                var helperEntity = CreateLaplaceSource(
                    helperDefinition.Definition,
                    LaplaceOutputKind.Voltage,
                    context);
                context.ContextEntities?.Add(helperEntity);
            }

            entity = outputKind == LaplaceOutputKind.Voltage
                ? CreateBehavioralVoltageSource(name, parameters, context, context.EvaluationContext, result.RewrittenExpression)
                : CreateBehavioralCurrentSource(name, parameters, context, context.EvaluationContext, result.RewrittenExpression);
            return true;
        }

        private protected BehavioralVoltageSource CreateLaplaceInputHelper(
            LaplaceFunctionInputHelperDefinition definition,
            IReadingContext context)
        {
            var helperNodes = new ParameterCollection(new List<Parameter>())
            {
                new IdentifierParameter(definition.HelperNodeName, definition.LineInfo),
                new IdentifierParameter("0", definition.LineInfo),
            };

            return CreateBehavioralVoltageSource(
                definition.SourceName,
                helperNodes,
                context,
                context.EvaluationContext,
                definition.Expression);
        }

        protected BehavioralVoltageSource CreateBehavioralVoltageSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context,
            EvaluationContext evalContext,
            string expression)
        {
            if (AddUnsupportedLtspiceExpressionDiagnostics(expression, parameters.LineInfo, context))
            {
                return null;
            }

            var entity = new BehavioralVoltageSource(name);
            context.CreateNodes(entity, parameters.Take(BehavioralVoltageSource.BehavioralVoltageSourcePinCount));
            entity.Parameters.Expression = expression;
            entity.Parameters.ParseAction = expressionToParse =>
            {
                var parser = context.CreateExpressionResolver(null);
                return parser.Resolve(expressionToParse);
            };

            if (evalContext.HaveFunctions(expression))
            {
                context.SimulationPreparations.ExecuteActionBeforeSetup(simulation =>
                {
                    entity.Parameters.Expression = expression;
                    entity.Parameters.ParseAction = expressionToParse =>
                    {
                        var parser = context.CreateExpressionResolver(simulation);
                        return parser.Resolve(expressionToParse);
                    };
                });
            }

            return entity;
        }

        protected BehavioralCurrentSource CreateBehavioralCurrentSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context,
            EvaluationContext evalContext,
            string expression)
        {
            if (AddUnsupportedLtspiceExpressionDiagnostics(expression, parameters.LineInfo, context))
            {
                return null;
            }

            var mParameter = (AssignmentParameter)parameters.FirstOrDefault(p =>
                p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "m");

            if (mParameter != null)
            {
                expression = $"({expression}) * ({mParameter.Value})";
            }

            var entity = new BehavioralCurrentSource(name);
            context.CreateNodes(entity, parameters.Take(BehavioralCurrentSource.BehavioralCurrentSourcePinCount));
            entity.Parameters.Expression = expression;
            entity.Parameters.ParseAction = expressionToParse =>
            {
                var parser = context.CreateExpressionResolver(null);
                return parser.Resolve(expressionToParse);
            };

            if (evalContext.HaveFunctions(expression))
            {
                context.SimulationPreparations.ExecuteActionBeforeSetup(simulation =>
                {
                    entity.Parameters.Expression = expression;
                    entity.Parameters.ParseAction = expressionToParse =>
                    {
                        var parser = context.CreateExpressionResolver(simulation);
                        return parser.Resolve(expressionToParse);
                    };
                });
            }

            return entity;
        }

        private protected static IEntity CreateLaplaceSource(
            LaplaceSourceDefinition definition,
            LaplaceOutputKind outputKind,
            IReadingContext context)
        {
            if (outputKind == LaplaceOutputKind.Voltage)
            {
                if (definition.Input.Kind == LaplaceSourceInputKind.Voltage)
                {
                    var entity = new LaplaceVoltageControlledVoltageSource(definition.SourceName);
                    var nodes = new ParameterCollection(new List<Parameter>())
                    {
                        new IdentifierParameter(definition.OutputPositiveNode, definition.LineInfo),
                        new IdentifierParameter(definition.OutputNegativeNode, definition.LineInfo),
                        new IdentifierParameter(definition.Input.ControlPositiveNode, definition.LineInfo),
                        new IdentifierParameter(definition.Input.ControlNegativeNode, definition.LineInfo),
                    };

                    context.CreateNodes(entity, nodes);
                    SetLaplaceParameters(entity, definition);
                    return entity;
                }

                var currentControlledEntity = new LaplaceCurrentControlledVoltageSource(definition.SourceName);
                var currentControlledNodes = new ParameterCollection(new List<Parameter>())
                {
                    new IdentifierParameter(definition.OutputPositiveNode, definition.LineInfo),
                    new IdentifierParameter(definition.OutputNegativeNode, definition.LineInfo),
                };

                context.CreateNodes(currentControlledEntity, currentControlledNodes);
                currentControlledEntity.ControllingSource = context.NameGenerator.GenerateObjectName(definition.Input.ControllingSource);
                SetLaplaceParameters(currentControlledEntity, definition);
                return currentControlledEntity;
            }

            if (definition.Input.Kind == LaplaceSourceInputKind.Voltage)
            {
                var entity = new LaplaceVoltageControlledCurrentSource(definition.SourceName);
                var nodes = new ParameterCollection(new List<Parameter>())
                {
                    new IdentifierParameter(definition.OutputPositiveNode, definition.LineInfo),
                    new IdentifierParameter(definition.OutputNegativeNode, definition.LineInfo),
                    new IdentifierParameter(definition.Input.ControlPositiveNode, definition.LineInfo),
                    new IdentifierParameter(definition.Input.ControlNegativeNode, definition.LineInfo),
                };

                context.CreateNodes(entity, nodes);
                SetLaplaceParameters(entity, definition);
                return entity;
            }

            var currentControlledCurrentEntity = new LaplaceCurrentControlledCurrentSource(definition.SourceName);
            var currentControlledCurrentNodes = new ParameterCollection(new List<Parameter>())
            {
                new IdentifierParameter(definition.OutputPositiveNode, definition.LineInfo),
                new IdentifierParameter(definition.OutputNegativeNode, definition.LineInfo),
            };

            context.CreateNodes(currentControlledCurrentEntity, currentControlledCurrentNodes);
            currentControlledCurrentEntity.ControllingSource = context.NameGenerator.GenerateObjectName(definition.Input.ControllingSource);
            SetLaplaceParameters(currentControlledCurrentEntity, definition);
            return currentControlledCurrentEntity;
        }

        private static IEnumerable<Parameter> GetExtraParameters(
            ParameterCollection parameters,
            IReadOnlyList<Parameter> excludedParameters)
        {
            return parameters.Where(parameter =>
                !excludedParameters.Any(excluded => ReferenceEquals(excluded, parameter)));
        }

        protected bool TryCreateLtspiceTableSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context,
            bool isCurrentSource,
            out IEntity entity)
        {
            entity = null;

            if (!context.ReaderSettings.Compatibility.IsLTspice)
            {
                return false;
            }

            var tableParameter = parameters
                .OfType<AssignmentParameter>()
                .FirstOrDefault(parameter => string.Equals(parameter.Name, "tbl", StringComparison.OrdinalIgnoreCase));

            if (tableParameter == null)
            {
                return false;
            }

            var parts = SplitTopLevelList(tableParameter.Value).ToList();
            if (parts.Count < 3 || (parts.Count - 1) % 2 != 0)
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Unsupported LTspice source option 'tbl': expected tbl=(expr, x1, y1, ...).",
                    tableParameter.LineInfo);
                return true;
            }

            var expression = NormalizeExpression(parts[0]);
            if (AddUnsupportedLtspiceExpressionDiagnostics(expression, tableParameter.LineInfo, context))
            {
                return true;
            }

            var points = new List<PointParameter>();
            for (var i = 1; i < parts.Count; i += 2)
            {
                var values = new PointValues(
                    new List<SingleParameter>
                    {
                        new ValueParameter(parts[i].Trim(), tableParameter.LineInfo),
                        new ValueParameter(parts[i + 1].Trim(), tableParameter.LineInfo),
                    },
                    tableParameter.LineInfo);
                points.Add(new PointParameter(values, tableParameter.LineInfo));
            }

            var tableExpression = ExpressionFactory.CreateTableExpression(expression, points);
            entity = isCurrentSource
                ? CreateBehavioralCurrentSource(name, parameters, context, context.EvaluationContext, tableExpression)
                : CreateBehavioralVoltageSource(name, parameters, context, context.EvaluationContext, tableExpression);
            return true;
        }

        private protected ParameterCollection PrepareLtspiceSourceParameters(
            string sourceName,
            string originalName,
            ParameterCollection parameters,
            IReadingContext context,
            bool isCurrentSource,
            out LtspiceSourceTopologyOptions topologyOptions)
        {
            var sourceParameters = new ParameterCollection(parameters.ToList());
            topologyOptions = ExtractLtspiceSourceTopologyOptions(sourceParameters, context, isCurrentSource);

            if (topologyOptions.HasSeriesResistance && sourceParameters.Count >= VoltageSource.PinCount)
            {
                topologyOptions.SeriesNodeName = CreateInternalSourceNodeName(sourceName, originalName);
                sourceParameters.RemoveAt(0);
                sourceParameters.Insert(0, new IdentifierParameter(topologyOptions.SeriesNodeName, parameters[0].LineInfo));
            }

            return sourceParameters;
        }

        private protected IEntity ApplyLtspiceSourceTopology(
            string sourceName,
            ParameterCollection externalParameters,
            IReadingContext context,
            LtspiceSourceTopologyOptions topologyOptions,
            IEntity entity)
        {
            if (entity == null || !topologyOptions.HasAny || externalParameters.Count < VoltageSource.PinCount)
            {
                return entity;
            }

            if (topologyOptions.HasSeriesResistance)
            {
                var resistor = new Resistor(sourceName + "_rser");
                context.CreateNodes(
                    resistor,
                    CreateNodeParameters(
                        externalParameters[0],
                        new IdentifierParameter(topologyOptions.SeriesNodeName, externalParameters[0].LineInfo)));
                context.SetParameter(resistor, "resistance", GetLtspiceSourceOptionValue(topologyOptions.SeriesResistance), true);
                context.ContextEntities?.Add(resistor);
            }

            if (topologyOptions.HasShuntResistance)
            {
                var resistor = new Resistor(sourceName + "_load");
                context.CreateNodes(resistor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(resistor, "resistance", GetLtspiceSourceOptionValue(topologyOptions.ShuntResistance), true);
                context.ContextEntities?.Add(resistor);
            }

            if (topologyOptions.HasParallelCapacitance)
            {
                var capacitor = new Capacitor(sourceName + "_cpar");
                context.CreateNodes(capacitor, CreateNodeParameters(externalParameters[0], externalParameters[1]));
                context.SetParameter(capacitor, "capacitance", GetLtspiceSourceOptionValue(topologyOptions.ParallelCapacitance), true);
                context.ContextEntities?.Add(capacitor);
            }

            return entity;
        }

        private static LtspiceSourceTopologyOptions ExtractLtspiceSourceTopologyOptions(
            ParameterCollection parameters,
            IReadingContext context,
            bool isCurrentSource)
        {
            var result = new LtspiceSourceTopologyOptions();

            if (!context.ReaderSettings.Compatibility.IsLTspice || parameters.Count < VoltageSource.PinCount)
            {
                return result;
            }

            for (var i = parameters.Count - 1; i >= VoltageSource.PinCount; i--)
            {
                if (parameters[i] is AssignmentParameter assignment
                    && TrySetLtspiceSourceTopologyOption(result, assignment.Name, assignment, isCurrentSource))
                {
                    parameters.RemoveAt(i);
                    continue;
                }

                if (parameters[i] is WordParameter word
                    && IsLtspiceSourceTopologyOption(word.Value))
                {
                    if (i + 1 < parameters.Count)
                    {
                        TrySetLtspiceSourceTopologyOption(result, word.Value, parameters[i + 1], isCurrentSource);
                        parameters.RemoveAt(i + 1);
                        parameters.RemoveAt(i);
                    }
                    else
                    {
                        context.Result.ValidationResult.AddError(
                            ValidationEntrySource.Reader,
                            $"Invalid LTspice source option '{word.Value}': expected '{word.Value}=<value>' or '{word.Value} <value>'.",
                            word.LineInfo);
                        parameters.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        private static bool TrySetLtspiceSourceTopologyOption(
            LtspiceSourceTopologyOptions result,
            string optionName,
            Parameter optionValue,
            bool isCurrentSource)
        {
            if (string.Equals(optionName, "cpar", StringComparison.OrdinalIgnoreCase))
            {
                result.ParallelCapacitance ??= optionValue;
                return true;
            }

            if (string.Equals(optionName, "rser", StringComparison.OrdinalIgnoreCase))
            {
                result.SeriesResistance ??= optionValue;
                return true;
            }

            if (string.Equals(optionName, "load", StringComparison.OrdinalIgnoreCase))
            {
                result.ShuntResistance ??= optionValue;
                return true;
            }

            if (string.Equals(optionName, "r", StringComparison.OrdinalIgnoreCase))
            {
                if (isCurrentSource)
                {
                    result.ShuntResistance ??= optionValue;
                }
                else
                {
                    result.SeriesResistance ??= optionValue;
                }

                return true;
            }

            return false;
        }

        private static bool IsLtspiceSourceTopologyOption(string optionName)
        {
            return string.Equals(optionName, "rser", StringComparison.OrdinalIgnoreCase)
                || string.Equals(optionName, "cpar", StringComparison.OrdinalIgnoreCase)
                || string.Equals(optionName, "load", StringComparison.OrdinalIgnoreCase)
                || string.Equals(optionName, "r", StringComparison.OrdinalIgnoreCase);
        }

        private static ParameterCollection CreateNodeParameters(Parameter firstNode, Parameter secondNode)
        {
            return new ParameterCollection(new List<Parameter>())
            {
                firstNode,
                secondNode,
            };
        }

        private static string CreateInternalSourceNodeName(string sourceName, string originalName)
        {
            return "__ltspice_" + (originalName ?? sourceName) + "_rser";
        }

        private static string GetLtspiceSourceOptionValue(Parameter parameter)
        {
            return parameter is AssignmentParameter assignment ? assignment.Value : parameter.Value;
        }

        private protected sealed class LtspiceSourceTopologyOptions
        {
            public Parameter SeriesResistance { get; set; }

            public Parameter ShuntResistance { get; set; }

            public Parameter ParallelCapacitance { get; set; }

            public string SeriesNodeName { get; set; }

            public bool HasSeriesResistance => SeriesResistance != null;

            public bool HasShuntResistance => ShuntResistance != null;

            public bool HasParallelCapacitance => ParallelCapacitance != null;

            public bool HasAny => HasSeriesResistance || HasShuntResistance || HasParallelCapacitance;
        }

        private static bool AddUnsupportedLtspiceExpressionDiagnostics(string expression, SpiceLineInfo lineInfo, IReadingContext context)
        {
            if (!context.ReaderSettings.Compatibility.IsLTspice || string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            var hasErrors = false;

            if (expression.Contains("~"))
            {
                context.Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    "Unsupported LTspice expression operator '~': unary bitwise/boolean inversion is not mapped yet.",
                    lineInfo);
                hasErrors = true;
            }

            return hasErrors;
        }

        private static IEnumerable<string> SplitTopLevelList(string value)
        {
            var start = 0;
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                }
                else if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                }
                else if (!inSingleQuote && !inDoubleQuote)
                {
                    if (c == '(' || c == '{' || c == '[')
                    {
                        depth++;
                    }
                    else if (c == ')' || c == '}' || c == ']')
                    {
                        depth--;
                    }
                    else if (c == ',' && depth == 0)
                    {
                        yield return value.Substring(start, i - start).Trim();
                        start = i + 1;
                    }
                }
            }

            yield return value.Substring(start).Trim();
        }

        private static string NormalizeExpression(string expression)
        {
            expression = expression.Trim();
            if (expression.Length >= 2
                && ((expression[0] == '{' && expression[expression.Length - 1] == '}')
                    || (expression[0] == '\'' && expression[expression.Length - 1] == '\'')))
            {
                return expression.Substring(1, expression.Length - 2);
            }

            return expression;
        }

        private static void SetLaplaceParameters(
            LaplaceVoltageControlledVoltageSource entity,
            LaplaceSourceDefinition definition)
        {
            entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
            entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
            entity.Parameters.Delay = definition.Delay;
        }

        private static void SetLaplaceParameters(
            LaplaceCurrentControlledVoltageSource entity,
            LaplaceSourceDefinition definition)
        {
            entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
            entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
            entity.Parameters.Delay = definition.Delay;
        }

        private static void SetLaplaceParameters(
            LaplaceVoltageControlledCurrentSource entity,
            LaplaceSourceDefinition definition)
        {
            entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
            entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
            entity.Parameters.Delay = definition.Delay;
        }

        private static void SetLaplaceParameters(
            LaplaceCurrentControlledCurrentSource entity,
            LaplaceSourceDefinition definition)
        {
            entity.Parameters.Numerator = definition.TransferFunction.NumeratorCoefficients;
            entity.Parameters.Denominator = definition.TransferFunction.DenominatorCoefficients;
            entity.Parameters.Delay = definition.Delay;
        }
    }
}
