using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    /// <summary>
    /// Processing context
    /// </summary>
    public class ProcessingContext : IProcessingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingContext"/> class.
        /// </summary>
        public ProcessingContext(
            string contextName,
            IEvaluator evaluator,
            IResultService resultService,
            NodeNameGenerator nodeNameGenerator,
            ObjectNameGenerator objectNameGenerator,
            IProcessingContext parent = null)
        {
            ContextName = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            NodeNameGenerator = nodeNameGenerator ?? throw new ArgumentNullException(nameof(nodeNameGenerator));
            ObjectNameGenerator = objectNameGenerator ?? throw new ArgumentNullException(nameof(objectNameGenerator));
            Parent = parent;

            if (Parent != null)
            {
                AvailableSubcircuits.AddRange(Parent.AvailableSubcircuits);
            }
        }

        /// <summary>
        /// Gets or sets the name of context
        /// </summary>
        public string ContextName { get; protected set; }

        /// <summary>
        /// Gets or sets the parent of context
        /// </summary>
        public IProcessingContext Parent { get; protected set; }

        /// <summary>
        /// Gets available subcircuits in context
        /// </summary>
        public List<SubCircuit> AvailableSubcircuits { get; } = new List<SubCircuit>();

        /// <summary>
        /// Gets or sets the evaluator
        /// </summary>
        public IEvaluator Evaluator { get; protected set; }

        /// <summary>
        /// Gets or sets the result service
        /// </summary>
        public IResultService Result { get; protected set; }

        /// <summary>
        /// Gets or sets the node name generator
        /// </summary>
        public NodeNameGenerator NodeNameGenerator { get; protected set; }

        /// <summary>
        /// Gets or sets the object name generator
        /// </summary>
        public ObjectNameGenerator ObjectNameGenerator { get; protected set; }

        /// <summary>
        /// Sets voltage initial condition for node
        /// </summary>
        /// <param name="nodeName">Name of node</param>
        /// <param name="expression">Expression</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            Result.SetInitialVoltageCondition(NodeNameGenerator.Generate(nodeName), Evaluator.EvaluateDouble(expression, out _));
        }

        /// <summary>
        /// Evaluates the value to double
        /// </summary>
        /// <param name="expression">Expressin to evaluate</param>
        /// <returns>
        /// A value of expression
        /// </returns>
        public double ParseDouble(string expression)
        {
            try
            {
                return Evaluator.EvaluateDouble(expression, out _);
            }
            catch (Exception)
            {
                throw new Exception("Exception during evaluation of expression: " + expression);
            }
        }

        /// <summary>
        /// Sets the parameter of entity and enables updates
        /// </summary>
        /// <param name="entity">An entity of parameter</param>
        /// <param name="parameterName">A parameter name</param>
        /// <param name="expression">An expression</param>
        public void SetParameter(Entity entity, string parameterName, string expression)
        {
            var value = Evaluator.EvaluateDouble(expression, out var parameters);
            entity.SetParameter(parameterName, value);

            var setter = entity.ParameterSets.GetSetter(parameterName);
            // re-evaluation makes sense only if there is a setter
            if (setter != null)
            {
                Evaluator.AddDynamicExpression(new DoubleExpression(expression, setter));
            }
        }

        /// <summary>
        /// Find model in the context and in parent contexts
        /// </summary>
        public T FindModel<T>(string modelName) where T : Entity
        {
            IProcessingContext context = this;
            while (context != null)
            {
                var modelNameToSearch = context.ObjectNameGenerator.Generate(modelName);

                Entity model;
                if (Result.FindObject(modelNameToSearch, out model))
                {
                    return (T)model;
                }

                context = context.Parent;
            }

            return null;
        }

        /// <summary>
        /// Sets entity parameters
        /// </summary>
        public void SetParameters(Entity entity, ParameterCollection parameters, int toSkip = 0)
        {
            foreach (SpiceObjects.Parameter parameter in parameters.Skip(toSkip).Take(parameters.Count - toSkip))
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        SetParameter(entity, ap.Name, ap.Value);
                    }
                    catch (Exception ex)
                    {
                        Result.AddWarning(ex.ToString());
                    }
                }
                else
                {
                    Result.AddWarning("Unknown parameter: " + parameter.Image);
                }
            }
        }

        /// <summary>
        /// Creates nodes for component
        /// </summary>
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
