using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processing context
    /// </summary>
    public class ProcessingContext
    {
        private string currentPath = null;

        public ProcessingContext()
        {
            ContextName = string.Empty;
            Netlist = new Netlist(new Circuit(), string.Empty);
            AvailableSubcircuits = new List<SubCircuit>();
            AvailableParameters = new Dictionary<string, double>();
            Evaluator = new Evaluator(AvailableParameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingContext"/> class.
        /// </summary>
        /// <param name="contextName">The name of context</param>
        /// <param name="netlist">The netlist for context</param>
        public ProcessingContext(string contextName, Netlist netlist)
        {
            ContextName = contextName;
            Netlist = netlist;
            AvailableSubcircuits = new List<SubCircuit>();
            AvailableParameters = new Dictionary<string, double>();
            Evaluator = new Evaluator(AvailableParameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingContext"/> class.
        /// </summary>
        public ProcessingContext(string contextName, 
            ProcessingContext parent, 
            SubCircuit currentSubciruit, 
            List<string> pinInstanceNames, 
            Dictionary<string, double> availableParameters)
        {
            ContextName = contextName;
            Netlist = parent.Netlist;
            Parent = parent;
            CurrrentSubCircuit = currentSubciruit;
            PinInstanceNames = pinInstanceNames;
            AvailableSubcircuits = new List<SubCircuit>();
            AvailableSubcircuits.AddRange(parent.AvailableSubcircuits);
            AvailableParameters = availableParameters;
            Evaluator = new Evaluator(AvailableParameters);
        }

        /// <summary>
        /// Gets the available paremeters values
        /// </summary>
        public Dictionary<string, double> AvailableParameters { get; }

        /// <summary>
        /// Gets the evaluator
        /// </summary>
        public Evaluator Evaluator { get; }

        /// <summary>
        /// Gets the current simulation configuration
        /// </summary>
        public SimulationConfiguration SimulationConfiguration { get; } = new SimulationConfiguration();

        /// <summary>
        /// Gets the available definions of subcircuits
        /// </summary>
        public List<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the created simulations
        /// </summary>
        public IEnumerable<Simulation> Simulations
        {
            get
            {
                return Netlist.Simulations;
            }
        }

        /// <summary>
        /// Gets the netlist
        /// </summary>
        protected Netlist Netlist { get; }

        /// <summary>
        /// Gets the context name
        /// </summary>
        protected string ContextName { get; }

        /// <summary>
        /// Gets the current subcircuit
        /// </summary>
        protected SubCircuit CurrrentSubCircuit { get; }

        /// <summary>
        /// Gets the names of pinds for the current subcircuit
        /// </summary>
        protected List<string> PinInstanceNames { get; }

        /// <summary>
        /// Gets the parent of the context
        /// </summary>
        protected ProcessingContext Parent { get;  }

        /// <summary>
        /// Gets the path of the context
        /// </summary>
        protected string Path
        {
            get
            {
                if (currentPath == null)
                {
                    List<string> path = new List<string>() { ContextName };

                    var context = this;
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

                    currentPath = result;
                }

                return currentPath;
            }
        }

        /// <summary>
        /// Adds warning
        /// </summary>
        public void AddWarning(string warning)
        {
            this.Netlist.Warnings.Add(warning);
        }

        /// <summary>
        /// Adds comment
        /// </summary>
        public void AddComment(CommentLine statement)
        {
            this.Netlist.Comments.Add(statement.Text);
        }

        /// <summary>
        /// Adds export to netlist
        /// </summary>
        public void AddExport(Export export)
        {
            Netlist.Exports.Add(export);
        }

        /// <summary>
        /// Adds plot to netlist
        /// </summary>
        public void AddPlot(Plot plot)
        {
            Netlist.Plots.Add(plot);
        }

        /// <summary>
        /// Adds entity to netlist
        /// </summary>
        public void AddEntity(Entity entity)
        {
            Netlist.Circuit.Objects.Add(entity);
        }

        /// <summary>
        /// Adds simulation to netlist
        /// </summary>
        public void AddSimulation(BaseSimulation simulation)
        {
            Netlist.Simulations.Add(simulation);
        }

        /// <summary>
        /// Sets voltage initial condition for node
        /// </summary>
        public void SetICVoltage(string nodeName, string value)
        {
            Netlist.Circuit.Nodes.InitialConditions[this.GenerateNodeName(nodeName)] = Evaluator.EvaluteDouble(value);
        }

        /// <summary>
        /// Evaluates the value to double
        /// </summary>
        public double ParseDouble(string value)
        {
            return Evaluator.EvaluteDouble(value);
        }

        public void SetProperty(Entity entity, string propertyName, string rawValue)
        {
            var value = this.Evaluator.EvaluteDouble(rawValue);
            entity.ParameterSets.SetProperty(propertyName, value, out SpiceSharp.Parameter param);
            Evaluator.EnableRefresh(propertyName, param, rawValue);
        }

        /// <summary>
        /// Find model in the context and in parent contexts
        /// </summary>
        public T FindModel<T>(string modelName)
            where T : Entity
        {
            var context = this;
            while (context != null)
            {
                var modelNameToSearch = (context.Path + modelName).ToLower();

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
        public void SetEntityParameters(Entity entity, ParameterCollection parameters, int toSkip = 0)
        {
            foreach (SpiceObjects.Parameter parameter in parameters.Skip(toSkip).Take(parameters.Count - toSkip))
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        entity.ParameterSets.SetProperty(ap.Name, Evaluator.EvaluteDouble(ap.Value), out SpiceSharp.Parameter param);
                        this.Evaluator.EnableRefresh(ap.Name, param, ap.Value);
                    }
                    catch (Exception ex)
                    {
                        Netlist.Warnings.Add(ex.ToString());
                    }
                }
                else
                {
                    Netlist.Warnings.Add("Unknown parameter: " + parameter.Image);
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
                nodes[i] = GenerateNodeName(pinName);
            }

            component.Connect(nodes);
        }

        /// <summary>
        /// Generates object name for current context
        /// </summary>
        public string GenerateObjectName(string objectName)
        {
            return Path + objectName;
        }

        /// <summary>
        /// Generates node name for current context
        /// </summary>
        public string GenerateNodeName(string pinName)
        {
            if (pinName == "0" || pinName == "gnd" || pinName == "GND")
            {
                return pinName.ToUpper();
            }

            if (CurrrentSubCircuit != null)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();

                for (var i = 0; i < this.CurrrentSubCircuit.Pins.Count; i++)
                {
                    map[CurrrentSubCircuit.Pins[i]] = this.PinInstanceNames[i];
                }

                if (map.ContainsKey(pinName))
                {
                    return map[pinName].ToLower();
                }
            }

            return (Path + pinName).ToLower();
        }
    }
}
