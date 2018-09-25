using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IReadingContext
    {
        /// <summary>
        /// Gets the context name.
        /// </summary>
        string ContextName { get; }

        /// <summary>
        /// Gets the simulations contexts.
        /// </summary>
        ISimulationContexts SimulationContexts { get; }

        /// <summary>
        /// Gets the parent of the context.
        /// </summary>
        IReadingContext Parent { get;  }

        /// <summary>
        /// Gets the children of the reading context.
        /// </summary>
        ICollection<IReadingContext> Children { get; }

        /// <summary>
        /// Gets the list of available subcircuit for the context.
        /// </summary>
        ICollection<SubCircuit> AvailableSubcircuits { get; }        

        /// <summary>
        /// Gets the result service for the context.
        /// </summary>
        IResultService Result { get; }

        /// <summary>
        /// Gets the node name generator.
        /// </summary>
        INodeNameGenerator NodeNameGenerator { get; }

        /// <summary>
        /// Gets the object name generator.
        /// </summary>
        IObjectNameGenerator ObjectNameGenerator { get; }

        /// <summary>
        /// Gets the evaluator for the reading of the model.
        /// </summary>
        IEvaluator ReadingEvaluator { get; }

        /// <summary>
        /// Gets the stochastic models registry.
        /// </summary>
        IStochasticModelsRegistry StochasticModelsRegistry { get; }

        /// <summary>
        /// Gets the statements reader.
        /// </summary>
        ISpiceStatementsReader StatementsReader { get; set; }

        /// <summary>
        /// Gets the waveform reader.
        /// </summary>
        IWaveformReader WaveformReader { get; set; }

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double ParseDouble(string expression);

        /// <summary>
        /// Sets voltage initial condition for node
        /// </summary>
        /// <param name="nodeName">Name of node</param>
        /// <param name="expression">Expression</param>
        void SetICVoltage(string nodeName, string expression);

        /// <summary>
        /// Sets voltage guess condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node</param>
        /// <param name="expression">Expression</param>
        void SetNodeSetVoltage(string nodeName, string expression);

        /// <summary>
        /// Sets the parameter of entity.
        /// </summary>
        /// <param name="entity">An entity of parameter</param>
        /// <param name="parameterName">A parameter name</param>
        /// <param name="expression">An expression</param>
        /// <returns>
        /// True if parameter has been set.
        /// </returns>
        bool SetParameter(Entity entity, string parameterName, string expression);

        /// <summary>
        /// Sets the parameter of entity.
        /// </summary>
        /// <param name="entity">An entity of parameter</param>
        /// <param name="parameterName">A parameter name</param>
        /// <param name="parameterValue">Function that provides parameter value.</param>
        /// <returns>
        /// True if parameter has been set.
        /// </returns>
        bool SetParameter(Entity entity, string parameterName, Func<object, double> parameterValue);

        /// <summary>
        /// Sets the parameter of entity.
        /// </summary>
        /// <param name="entity">An entity of parameter</param>
        /// <param name="parameterName">A parameter name</param>
        /// <param name="object">An parameter value</param>
        /// <returns>
        /// True if the parameter has been set.
        /// </returns>
        bool SetParameter(Entity entity, string parameterName, object @object);

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component</param>
        /// <param name="parameters">Parameters of component</param>
        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);

        /// <summary>
        /// Reads the statements with given order.
        /// </summary>
        /// <param name="statements">Statements.</param>
        /// <param name="orderer">Orderer.</param>
        void Read(Statements statement, ISpiceStatementsOrderer orderer);
    }
}
