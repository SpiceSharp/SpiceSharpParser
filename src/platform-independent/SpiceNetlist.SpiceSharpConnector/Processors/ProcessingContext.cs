using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processing context
    /// </summary>
    public class ProcessingContext : ProcessingContextBase
    {
        public ProcessingContext()
        {
            ContextName = string.Empty;
            Path = GetPath();
            Netlist = new Netlist(new Circuit(), string.Empty);
            AvailableSubcircuits = new List<SubCircuit>();
            Evaluator = new Evaluator();
            Adder = new NetlistAdder(Netlist);
            NameGenerator = new NameGenerator(Path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingContext"/> class.
        /// </summary>
        /// <param name="contextName">The name of context</param>
        /// <param name="netlist">The netlist for context</param>
        public ProcessingContext(string contextName, Netlist netlist)
        {
            ContextName = contextName;
            Path = GetPath();
            Netlist = netlist;
            AvailableSubcircuits = new List<SubCircuit>();
            Evaluator = new Evaluator();
            Adder = new NetlistAdder(netlist);
            NameGenerator = new NameGenerator(Path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingContext"/> class.
        /// </summary>
        public ProcessingContext(string contextName,
            ProcessingContextBase parent,
            SubCircuit currentSubciruit,
            List<string> pinInstanceNames)
        {
            ContextName = contextName;
            Parent = parent;
            Netlist = parent.Netlist;
            Path = GetPath();
            AvailableSubcircuits = new List<SubCircuit>();
            AvailableSubcircuits.AddRange(parent.AvailableSubcircuits);
            Evaluator = new Evaluator(parent.Evaluator);
            Adder = new NetlistAdder(parent.Netlist);
            NameGenerator = new NameGenerator(Path, currentSubciruit, pinInstanceNames);
        }

        /// <summary>
        /// Gets the created simulations
        /// </summary>
        public override IEnumerable<Simulation> Simulations
        {
            get
            {
                return Netlist.Simulations;
            }
        }

        /// <summary>
        /// Gets the path of the context
        /// </summary>
        /// <returns>
        /// The path of context
        /// </returns>
        public string GetPath()
        {
            List<string> path = new List<string>() { ContextName };

            ProcessingContextBase context = this;
            while (context.Parent != null)
            {
                path.Insert(0, context.Parent.ContextName);
                context = context.Parent;
            }

            var result = string.Empty;
            foreach (var pathPart in path)
            {
                if (pathPart != string.Empty)
                {
                    result += pathPart + ".";
                }
            }

            return result;
        }

        /// <summary>
        /// Sets voltage initial condition for node
        /// </summary>
        public override void SetICVoltage(string nodeName, string expression)
        {
            Netlist.Circuit.Nodes.InitialConditions[NameGenerator.GenerateNodeName(nodeName)] = Evaluator.EvaluateDouble(expression, out _);

            // TODO: Add dynamic parameter support :)
        }

        /// <summary>
        /// Evaluates the value to double
        /// </summary>
        /// <param name="expression">Expressin to evaluate</param>
        /// <returns>
        /// A value of expression
        /// </returns>
        public override double ParseDouble(string expression)
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
        /// <param name="propertyName">A property name</param>
        /// <param name="expression">An expression</param>
        public override void SetParameter(Entity entity, string propertyName, string expression)
        {
            entity.SetParameter(propertyName, Evaluator.EvaluateDouble(expression, out var parameters));

            var setter = entity.ParameterSets.GetSetter(propertyName);
            // re-evaluation makes sense only if there is a setter
            if (setter != null)
            {
                Evaluator.AddDynamicExpression(new DoubleExpression(expression, setter));
            }

            // find parameter to see if it's temperature dependent. if it's then add all expression parameters to TemperatureParameters
            if (parameters != null && parameters.Count > 0)
            {
                var parameter = entity.ParameterSets.GetParameter(propertyName);
                if (parameter != null)
                {
                    var attribute = entity.ParameterSets.GetParameterAttribute(propertyName);
                    if (attribute != null)
                    {
                        if (attribute.IsTemperatureDepended)
                        {
                            foreach (var param in parameters)
                            {
                                TemperatureParameters.Add(param);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find model in the context and in parent contexts
        /// </summary>
        public override T FindModel<T>(string modelName)
        {
            ProcessingContextBase context = this;
            while (context != null)
            {
                var modelNameToSearch = NameGenerator.GenerateObjectName(context.Path, modelName);

                Entity model;
                if (Netlist.Circuit.Objects.TryGetEntity(new Identifier(modelNameToSearch), out model))
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
        public override void SetParameters(Entity entity, ParameterCollection parameters, int toSkip = 0)
        {
            foreach (SpiceObjects.Parameter parameter in parameters.Skip(toSkip).Take(parameters.Count - toSkip))
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        this.SetParameter(entity, ap.Name, ap.Value);
                    }
                    catch (Exception ex)
                    {
                        Adder.AddWarning(ex.ToString());
                    }
                }
                else
                {
                    Adder.AddWarning("Unknown parameter: " + parameter.Image);
                }
            }
        }

        /// <summary>
        /// Creates nodes for component
        /// </summary>
        public override void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters)
        {
            Identifier[] nodes = new Identifier[component.PinCount];
            for (var i = 0; i < component.PinCount; i++)
            {
                string pinName = parameters.GetString(i);
                nodes[i] = NameGenerator.GenerateNodeName(pinName);
            }

            component.Connect(nodes);
        }
    }
}
