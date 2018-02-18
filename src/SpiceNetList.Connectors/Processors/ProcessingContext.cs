using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ProcessingContext
    {
        private string currentPath = null;

        public ProcessingContext(string contextName, NetList netlist)
        {
            this.ContextName = contextName;
            this.Netlist = netlist;
            this.AvailableSubcircuits = new List<SubCircuit>();
            this.AvailableParameters = new Dictionary<string, double>();
        }
        
        public ProcessingContext(string contextName, ProcessingContext parent, SubCircuit currentSubciruit, List<string> pinInstanceNames, Dictionary<string, double> availableParameters) 
        {
            this.ContextName = contextName;
            this.Netlist = parent.Netlist;
            this.Parent = parent;
            this.CurrrentSubCircuit = currentSubciruit;
            this.PinInstanceNames = pinInstanceNames;
            this.AvailableSubcircuits = new List<SubCircuit>();
            this.AvailableSubcircuits.AddRange(parent.AvailableSubcircuits);
            this.AvailableParameters = availableParameters;
        }

        /// <summary>
        /// Gets dictionary of available
        /// </summary>
        public Dictionary<string, double> AvailableParameters { get; }

        public GlobalConfiguration GlobalConfiguration { get; set; } = new GlobalConfiguration();

        public List<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets numer of simulations
        /// </summary>
        public int SimulationsCount
        {
            get
            {
                return Netlist.Simulations.Count;
            }
        }

        /// <summary>
        /// Gets first simulation
        /// </summary>
        public Simulation FirstSimulation
        {
            get
            {
                return Netlist.Simulations[0];
            }
        }

        protected NetList Netlist { get; }

        protected string ContextName { get; }

        protected SubCircuit CurrrentSubCircuit { get; }

        protected List<string> PinInstanceNames { get; }

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
            Netlist.Circuit.Nodes.InitialConditions[this.GenerateNodeName(nodeName)] = this.ParseDouble(value);
        }

        /// <summary>
        /// Parses value to double
        /// </summary>
        public double ParseDouble(string value)
        {
            if (AvailableParameters.ContainsKey(value))
            {
                return AvailableParameters[value];
            }
            var spiceExpressionParser = new SpiceSharpConnector.Expressions.SpiceExpression();
            spiceExpressionParser.Parameters = AvailableParameters;

            return spiceExpressionParser.Parse(value.Trim('{', '}'));
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
                        entity.ParameterSets.SetProperty(ap.Name, this.ParseDouble(ap.Value));
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
            if (pinName == "0")
            {
                return pinName;
            }

            if (this.CurrrentSubCircuit != null)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();

                for (var i = 0; i < this.CurrrentSubCircuit.Pins.Count; i++)
                {
                    map[this.CurrrentSubCircuit.Pins[i]] = this.PinInstanceNames[i];
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
