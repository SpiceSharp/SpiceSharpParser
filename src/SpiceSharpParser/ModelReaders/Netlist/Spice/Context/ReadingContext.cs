using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

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
        /// <param name="resultService">Result service for the context.</param>
        /// <param name="nodeNameGenerator">Node name generator for the context.</param>
        /// <param name="objectNameGenerator">Object name generator for the context.</param>
        /// <param name="statementsReader">Statements reader.</param>
        /// <param name="waveformReader">Waveform reader.</param>
        /// <param name="parent">Parent of th econtext.</param>
        public ReadingContext(
            string contextName,
            ISimulationsParameters parameters,
            ISimulationEvaluators evaluators,
            IResultService resultService,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator objectNameGenerator,
            ISpiceStatementsReader statementsReader,
            IWaveformReader waveformReader,
            IReadingContext parent = null)
        {
            ContextName = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            NodeNameGenerator = nodeNameGenerator ?? throw new ArgumentNullException(nameof(nodeNameGenerator));
            ObjectNameGenerator = objectNameGenerator ?? throw new ArgumentNullException(nameof(objectNameGenerator));
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
            SimulationEvaluators = evaluators;

            var generators = new List<IObjectNameGenerator>();
            IReadingContext current = this;
            while (current != null)
            {
                generators.Add(current.ObjectNameGenerator);
                current = current.Parent;
            }

            ModelsRegistry = new StochasticModelsRegistry(generators);
            StatementsReader = statementsReader;
            WaveformReader = waveformReader;
        }

        /// <summary>
        /// Gets or sets the evaluators.
        /// </summary>
        public ISimulationEvaluators SimulationEvaluators { get; }

        /// <summary>
        /// Gets or sets simulation parameters.
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
        /// Gets or sets the object name generator.
        /// </summary>
        public IObjectNameGenerator ObjectNameGenerator { get; protected set; }

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

        /// <summary>
        /// Sets voltage initial condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression.</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            var fullNodeName = NodeNameGenerator.Generate(nodeName);
            SimulationsParameters.SetICVoltage(nodeName, expression);
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
                return SimulationEvaluators.EvaluateDouble(expression);
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
            Identifier[] nodes = new Identifier[component.PinCount];
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
            entity.SetParameter(parameterName.ToLower(), this.SimulationEvaluators.EvaluateDouble(expression));
            SimulationsParameters.SetParameter(entity, parameterName.ToLower(), expression, 0, onload);
        }

        public void SetParameter(Entity entity, string parameterName, double value)
        {
            entity.SetParameter(parameterName.ToLower(), value);
        }
    }
}
