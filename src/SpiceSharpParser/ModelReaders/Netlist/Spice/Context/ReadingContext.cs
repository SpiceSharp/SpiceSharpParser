using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
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
        /// <param name="parameters">Parameters.</param>
        /// <param name="evaluators">Evaluator for the context.</param>
        /// <param name="resultService">SpiceSharpModel service for the context.</param>
        /// <param name="nodeNameGenerator">Name generator for the nodes.</param>
        /// <param name="componentNameGenerator">Name generator for the components.</param>
        /// <param name="modelNameGenerator">Name generator for the models.</param>
        /// <param name="statementsReader">Statements reader.</param>
        /// <param name="waveformReader">Waveform reader.</param>
        /// <param name="caseSettings">Case settings</param>
        /// <param name="parent">Parent of th context.</param>
        public ReadingContext(
            string contextName,
            ISimulationsParameters parameters,
            IEvaluatorsContainer evaluators,
            IResultService resultService,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator,
            ISpiceStatementsReader statementsReader,
            IWaveformReader waveformReader,
            CaseSensitivitySettings caseSettings,
            IReadingContext parent = null)
        {
            ContextName = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            NodeNameGenerator = nodeNameGenerator ?? throw new ArgumentNullException(nameof(nodeNameGenerator));
            ComponentNameGenerator = componentNameGenerator ?? throw new ArgumentNullException(nameof(componentNameGenerator));
            ModelNameGenerator = modelNameGenerator ?? throw new ArgumentNullException(nameof(componentNameGenerator));
            Parent = parent;

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
            Evaluators = evaluators;

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

            ModelsRegistry = new StochasticModelsRegistry(generators, caseSettings.IsEntityNameCaseSensitive);
        }

        /// <summary>
        /// Gets the evaluators for the context.
        /// </summary>
        public IEvaluatorsContainer Evaluators { get; }

        /// <summary>
        /// Gets simulation parameters.
        /// </summary>
        public ISimulationsParameters SimulationsParameters { get; }

        /// <summary>
        /// Gets or sets the name of context.
        /// </summary>
        public string ContextName { get; protected set; }

        /// <summary>
        /// Gets or sets the parent of context.
        /// </summary>
        public IReadingContext Parent { get; protected set; }

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

        public CaseSensitivitySettings CaseSensitivity { get; set; }

        /// <summary>
        /// Sets voltage initial condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression.</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            var nodeId = NodeNameGenerator.Generate(nodeName);
            SimulationsParameters.SetICVoltage(nodeId, expression);
        }

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse.</param>
        /// <returns>
        /// A value of expression..
        /// </returns>
        public double ParseDouble(string expression)
        {
            try
            {
                return Evaluators.EvaluateDouble(expression);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of expression: " + expression, ex);
            }
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

        public void SetParameter(Entity entity, string parameterName, string expression, bool onload = true)
        {
            IEqualityComparer<string> comparer = StringComparerFactory.Create(CaseSensitivity.IsEntityParameterNameCaseSensitive);
            entity.SetParameter(parameterName, Evaluators.EvaluateDouble(expression), comparer);
            SimulationsParameters.SetParameter(entity, parameterName, expression, 0, onload, comparer);
        }
    }
}
