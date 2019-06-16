using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// Reading context.
    /// </summary>
    public class ReadingContext : IReadingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadingContext"/> class.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <param name="parser">Expression parser.</param>
        /// <param name="simulationPreparations">Parameters.</param>
        /// <param name="simulationEvaluators">Evaluator for the context.</param>
        /// <param name="contexts">Context. </param>
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
        /// <param name="exporters">Exporters.</param>
        /// <param name="workingDirectory">Working directory.</param>
        public ReadingContext(
            string contextName,
            IExpressionParser parser,
            ISimulationPreparations simulationPreparations,
            ISimulationEvaluators simulationEvaluators,
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
            IReadingContext parent,
            IMapper<Exporter> exporters,
            string workingDirectory = null)
        {
            Name = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            NodeNameGenerator = nodeNameGenerator ?? throw new ArgumentNullException(nameof(nodeNameGenerator));
            ComponentNameGenerator = componentNameGenerator ?? throw new ArgumentNullException(nameof(componentNameGenerator));
            ModelNameGenerator = modelNameGenerator ?? throw new ArgumentNullException(nameof(componentNameGenerator));
            Parent = parent;
            Children = new List<IReadingContext>();
            CaseSensitivity = caseSettings;
            SimulationExpressionContexts = contexts;
            ExpressionParser = parser;
            ReadingExpressionContext = readingExpressionContext;
            SimulationPreparations = simulationPreparations;
            SimulationEvaluators = simulationEvaluators;
            StatementsReader = statementsReader;
            WaveformReader = waveformReader;
            ReadingEvaluator = readingEvaluator;

            AvailableSubcircuits = CreateAvailableSubcircuitsCollection();
            ModelsRegistry = CreateModelsRegistry();
            Exporters = exporters;
            WorkingDirectory = workingDirectory;
        }

        /// <summary>
        /// Gets the working directory.
        /// </summary>
        public string WorkingDirectory { get; }

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
        public ISimulationEvaluators SimulationEvaluators { get; }

        /// <summary>
        /// Gets the reading evaluator.
        /// </summary>
        public IEvaluator ReadingEvaluator { get; }

        /// <summary>
        /// Gets or sets exporter mapper.
        /// </summary>
        public IMapper<Exporter> Exporters { get; set; }

        /// <summary>
        /// Gets the expression parser.
        /// </summary>
        public IExpressionParser ExpressionParser { get; }

        /// <summary>
        /// Gets the simulation expression contexts.
        /// </summary>
        public SimulationExpressionContexts SimulationExpressionContexts { get; }

        /// <summary>
        /// Gets or sets the reading expression context.
        /// </summary>
        public ExpressionContext ReadingExpressionContext { get; set; }

        /// <summary>
        /// Gets simulation parameters.
        /// </summary>
        public ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets or sets available subcircuits in context.
        /// </summary>
        public ICollection<SubCircuit> AvailableSubcircuits { get; protected set; }

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
        /// <param name="expression">Expression string.</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            if (nodeName == null)
            {
                throw new ArgumentNullException(nameof(nodeName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var nodeId = NodeNameGenerator.Generate(nodeName);
            SimulationPreparations.SetICVoltage(nodeId, expression);
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
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            try
            {
                return ReadingEvaluator.EvaluateValueExpression(expression, ReadingExpressionContext);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of expression: " + expression, ex);
            }
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        public void SetParameter(string parameterName, double value)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }
            ReadingExpressionContext.SetParameter(parameterName, value);
        }

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component.</param>
        /// <param name="parameters">Parameters of component.</param>
        public void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.Count < component.PinCount)
            {
                throw new WrongParametersCountException(
                    "Too less parameters for: " + component.Name + " to create nodes");
            }

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
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            if (orderer == null)
            {
                throw new ArgumentNullException(nameof(orderer));
            }

            var orderedStatements = orderer.Order(statements);
            foreach (var statement in orderedStatements)
            {
                StatementsReader.Read(statement, this);
            }
        }

        public void SetParameter(string parameterName, string expression)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            ReadingExpressionContext.SetParameter(
                parameterName,
                expression,
                ExpressionParser.Parse(expression, new ExpressionParserContext(ReadingExpressionContext.Name, ReadingExpressionContext.Functions)).FoundParameters);
        }

        public void AddFunction(string functionName, List<string> arguments, string body)
        {
            if (functionName == null)
            {
                throw new ArgumentNullException(nameof(functionName));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }
            FunctionFactory factory = new FunctionFactory();
            ReadingExpressionContext.AddFunction(functionName, factory.Create(functionName, arguments, body));
        }

        public void SetNamedExpression(string expressionName, string expression)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var foundParameters = ExpressionParser.Parse(
                expression,
                new ExpressionParserContext(ReadingExpressionContext.Name, ReadingExpressionContext.Functions)).FoundParameters;

            ReadingExpressionContext.SetNamedExpression(expressionName, expression, foundParameters);
        }

        public void SetParameter(Entity entity, string parameterName, string expression, bool onload = true)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            IEqualityComparer<string> comparer = StringComparerProvider.Get(CaseSensitivity.IsEntityParameterNameCaseSensitive);

            double value = ReadingEvaluator.EvaluateValueExpression(expression, ReadingExpressionContext);

            if (!entity.SetParameter(parameterName, value, comparer))
            {
                throw new Exception($"Unknown parameter {parameterName} for entity {entity.Name}");
            }

            var parseResult = ReadingEvaluator.ExpressionParser.Parse(
                expression,
                new ExpressionParserContext(ReadingExpressionContext.Name, ReadingExpressionContext.Functions));

            if (parseResult.IsConstantExpression == false)
            {
                SimulationPreparations.SetParameter(entity, parameterName, expression, true, onload);
            }
        }

        protected ICollection<SubCircuit> CreateAvailableSubcircuitsCollection()
        {
            if (Parent != null)
            {
                return new List<SubCircuit>(Parent.AvailableSubcircuits);
            }
            else
            {
                return new List<SubCircuit>();
            }
        }

        protected IModelsRegistry CreateModelsRegistry()
        {
            if (Parent != null)
            {
                var generators = new List<IObjectNameGenerator>();
                IReadingContext current = this;
                while (current != null)
                {
                    generators.Add(current.ModelNameGenerator);
                    current = current.Parent;
                }

                return Parent.ModelsRegistry.CreateChildRegistry(generators);
            }
            else
            {
                var generators = new List<IObjectNameGenerator>();
                generators.Add(ModelNameGenerator);
                return new StochasticModelsRegistry(generators, CaseSensitivity.IsEntityNameCaseSensitive);
            }
        }
    }
}
