using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
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
                        component.SetParameter("waveform", context.WaveformReader.Generate(bp.Name, bp.Parameters, context));
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
                            component.SetParameter("waveform", context.WaveformReader.Generate(wp.Value, parameters.Skip(1), context));
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
                            component.SetParameter("waveform", context.WaveformReader.Generate(assignmentParameter.Name, parameters, context));
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

        protected BehavioralVoltageSource CreateBehavioralVoltageSource(
            string name,
            ParameterCollection parameters,
            IReadingContext context,
            EvaluationContext evalContext,
            string expression)
        {
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
