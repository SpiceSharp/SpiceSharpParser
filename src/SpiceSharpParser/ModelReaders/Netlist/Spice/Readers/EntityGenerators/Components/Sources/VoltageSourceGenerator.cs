using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using Component = SpiceSharp.Components.Component;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Entities;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Voltage sources generator.
    /// </summary>
    public class VoltageSourceGenerator : SourceGenerator
    {
        public override IEntity Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            switch (type.ToLower())
            {
                case "v": return GenerateVoltageSource(componentIdentifier, parameters, context);
                case "h": return GenerateCurrentControlledVoltageSource(componentIdentifier, parameters, context);
                case "e": return GenerateVoltageControlledVoltageSource(componentIdentifier, parameters, context);
            }

            return null;
        }

        /// <summary>
        /// Generates new voltage controlled voltage source: EName.
        /// </summary>
        /// <param name="name">The name of voltage source to generate.</param>
        /// <param name="parameters">The parameters for voltage source.</param>
        /// <param name="context">The reading context.</param>
        /// <returns>
        /// A new instance of voltage controlled voltage source.
        /// </returns>
        protected IEntity GenerateVoltageControlledVoltageSource(
            string name,
            ParameterCollection parameters,
            ICircuitContext context)
        {
            if (parameters.Count == 5
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2)
                && parameters.IsValueString(3)
                && parameters.IsValueString(4))
            {
                var vcvs = new VoltageControlledVoltageSource(name);
                context.CreateNodes(vcvs, parameters);
                context.SetParameter(vcvs, "gain", parameters.Get(4));

                return vcvs;
            }
            else
            {
                if (parameters.Count == 3
                    && parameters[0] is PointParameter pp1
                    && pp1.Values.Count() == 2
                    && parameters[1] is PointParameter pp2
                    && pp2.Values.Count() == 2)
                {
                    var vcvsNodes = new ParameterCollection(new List<Parameter>())
                    {
                        pp1.Values.Items[0],
                        pp1.Values.Items[1],
                        pp2.Values.Items[0],
                        pp2.Values.Items[1],
                    };

                    var vcvs = new VoltageControlledVoltageSource(name);
                    context.CreateNodes(vcvs, vcvsNodes);
                    context.SetParameter(vcvs, "gain", parameters.Get(2));
                    return vcvs;
                }
                else
                {
                    return CreateCustomVoltageSource(name, parameters, context, true);
                }
            }
        }

        /// <summary>
        /// Generates new current controlled voltage source HName.
        /// </summary>
        /// <param name="name">The name of voltage source to generate.</param>
        /// <param name="parameters">The parameters for voltage source.</param>
        /// <param name="context">The reading context.</param>
        /// <returns>
        /// A new instance of current controlled voltage source.
        /// </returns>
        protected IEntity GenerateCurrentControlledVoltageSource(
            string name,
            ParameterCollection parameters,
            ICircuitContext context)
        {
            if (parameters.Count == 4
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2) && parameters[2].Image.ToLower() != "value"
                && parameters.IsValueString(3))
            {
                var ccvs = new CurrentControlledVoltageSource(name);
                context.CreateNodes(ccvs, parameters);
                ccvs.ControllingSource = context.NameGenerator.GenerateObjectName(parameters.Get(2).Image);
                context.SetParameter(ccvs, "gain", parameters.Get(3).Image);
                return ccvs;
            }
            else
            {
                return CreateCustomVoltageSource(name, parameters, context, false);
            }
        }

        /// <summary>
        /// Generates new voltage source.
        /// </summary>
        /// <param name="name">The name of voltage source to generate.</param>
        /// <param name="parameters">The parameters for voltage source.</param>
        /// <param name="context">The reading context.</param>
        /// <returns>
        /// A new instance of voltage source.
        /// </returns>
        protected IEntity GenerateVoltageSource(string name, ParameterCollection parameters, ICircuitContext context)
        {
            var vs = new VoltageSource(name);
            context.CreateNodes(vs, parameters);
            SetSourceParameters(name, parameters, context, vs);
            return vs;
        }

        protected IEntity CreateCustomVoltageSource(
            string name,
            ParameterCollection parameters,
            ICircuitContext context,
            bool isVoltageControlled)
        {
            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var entity = new BehavioralVoltageSource(name);
                context.CreateNodes(entity, parameters);
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");
                entity.Parameters.Expression = valueParameter.Value;
                return entity;
            }

            if (parameters.Any(p => p is WordParameter ap && ap.Image.ToLower() == "value"))
            {
                var entity = new BehavioralVoltageSource(name);
                context.CreateNodes(entity, parameters);

                var expressionParameter = parameters.FirstOrDefault(p => p is ExpressionParameter);
                if (expressionParameter != null)
                {
                    entity.Parameters.Expression = expressionParameter.Image;
                }

                return entity;
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Image.ToLower() == "poly"))
            {
                var entity = new VoltageSource(name);
                context.CreateNodes(entity, parameters);
                parameters = parameters.Skip(VoltageSource.PinCount);
                var dimension = 1;
                var expression = CreatePolyExpression(dimension, parameters.Skip(1), isVoltageControlled, context.Evaluator.GetEvaluationContext());
                context.SetParameter(entity, "dc", expression);

                return entity;
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var polyParameter = (BracketParameter)parameters.Single(p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "poly expects one argument => dimension", polyParameter.LineInfo));
                }

                var entity = new BehavioralVoltageSource(name);
                context.CreateNodes(entity, parameters);

                parameters = parameters.Skip(VoltageSource.PinCount);
                var dimension = (int)context.Evaluator.EvaluateDouble(polyParameter.Parameters[0].Image);
                var expression = CreatePolyExpression(dimension, parameters.Skip(1), isVoltageControlled, context.Evaluator.GetEvaluationContext());
                entity.Parameters.Expression = expression;
               
                return entity;
            }

            var tableParameter = parameters.FirstOrDefault(p => p.Image.ToLower() == "table");
            if (tableParameter != null)
            {
                int tableParameterPosition = parameters.IndexOf(tableParameter);
                if (tableParameterPosition == parameters.Count - 1)
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "table expects expression parameter", tableParameter.LineInfo));
                }

                var nextParameter = parameters[tableParameterPosition + 1];

                if (nextParameter is ExpressionEqualParameter eep)
                {
                    var entity = new BehavioralVoltageSource(name);
                    context.CreateNodes(entity, parameters);
                    entity.Parameters.Expression =  ExpressionFactory.CreateTableExpression(eep.Expression, eep.Points);
                    return entity;
                }
                else
                {
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, "table expects expression equal parameter", parameters.LineInfo));
                }
            }

            return null;
        }
    }
}