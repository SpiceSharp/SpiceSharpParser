using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Component = SpiceSharp.Components.Component;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    /// <summary>
    /// Current sources generator.
    /// </summary>
    public class CurrentSourceGenerator : SourceGenerator
    {
        public override Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
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
        protected Component GenerateCurrentControlledCurrentSource(string name,  ParameterCollection parameters, ICircuitContext context)
        {
            if (parameters.Count == 4
                && parameters.IsValueString(0)
                && parameters.IsValueString(1)
                && parameters.IsValueString(2) && parameters[2].Image.ToLower() != "value"
                && parameters.IsValueString(3))
            {
                var cccs = new CurrentControlledCurrentSource(name);
                context.CreateNodes(cccs, parameters);
                cccs.ControllingName = context.NameGenerator.GenerateObjectName(parameters.Get(2).Image);
                context.SetParameter(cccs, "gain", parameters.Get(3));
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
        protected Component GenerateVoltageControlledCurrentSource(string name, ParameterCollection parameters, ICircuitContext context)
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
                context.SetParameter(vccs, "gain", parameters.Get(4));
                return vccs;
            }
            else
            {
                if (parameters.Count == 3
                    && parameters[0] is PointParameter pp1 && pp1.Values.Count() == 2
                    && parameters[1] is PointParameter pp2 && pp2.Values.Count() == 2)
                {
                    var vccsNodes = new ParameterCollection();
                    vccsNodes.Add(pp1.Values.Items[0]);
                    vccsNodes.Add(pp1.Values.Items[1]);
                    vccsNodes.Add(pp2.Values.Items[0]);
                    vccsNodes.Add(pp2.Values.Items[1]);

                    var vccs = new VoltageControlledCurrentSource(name);
                    context.CreateNodes(vccs, vccsNodes);
                    context.SetParameter(vccs, "gain", parameters.Get(2));
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
        protected Component GenerateCurrentSource(string name,  ParameterCollection parameters, ICircuitContext context)
        {
            CurrentSource cs = new CurrentSource(name);
            context.CreateNodes(cs, parameters);
            SetSourceParameters(name, parameters, context, cs);
            return cs;
        }

        private Component CreateCustomCurrentSource(string name, ParameterCollection parameters, ICircuitContext context, bool isVoltageControlled)
        {
            if (parameters.Any(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value"))
            {
                var valueParameter = (AssignmentParameter)parameters.Single(p => p is AssignmentParameter ap && ap.Name.ToLower() == "value");

                var entity = new BehavioralCurrentSource(name);
                context.CreateNodes(entity, parameters);
                
                var baseParameters = entity.ParameterSets.Get<SpiceSharpBehavioral.Components.BehavioralBehaviors.BaseParameters>();
                baseParameters.Expression = valueParameter.Value;
                baseParameters.SpicePropertyComparer = StringComparerProvider.Get(context.CaseSensitivity.IsFunctionNameCaseSensitive);
                baseParameters.Parser = (sim) => CreateParser(context, sim);
                return entity;
            }

            if (parameters.Any(p => p is WordParameter ap && ap.Image.ToLower() == "value"))
            {
                var expressionParameter = parameters.FirstOrDefault(p => p is ExpressionParameter);
                if (expressionParameter != null)
                {
                    var entity = new BehavioralCurrentSource(name);
                    context.CreateNodes(entity, parameters);

                    var baseParameters = entity.ParameterSets.Get<SpiceSharpBehavioral.Components.BehavioralBehaviors.BaseParameters>();
                    baseParameters.Expression = expressionParameter.Image;
                    baseParameters.SpicePropertyComparer = StringComparerProvider.Get(context.CaseSensitivity.IsFunctionNameCaseSensitive);
                    baseParameters.Parser = (sim) => CreateParser(context, sim);
                    return entity;
                }
            }

            if (parameters.Any(p => p is WordParameter bp && bp.Image.ToLower() == "poly"))
            {
                var entity = new CurrentSource(name);
                context.CreateNodes(entity, parameters);
                parameters = parameters.Skip(CurrentSource.CurrentSourcePinCount);
                var dimension = 1;
                var expression = CreatePolyExpression(dimension, parameters.Skip(1), isVoltageControlled);
                context.SetParameter(entity, "dc", expression);
                return entity;
            }

            if (parameters.Any(p => p is BracketParameter bp && bp.Name.ToLower() == "poly"))
            {
                var entity = new CurrentSource(name);
                context.CreateNodes(entity, parameters);
                parameters = parameters.Skip(CurrentSource.CurrentSourcePinCount);

                var polyParameter = (BracketParameter)parameters.Single(p => p is BracketParameter bp && bp.Name.ToLower() == "poly");

                if (polyParameter.Parameters.Count != 1)
                {
                    throw new WrongParametersCountException(name, "poly expects one argument => dimension");
                }

                var dimension = (int)context.CircuitEvaluator.EvaluateDouble(polyParameter.Parameters[0].Image);
                var expression = CreatePolyExpression(dimension, parameters.Skip(1), isVoltageControlled);

                context.SetParameter(entity, "dc", expression);
                return entity;
            }

            var tableParameter = parameters.FirstOrDefault(p => p.Image.ToLower() == "table");
            if (tableParameter != null)
            {
                int tableParameterPosition = parameters.IndexOf(tableParameter);
                if (tableParameterPosition == parameters.Count - 1)
                {
                    throw new WrongParametersCountException(name, "table expects expression parameter");
                }

                var nextParameter = parameters[tableParameterPosition + 1];

                if (nextParameter is ExpressionEqualParameter eep)
                {
                    var entity = new BehavioralCurrentSource(name);
                    context.CreateNodes(entity, parameters);
                    
                    var baseParameters = entity.ParameterSets.Get<SpiceSharpBehavioral.Components.BehavioralBehaviors.BaseParameters>();
                    baseParameters.Expression = ExpressionFactory.CreateTableExpression(eep.Expression, eep.Points);
                    baseParameters.SpicePropertyComparer = StringComparerProvider.Get(context.CaseSensitivity.IsFunctionNameCaseSensitive);
                    baseParameters.Parser = (sim) => CreateParser(context, sim);
                    return entity;
                }
                else
                {
                    throw new WrongParameterTypeException(name, "table expects expression equal parameter");
                }
            }

            return null;
        }
    }
}
