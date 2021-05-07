using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;
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
        /// <param name="parent">Parent of the context.</param>
        /// <param name="evaluator">Circuit evaluator.</param>
        /// <param name="simulationPreparations">Simulation preparations.</param>
        /// <param name="resultService">SpiceModel service for the context.</param>
        /// <param name="nameGenerator">Name generator for the models.</param>
        /// <param name="statementsReader">Statements reader.</param>
        /// <param name="waveformReader">Waveform reader.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <param name="exporters">Exporters.</param>
        /// <param name="workingDirectory">Working directory.</param>
        /// <param name="instanceData">Instance data.</param>
        public ReadingContext(
            string contextName,
            IReadingContext parent,
            IEvaluator evaluator,
            ISimulationPreparations simulationPreparations,
            INameGenerator nameGenerator,
            ISpiceStatementsReader statementsReader,
            IWaveformReader waveformReader,
            ISpiceNetlistCaseSensitivitySettings caseSettings,
            IMapper<Exporter> exporters,
            string workingDirectory,
            bool expandSubcircuits,
            SimulationConfiguration simulationConfiguration,
            SpiceModel<Circuit, Simulation> result,
            string separator)
        {
            Name = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Evaluator = evaluator;
            SimulationPreparations = simulationPreparations;
            NameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
            Parent = parent;
            Children = new List<IReadingContext>();
            CaseSensitivity = caseSettings;
            StatementsReader = statementsReader;
            WaveformReader = waveformReader;
            AvailableSubcircuits = CreateAvailableSubcircuitsCollection();
            AvailableSubcircuitDefinitions = CreateAvailableSubcircuitDefinitions();
            ModelsRegistry = CreateModelsRegistry();
            Exporters = exporters;
            WorkingDirectory = workingDirectory;
            ContextEntities = new Circuit(new EntityCollection(StringComparerProvider.Get(caseSettings.IsEntityNamesCaseSensitive)));
            ExpandSubcircuits = expandSubcircuits;
            SimulationConfiguration = simulationConfiguration;
            Result = result;
            Separator = separator;
        }

        /// <summary>
        /// Gets the main configuration.
        /// </summary>
        public SimulationConfiguration SimulationConfiguration { get; }

        public SpiceModel<Circuit, Simulation> Result { get; }

        public string Separator { get; }

        /// <summary>
        /// Gets the working directory.
        /// </summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Gets the name of context.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parent of context.
        /// </summary>
        public IReadingContext Parent { get; }

        /// <summary>
        /// Gets exporter mapper.
        /// </summary>
        public IMapper<Exporter> Exporters { get; }

        /// <summary>
        /// Gets the evaluator.
        /// </summary>
        public IEvaluator Evaluator { get; }

        /// <summary>
        /// Gets simulation parameters.
        /// </summary>
        public ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets available subcircuits in context.
        /// </summary>
        public ICollection<SubCircuit> AvailableSubcircuits { get; }

        public Dictionary<string, SubcircuitDefinition> AvailableSubcircuitDefinitions { get; }

        /// <summary>
        /// Gets the name generator.
        /// </summary>
        public INameGenerator NameGenerator { get;  }

        /// <summary>
        /// Gets the children of the reading context.
        /// </summary>
        public ICollection<IReadingContext> Children { get; }

        /// <summary>
        /// Gets the stochastic models registry.
        /// </summary>
        public IModelsRegistry ModelsRegistry { get; }

        /// <summary>
        /// Gets statements reader.
        /// </summary>
        public ISpiceStatementsReader StatementsReader { get; }

        /// <summary>
        /// Gets waveform reader.
        /// </summary>
        public IWaveformReader WaveformReader { get;  }

        /// <summary>
        /// Gets the case sensitivity settings.
        /// </summary>
        public ISpiceNetlistCaseSensitivitySettings CaseSensitivity { get; }

        public Circuit ContextEntities { get; set; }

        public bool ExpandSubcircuits { get; }

        /// <summary>
        /// Sets voltage initial condition for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression string.</param>
        public void SetICVoltage(string nodeName, string expression)
        {
            if (nodeName == null)
            {
                throw new ArgumentNullException(nameof(nodeName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var nodeId = NameGenerator.ParseNodeName(nodeName);
            SimulationPreparations.SetICVoltage(nodeId, expression);
        }

        /// <summary>
        /// Sets node set voltage for node.
        /// </summary>
        /// <param name="nodeName">Name of node.</param>
        /// <param name="expression">Expression string.</param>
        public void SetNodeSetVoltage(string nodeName, string expression)
        {
            if (nodeName == null)
            {
                throw new ArgumentNullException(nameof(nodeName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var nodeId = NameGenerator.ParseNodeName(nodeName);
            SimulationPreparations.SetNodeSetVoltage(nodeId, expression);
        }

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component.</param>
        /// <param name="parameters">Parameters of component.</param>
        public void CreateNodes(SpiceSharp.Components.IComponent component, ParameterCollection parameters)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.Count < component.Nodes.Count)
            {
                throw new SpiceSharpParserException($"Too few parameters for: {component.Name} to create nodes", parameters.LineInfo);
            }

            string[] nodes = new string[component.Nodes.Count];
            for (var i = 0; i < component.Nodes.Count; i++)
            {
                string pinName = parameters.Get(i).Value;
                if (ExpandSubcircuits)
                {
                    nodes[i] = NameGenerator.GenerateNodeName(pinName);
                }
                else
                {
                    nodes[i] = pinName;
                }
            }

            component.Connect(nodes);
        }

        /// <summary>
        /// Finds the object in the result.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>
        /// True if found.
        /// </returns>
        public bool FindObject(string objectId, out IEntity entity)
        {
            if (objectId == null)
            {
                throw new ArgumentNullException(nameof(objectId));
            }

            return ContextEntities.TryGetEntity(objectId, out entity);
        }

        public virtual void Read(Statements statements, ISpiceStatementsOrderer orderer)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            if (orderer == null)
            {
                throw new ArgumentNullException(nameof(orderer));
            }

            var orderedStatements = orderer.Order(statements);
            foreach (var statement in orderedStatements)
            {
                StatementsReader.Read(statement, this);
            }
        }

        public void SetParameter(IEntity entity, string parameterName, string expression, bool beforeTemperature = true, Simulation simulation = null)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }


            double value = Evaluator.EvaluateDouble(expression, simulation);

            entity.SetParameter(parameterName, value);
            var context = Evaluator.GetEvaluationContext(simulation);

            bool shouldBeUpdatedBeforeTemperature = (context.HaveSpiceProperties(expression)
                             || context.HaveFunctions(expression)
                             || context.GetExpressionParameters(expression, false).Any())
                             && beforeTemperature;

            if (shouldBeUpdatedBeforeTemperature)
            {
                SimulationPreparations.SetParameterBeforeTemperature(entity, parameterName, expression);
            }
        }

        public void SetParameter(IEntity entity, string parameterName, Parameter parameter, bool beforeTemperature = true, Simulation simulation = null)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            string expression = null;

            if (parameter is SingleParameter sp)
            {
                expression = sp.Value;
            }

            if (parameter is AssignmentParameter asg)
            {
                expression = asg.Value;
            }

            try
            {
                SetParameter(entity, parameterName, expression, beforeTemperature, simulation);

            }
            catch (Exception e)
            {
                Result.ValidationResult.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Problem with setting parameter {parameter}",
                        parameter.LineInfo,
                        exception: e));
            }
        }

        protected ICollection<SubCircuit> CreateAvailableSubcircuitsCollection()
        {
            if (Parent != null)
            {
                return new List<SubCircuit>(Parent.AvailableSubcircuits);
            }
            else
            {
                return new List<SubCircuit>();
            }
        }

        protected Dictionary<string, SubcircuitDefinition> CreateAvailableSubcircuitDefinitions()
        {
            if (Parent != null)
            {
                return new Dictionary<string, SubcircuitDefinition>(Parent.AvailableSubcircuitDefinitions);
            }
            else
            {
                return new Dictionary<string, SubcircuitDefinition>();
            }
        }

        protected IModelsRegistry CreateModelsRegistry()
        {
            if (Parent != null)
            {
                var generators = new List<INameGenerator>();
                IReadingContext current = this;
                while (current != null)
                {
                    generators.Add(current.NameGenerator);
                    current = current.Parent;
                }

                return Parent.ModelsRegistry.CreateChildRegistry(generators);
            }
            else
            {
                var generators = new List<INameGenerator>();
                generators.Add(NameGenerator);
                return new StochasticModelsRegistry(generators, CaseSensitivity.IsEntityNamesCaseSensitive);
            }
        }
    }
}