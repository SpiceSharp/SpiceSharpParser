using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Evaluation;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public interface IProcessingContext
    {
        /// <summary>
        /// Gets the context name
        /// </summary>
        string ContextName { get; }

        /// <summary>
        /// Gets the parent of the context
        /// </summary>
        IProcessingContext Parent { get;  }

        /// <summary>
        /// Gets the children of the processing context
        /// </summary>
        ICollection<IProcessingContext> Children { get; }

        /// <summary>
        /// Gets the list of available subcircuit for the context
        /// </summary>
        ICollection<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the evaluator for the context
        /// </summary>
        IEvaluator Evaluator { get; }

        /// <summary>
        /// Gets the result service for the context
        /// </summary>
        IResultService Result { get; }

        /// <summary>
        /// Gets the node name generator
        /// </summary>
        INodeNameGenerator NodeNameGenerator { get; }

        /// <summary>
        /// Gets the object name generator
        /// </summary>
        IObjectNameGenerator ObjectNameGenerator { get; }

        /// <summary>
        /// Parses an expression to double
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <returns>
        /// A value of expression
        /// </returns>
        double ParseDouble(string expression);

        /// <summary>
        /// Sets voltage initial condition for node
        /// </summary>
        /// <param name="nodeName">Name of node</param>
        /// <param name="expression">Expression</param>
        void SetICVoltage(string nodeName, string expression);

        /// <summary>
        /// Sets the parameter of entity and enables updates
        /// </summary>
        /// <param name="entity">An entity of parameter</param>
        /// <param name="parameterName">A parameter name</param>
        /// <param name="expression">An expression</param>
        /// <returns>
        /// True if parameter has been set
        /// </returns>
        bool SetParameter(Entity entity, string parameterName, string expression);

        /// <summary>
        /// Finds model in the context and in parent contexts
        /// </summary>
        /// <param name="modelName">Name of model to find</param>
        /// <returns>
        /// A reference to model
        /// </returns>
        T FindModel<T>(string modelName)
            where T : Entity;

        /// <summary>
        /// Creates nodes for a component
        /// </summary>
        /// <param name="component">A component</param>
        /// <param name="parameters">Parameters of component</param>
        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);
    }
}
