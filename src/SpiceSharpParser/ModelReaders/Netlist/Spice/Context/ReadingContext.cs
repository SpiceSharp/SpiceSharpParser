using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
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
        /// <param name="readingEvaluator">Evaluator for the context.</param>
        /// <param name="resultService">Result service for the context.</param>
        /// <param name="nodeNameGenerator">Node name generator for the context.</param>
        /// <param name="objectNameGenerator">Object name generator for the context.</param>
        /// <param name="parent">Parent of th econtext.</param>
        public ReadingContext(
            string contextName,
            ISimulationContexts simulationContexts,
            IEvaluator readingEvaluator,
            IResultService resultService,
            INodeNameGenerator nodeNameGenerator,
            IObjectNameGenerator objectNameGenerator,
            IReadingContext parent = null)
        {
            ContextName = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            ReadingEvaluator = readingEvaluator ?? throw new ArgumentNullException(nameof(readingEvaluator));
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
            SimulationContexts = simulationContexts;

            var generators = new List<IObjectNameGenerator>();
            IReadingContext current = this;
            while (current != null)
            {
                generators.Add(current.ObjectNameGenerator);
                current = current.Parent;
            }

            StochasticModelsRegistry = new StochasticModelsRegistry(generators);
        }

        /// <summary>
        /// Gets or sets the simulation contexts.
        /// </summary>
        public ISimulationContexts SimulationContexts { get; protected set; }

        /// <summary>
        /// Gets or sets the name of context.
        /// </summary>
        public string ContextName { get; protected set; }

        /// <summary>
        /// Gets or sets the reading evaluator.
        /// </summary>
        public IEvaluator ReadingEvaluator { get; protected set; }

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
        public IStochasticModelsRegistry StochasticModelsRegistry { get; protected set; }

        /// <summary>
        /// Sets voltage initial condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression.</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            var fullNodeName = NodeNameGenerator.Generate(nodeName);
            var initialValue = ReadingEvaluator.EvaluateDouble(expression);

            SimulationContexts.SetICVoltage(nodeName, expression);
        }

        /// <summary>
        /// Sets voltage guess condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression.</param>
        public void SetNodeSetVoltage(string nodeName, string expression)
        {
            foreach (var simulation in Result.Simulations)
            {
                simulation.Nodes.NodeSets[nodeName] = ReadingEvaluator.EvaluateDouble(expression);
            }
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
                return ReadingEvaluator.EvaluateDouble(expression);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during evaluation of expression: " + expression, ex);
            }
        }

        /// <summary>
        /// Sets the parameter of entity and enables updates.
        /// </summary>
        /// <param name="entity">An entity of parameter.</param>
        /// <param name="parameterName">A parameter name.</param>
        /// <param name="expression">An expression.</param>
        /// <returns>
        /// True if the parameter has been set.
        /// </returns>
        public bool SetParameter(Entity entity, string parameterName, string expression)
        {
            double value;
            try
            {
                value = ReadingEvaluator.EvaluateDouble(expression);
            }
            catch (Exception ex)
            {
                Result.AddWarning("Exception during parsing expression '" + expression + "': " + ex);
                return false;
            }

            bool wasSet = entity.SetParameter(parameterName.ToLower(), value);

            if (wasSet)
            {
                SimulationContexts.SetEntityParameter(parameterName.ToLower(), entity, expression);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the parameter of entity.
        /// </summary>
        /// <param name="entity">An entity of parameter.</param>
        /// <param name="parameterName">A parameter name.</param>
        /// <param name="object">An parameter value.</param>
        /// <returns>
        /// True if the parameter has been set.
        /// </returns>
        [Obsolete]
        public bool SetParameter(Entity entity, string parameterName, object @object)
        {
            return entity.SetParameter(parameterName.ToLower(), @object);
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
    }
}
