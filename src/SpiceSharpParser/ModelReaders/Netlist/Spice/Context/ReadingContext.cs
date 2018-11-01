using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    using SpiceSharp.Simulations;

    using SpiceSharpParser.Common.Evaluation;

    using Expression = System.Linq.Expressions.Expression;

    /// <summary>
    /// Reading context.
    /// </summary>
    public class ReadingContext : IReadingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadingContext"/> class.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="evaluators">Evaluator for the context.</param>
        /// <param name="resultService">SpiceSharpModel service for the context.</param>
        /// <param name="nodeNameGenerator">Name generator for the nodes.</param>
        /// <param name="componentNameGenerator">Name generator for the components.</param>
        /// <param name="modelNameGenerator">Name generator for the models.</param>
        /// <param name="statementsReader">Statements reader.</param>
        /// <param name="waveformReader">Waveform reader.</param>
        /// <param name="readingEvaluator">Reading evaluator.</param>
        /// <param name="readingExpressionContext">Reading expression context.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <param name="parent">Parent of th context.</param>
        public ReadingContext(
            string contextName,
            IExpressionParser parser,
            ISimulationsParameters parameters,
            ISimulationEvaluators evaluators,
            SimulationExpressionContexts contexts,
            IResultService resultService,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            ISpiceStatementsReader statementsReader,
            IWaveformReader waveformReader,
            IEvaluator readingEvaluator,
            ExpressionContext readingExpressionContext,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            IReadingContext parent = null)
        {
            Name = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            NodeNameGenerator = nodeNameGenerator ?? throw new ArgumentNullException(nameof(nodeNameGenerator));
            ComponentNameGenerator = componentNameGenerator ?? throw new ArgumentNullException(nameof(componentNameGenerator));
            ModelNameGenerator = modelNameGenerator ?? throw new ArgumentNullException(nameof(componentNameGenerator));
            Parent = parent;
            SimulationExpressionContexts = contexts;
            ExpressionParser = parser;
            ReadingExpressionContext = readingExpressionContext;

            if (Parent != null)
            {
                AvailableSubcircuits = new List<SubCircuit>(Parent.AvailableSubcircuits);
            }
            else
            {
                AvailableSubcircuits = new List<SubCircuit>();
            }

            Children = new List<IReadingContext>();

            SimulationsParameters = parameters;
            SimulutionEvaluators = evaluators;

            var generators = new List<IObjectNameGenerator>();
            IReadingContext current = this;
            while (current != null)
            {
                generators.Add(current.ModelNameGenerator);
                current = current.Parent;
            }

            StatementsReader = statementsReader;
            WaveformReader = waveformReader;
            CaseSensitivity = caseSettings;
            ReadingEvaluator = readingEvaluator;
            ModelsRegistry = new StochasticModelsRegistry(generators, caseSettings.IsEntityNameCaseSensitive);
        }

        /// <summary>
        /// Gets or sets the name of context.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the parent of context.
        /// </summary>
        public IReadingContext Parent { get; protected set; }

        /// <summary>
        /// Gets the simulationEvaluators for the context.
        /// </summary>
        public ISimulationEvaluators SimulutionEvaluators { get; }

        public IEvaluator ReadingEvaluator { get;  }

        public IExpressionParser ExpressionParser { get; }

        public SimulationExpressionContexts SimulationExpressionContexts { get; }

        public ExpressionContext ReadingExpressionContext { get; set; }

        /// <summary>
        /// Gets simulation parameters.
        /// </summary>
        public ISimulationsParameters SimulationsParameters { get; }

        /// <summary>
        /// Gets available subcircuits in context.
        /// </summary>
        public ICollection<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets or sets the result service.
        /// </summary>
        public IResultService Result { get; protected set; }

        /// <summary>
        /// Gets or sets the node name generator.
        /// </summary>
        public INodeNameGenerator NodeNameGenerator { get; protected set; }

        /// <summary>
        /// Gets or sets the component name generator.
        /// </summary>
        public IObjectNameGenerator ComponentNameGenerator { get; protected set; }

        /// <summary>
        /// Gets or sets the model name generator.
        /// </summary>
        public IObjectNameGenerator ModelNameGenerator { get; protected set; }

        /// <summary>
        /// Gets or sets the children of the reading context.
        /// </summary>
        public ICollection<IReadingContext> Children { get; protected set; }

        /// <summary>
        /// Gets or sets the stochastic models registry.
        /// </summary>
        public IModelsRegistry ModelsRegistry { get; protected set; }

        /// <summary>
        /// Gets or sets statements reader.
        /// </summary>
        public ISpiceStatementsReader StatementsReader { get; set; }

        /// <summary>
        /// Gets or sets waveform reader.
        /// </summary>
        public IWaveformReader WaveformReader { get; set; }

        public SpiceNetlistCaseSensitivitySettings CaseSensitivity { get; set; }

        /// <summary>
        /// Sets voltage initial condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression.</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            var nodeId = NodeNameGenerator.Generate(nodeName);
            SimulationsParameters.SetICVoltage(this.SimulationExpressionContexts, nodeId, expression);
        }

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>
        /// A value of expression..
        /// </returns>
        public double EvaluateDouble(string expression)
        {
            try
            {
                return ReadingEvaluator.EvaluateValueExpression(expression, this.ReadingExpressionContext);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of expression: " + expression, ex);
            }
        }

        public void SetParameter(string pName, double value)
        {
            this.ReadingExpressionContext.SetParameter(pName, value);
        }

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component.</param>
        /// <param name="parameters">Parameters of component.</param>
        public void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters)
        {
            string[] nodes = new string[component.PinCount];
            for (var i = 0; i < component.PinCount; i++)
            {
                string pinName = parameters.GetString(i);
                nodes[i] = NodeNameGenerator.Generate(pinName);
            }

            component.Connect(nodes);
        }

        public virtual void Read(Statements statements, ISpiceStatementsOrderer orderer)
        {
            foreach (var statement in orderer.Order(statements))
            {
                StatementsReader.Read(statement, this);
            }
        }

        public void SetParameter(string pName, string expression)
        {
            this.ReadingExpressionContext.SetParameter(
                pName,
                expression,
                ExpressionParser.Parse(
                    expression,
                    new ExpressionParserContext(CaseSensitivity.IsFunctionNameCaseSensitive)
                        {
                            Functions = this.ReadingExpressionContext.Functions
                        }).FoundParameters);
        }

        public void AddFunction(string functionName, List<string> arguments, string body)
        {
            FunctionFactory factory = new FunctionFactory();
            this.ReadingExpressionContext.Functions.Add(functionName, factory.Create(functionName, arguments, body));
        }

        public void SetNamedExpression(string expressionName, string expression)
        {
            this.ReadingExpressionContext.SetNamedExpression(expressionName, expression,
                ExpressionParser.Parse(
                    expression,
                    new ExpressionParserContext(CaseSensitivity.IsFunctionNameCaseSensitive)
                        {
                            Functions = this.ReadingExpressionContext.Functions
                        }).FoundParameters);
        }

        public void SetParameter(Entity entity, string parameterName, string expression, bool onload = true)
        {
            IEqualityComparer<string> comparer = StringComparerProvider.Get(CaseSensitivity.IsEntityParameterNameCaseSensitive);

            double value = ReadingEvaluator.EvaluateValueExpression(expression, this.ReadingExpressionContext);

            if (double.IsNaN(value))
            {
                value = 0;
            }

            if (!entity.SetParameter(parameterName, value, comparer))
            {
                throw new Exception($"Uknown parameter {parameterName} for entity {entity.Name}");
            }

            var parseResult = ReadingEvaluator.ExpressionParser.Parse(
                expression,
                new ExpressionParserContext(CaseSensitivity.IsFunctionNameCaseSensitive)
                    {
                        Functions = this.ReadingExpressionContext.Functions
                    });

            if (parseResult.IsConstantExpression == false)
            {
                SimulationsParameters.SetParameter(this.SimulationExpressionContexts, entity, parameterName, expression, 0, onload, comparer);
            }
        }
    }
}
