using System;
using System.Collections.Generic;
using System.Linq;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Expressions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ProcessingContext
    {
        private NetList netlist;
        private string name;
        private readonly SubCircuit subDef;
        private readonly List<string> pinInstanceNames;

        public ProcessingContext(string name, NetList netlist)
        {
            this.name = name;
            this.netlist = netlist;
            this.AvailableDefinitions = new List<SubCircuit>();
            this.AvailableParameters = new Dictionary<string, double>();
        }

        public ProcessingContext(string name, ProcessingContext parent, SubCircuit subDef, List<string> pinInstanceNames, Dictionary<string, double> availableParameters)
        {
            this.name = name;
            this.netlist = parent.netlist;
            this.Parent = parent;
            this.subDef = subDef;
            this.pinInstanceNames = pinInstanceNames;

            this.AvailableDefinitions = new List<SubCircuit>();
            this.AvailableDefinitions.AddRange(parent.AvailableDefinitions);
            this.AvailableParameters = availableParameters;
        }

        public List<SubCircuit> AvailableDefinitions { get; }

        public Dictionary<string, double> AvailableParameters { get; }

        public ProcessingContext Parent { get; }

        public BaseConfiguration BaseConfiguration { get; set; }

        public FrequencyConfiguration FrequencyConfiguration { get; set; }

        public TimeConfiguration TimeConfiguration { get; set; }

        public DCConfiguration DCConfiguration { get; set; }

        public int SimulationsCount
        {
            get
            {
                return netlist.Simulations.Count;
            }
        }


        internal Simulation GetSimulation()
        {
            return this.netlist.Simulations[0];
        }

        public void AddExport(Export export)
        {
            netlist.Exports.Add(export);
        }

        public void AddEntity(Entity entity)
        {
            netlist.Circuit.Objects.Add(entity);
        }

        public void AddSimulation(BaseSimulation simulation)
        {
            netlist.Simulations.Add(simulation);
        }

        public double ParseDouble(string value)
        {
            if (AvailableParameters.ContainsKey(value))
            {
                return AvailableParameters[value];
            }

            SpiceExpression spiceExpressionParser = new SpiceExpression();
            spiceExpressionParser.Parameters = AvailableParameters;

            return spiceExpressionParser.Parse(value.Trim('{', '}'));
        }

        public T FindModel<T>(string modelName)
            where T : Entity
        {
            var context = this;
            while (context != null)
            {
                var modelNameToSearch = (context.GetPath() + modelName).ToLower();

                Entity model;
                if (this.netlist.Circuit.Objects.TryGetEntity(new Identifier(modelNameToSearch), out model))
                {
                    return (T)model;
                }

                context = context.Parent;
            }

            return null;
        }

        public void SetParameters(Entity entity, ParameterCollection parameters, int toSkip = 0, int count = 0)
        {
            foreach (var parameter in parameters.Skip(toSkip).Take(parameters.Count - toSkip - count))
            {
                if (parameter is AssignmentParameter ap)
                {
                    try
                    {
                        entity.ParameterSets.SetProperty(ap.Name, this.ParseDouble(ap.Value));
                    }
                    catch(Exception ex)
                    {
                        this.netlist.Warnings.Add(ex.ToString());
                    }
                }
            }
        }

        public void CreateNodes(ParameterCollection parameters, SpiceSharp.Components.Component component)
        {
            Identifier[] nodes = new Identifier[component.PinCount];
            for (var i = 0; i < component.PinCount; i++)
            {
                string pinName = parameters.GetString(i);
                nodes[i] = GenerateNodeName(pinName);
            }

            component.Connect(nodes);
        }

        public string GenerateObjectName(string objectName)
        {
            return GetPath() + objectName;
        }

        public string GenerateNodeName(string pinName)
        {
            if (pinName == "0")
            {
                return pinName;
            }

            if (this.subDef != null)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();

                for (var i = 0; i < this.subDef.Pins.Count; i++)
                {
                    map[this.subDef.Pins[i]] = this.pinInstanceNames[i];
                }

                if (map.ContainsKey(pinName))
                {
                    return map[pinName].ToLower();
                }
            }

            return (GetPath() + pinName).ToLower();
        }

        private string GetPath()
        {
            List<string> path = new List<string>() { name };

            var context = this;
            while (context.Parent != null)
            {
                path.Insert(0, context.Parent.name);
                context = context.Parent;
            }

            var prefix = string.Empty;
            foreach (var pathPart in path)
            {
                if (pathPart != "")
                {
                    prefix += pathPart + ".";
                }
            }

            return prefix;
        }
    }
}
