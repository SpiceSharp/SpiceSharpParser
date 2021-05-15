using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Current sources generator.
    /// </summary>
    public class CurrentSourceGenerator : SourceGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            switch (type.ToLower())
            {
                case "i": return GenerateCurrentSource(componentIdentifier, parameters, context);
                case "g": return GenerateVoltageControlledCurrentSource(componentIdentifier, parameters, context);
                case "f": return GenerateCurrentControlledCurrentSource(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates a new current controlled current source: FName.
        /// </summary>
        /// <param name="name">Name of generated current controlled current source.</param>
        /// <param name="parameters">Parameters for current source.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current controlled current source.
        /// </returns>
        protected IEntity GenerateCurrentControlledCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 4
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2) && parameters[2].Value.ToLower() != "value"
                && parameters.IsValueString(3))
            {
                var cccs = new CurrentControlledCurrentSource(name);
                context.CreateNodes(cccs, parameters);
                cccs.ControllingSource = context.NameGenerator.GenerateObjectName(parameters.Get(2).Value);

                var mParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "m");
                if (mParameter == null)
                {
                    context.SetParameter(cccs, "gain", parameters.Get(3));
                }
                else
                {
                    context.SetParameter(cccs, "gain", $"({mParameter.Value}) * ({parameters.Get(3)})");
                }
                return cccs;
            }
            else
            {
                return CreateCustomCurrentSource(name, parameters, context, false);
            }
        }

        /// <summary>
        /// Generates a new voltage controlled current source: GName.
        /// </summary>
        /// <param name="name">Name of generated voltage controlled current source.</param>
        /// <param name="parameters">Parameters for current source.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of voltage controlled current source.
        /// </returns>
        protected IEntity GenerateVoltageControlledCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count == 5
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2)
                && parameters.IsValueString(3)
                && parameters.IsValueString(4))
            {
                var vccs = new VoltageControlledCurrentSource(name);
                context.CreateNodes(vccs, parameters);
                var mParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "m");
                if (mParameter == null)
                {
                    context.SetParameter(vccs, "gain", parameters.Get(4));
                }
                else
                {
                    context.SetParameter(vccs, "gain", $"({mParameter.Value}) * ({parameters.Get(4)})");
                }

                return vccs;
            }
            else
            {
                if (parameters.Count == 3
                    && parameters[0] is PointParameter pp1 && pp1.Values.Count() == 2
                    && parameters[1] is PointParameter pp2 && pp2.Values.Count() == 2)
                {
                    var vccsNodes = new ParameterCollection(new List<Parameter>());
                    vccsNodes.Add(pp1.Values.Items[0]);
                    vccsNodes.Add(pp1.Values.Items[1]);
                    vccsNodes.Add(pp2.Values.Items[0]);
                    vccsNodes.Add(pp2.Values.Items[1]);

                    var vccs = new VoltageControlledCurrentSource(name);
                    context.CreateNodes(vccs, vccsNodes);
                    var mParameter = (AssignmentParameter)parameters.FirstOrDefault(p => p is AssignmentParameter asgParameter && asgParameter.Name.ToLower() == "m");
                    if (mParameter == null)
                    {
                        context.SetParameter(vccs, "gain", parameters.Get(2));
                    }
                    else
                    {
                        context.SetParameter(vccs, "gain", $"({mParameter.Value}) * ({parameters.Get(2)})");
                    }
                    return vccs;
                }
                else
                {
                    return CreateCustomCurrentSource(name, parameters, context, true);
                }
            }
        }

        /// <summary>
        /// Generates a new current source.
        /// </summary>
        /// <param name="name">Name of generated current source.</param>
        /// <param name="parameters">Parameters for current source.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of current source.
        /// </returns>
        protected IEntity GenerateCurrentSource(string name, ParameterCollection parameters, IReadingContext context)
        {
            var evalContext = context.Evaluator.GetEvaluationContext();

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                string expression = valueParameter.Value;
                if (evalContext.HaveSpiceProperties(expression) || evalContext.HaveFunctions(expression))
                {
                    BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                    return entity;
                }
            }

            if (parameters.Any(p => p is ExpressionParameter ep))
            {
                var expressionParameter = (ExpressionParameter)parameters.Single(p => p is ExpressionParameter);
                string expression = expressionParameter.Value;

                if (evalContext.HaveSpiceProperties(expressionParameter.Value))
                {
                    BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                    return entity;
                }
            }

            var cs = new CurrentSource(name);
            context.CreateNodes(cs, parameters);
            SetSourceParameters(parameters, context, cs, true);
            return cs;
        }

        protected static BehavioralCurrentSource CreateBehavioralCurrentSource(string name, ParameterCollection parameters, IReadingContext context, EvaluationContext evalContext, string expression)
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
            entity.Parameters.ParseAction = (expression) =>
            {
                var parser = context.CreateExpressionParser(null);
                return parser.Resolve(expression);
            };

            if (evalContext.HaveFunctions(expression))
            {
                context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                {
                    entity.Parameters.Expression = expression.ToString();
                    entity.Parameters.ParseAction = (expression) =>
                    {
                        var parser = context.CreateExpressionParser(simulation);
                        return parser.Resolve(expression);
                    };
                });
            }

            return entity;
        }

        private IEntity CreateCustomCurrentSource(string name, ParameterCollection parameters, IReadingContext context, bool isVoltageControlled)
        {
            var evalContext = context.Evaluator.GetEvaluationContext();

            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                string expression = valueParameter.Value;

                BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                return entity;
            }

            if (parameters.Any(p => p is WordParameter ap && ap.Value.ToLower() == "value"))
            {
                var expressionParameter = parameters.FirstOrDefault(p => p is ExpressionParameter);
                if (expressionParameter != null)
                {
                    var expression = expressionParameter.Value;

                    BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                    return entity;
                }
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Value.ToLower() == "poly"))
            {
                var dimension = 1;
                var expression = CreatePolyExpression(dimension, parameters.Skip(CurrentSource.PinCount + 1), isVoltageControlled, context.Evaluator.GetEvaluationContext());
                BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                return entity;
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var polyParameter = (BracketParameter)parameters.Single(p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "poly expects one argument => dimension", polyParameter.LineInfo));
                    return null;
                }

                var dimension = (int)context.Evaluator.EvaluateDouble(polyParameter.Parameters[0].Value);
                var expression = CreatePolyExpression(dimension, parameters.Skip(CurrentSource.PinCount + 1), isVoltageControlled, context.Evaluator.GetEvaluationContext());
                BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                return entity;
            }

            var tableParameter = parameters.FirstOrDefault(p => p.Value.ToLower() == "table");
            if (tableParameter != null)
            {
                int tableParameterPosition = parameters.IndexOf(tableParameter);
                if (tableParameterPosition == parameters.Count - 1)
                {
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "table expects expression parameter", tableParameter.LineInfo));
                    return null;
                }

                var nextParameter = parameters[tableParameterPosition + 1];

                if (nextParameter is ExpressionEqualParameter eep)
                {
                    var expression = ExpressionFactory.CreateTableExpression(eep.Expression, eep.Points);

                    BehavioralCurrentSource entity = CreateBehavioralCurrentSource(name, parameters, context, evalContext, expression);
                    return entity;
                }
                else
                {
                    context.Result.ValidationResult.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "table expects equal parameter", tableParameter.LineInfo));
                    return null;
                }
            }

            return null;
        }
    }
}