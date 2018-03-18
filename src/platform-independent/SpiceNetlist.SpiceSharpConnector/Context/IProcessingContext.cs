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
        /// Gets the list of available subcircuit for the context
        /// </summary>
        List<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the evaluator for the context
        /// </summary>
        IEvaluator Evaluator { get; }

        /// <summary>
        /// Gets the result service for the context
        /// </summary>
        IResultService Result { get; }

        /// <summary>
        /// 
        /// </summary>
        INodeNameGenerator NodeNameGenerator { get; }

        /// <summary>
        /// 
        /// </summary>
        IObjectNameGenerator ObjectNameGenerator { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        double ParseDouble(string expression);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="expression"></param>
        void SetICVoltage(string nodeName, string expression);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="expression"></param>
        void SetParameter(Entity entity, string propertyName, string expression);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelName"></param>
        /// <returns></returns>
        T FindModel<T>(string modelName)
            where T : Entity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parameters"></param>
        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);
    }
}
