using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
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
        /// <param name="evaluationContext">Evaluation context.</param>
        /// <param name="simulationPreparations">Simulation preparations.</param>
        /// <param name="nameGenerator">Name generator for the models.</param>
        /// <param name="statementsReader">Statements reader.</param>
        /// <param name="waveformReader">Waveform reader.</param>
        /// <param name="exporters">Exporters.</param>
        /// <param name="simulationConfiguration">Simulation configuration.</param>
        /// <param name="result">Result.</param>
        /// <param name="readerSettings">Reader settings.</param>
        public ReadingContext(
            string contextName,
            IReadingContext parent,
            EvaluationContext evaluationContext,
            ISimulationPreparations simulationPreparations,
            INameGenerator nameGenerator,
            ISpiceStatementsReader statementsReader,
            IWaveformReader waveformReader,
            IMapper<Exporter> exporters,
            SimulationConfiguration simulationConfiguration,
            SpiceSharpModel result,
            SpiceNetlistReaderSettings readerSettings)
        {
            Name = contextName ?? throw new ArgumentNullException(nameof(contextName));
            EvaluationContext = evaluationContext;
            SimulationPreparations = simulationPreparations;
            NameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
            Parent = parent;
            Children = new List<IReadingContext>();
            StatementsReader = statementsReader;
            WaveformReader = waveformReader;
            ReaderSettings = readerSettings;
            AvailableSubcircuits = CreateAvailableSubcircuitsCollection();
            AvailableSubcircuitDefinitions = CreateAvailableSubcircuitDefinitions();
            Exporters = exporters;
            ContextEntities = new Circuit(new EntityCollection(StringComparerProvider.Get(readerSettings.CaseSensitivity.IsEntityNamesCaseSensitive)));
            SimulationConfiguration = simulationConfiguration;
            Result = result;

            ModelsRegistry = CreateModelsRegistry();

            EvaluationContext.Seed = readerSettings.Seed;
            EvaluationContext.SetEntities(ContextEntities);
            EvaluationContext.CircuitContext = this;
        }

        /// <summary>
        /// Gets the main configuration.
        /// </summary>
        public SimulationConfiguration SimulationConfiguration { get; }

        public SpiceSharpModel Result { get; }

        public SpiceNetlistReaderSettings ReaderSettings { get; }

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
        /// Gets simulation parameters.
        /// </summary>
        public ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets available subcircuits in context.
        /// </summary>
        public Dictionary<string, SubCircuit> AvailableSubcircuits { get; }

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

        public Circuit ContextEntities { get; set; }

        public EvaluationContext EvaluationContext { get; set; }

        public IEvaluator Evaluator => EvaluationContext.Evaluator;

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
        public void CreateNodes(IComponent component, ParameterCollection parameters)
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
                if (ReaderSettings.ExpandSubcircuits)
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

        public void SetParameter(IEntity entity, string parameterName, string expression, bool beforeTemperature = true, Simulation simulation = null, bool logError = true)
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

            try
            {
                var context = simulation != null
                    ? EvaluationContext.GetSimulationContext(simulation)
                    : EvaluationContext;
                double value = context.Evaluator.EvaluateDouble(expression);
                entity.SetParameter(parameterName, value);

                bool shouldBeUpdatedBeforeTemperature = (context.HaveSpiceProperties(expression)
                                                         || context.HaveFunctions(expression)
                                                         || context.GetExpressionParameters(expression, false).Any())
                                                        && beforeTemperature;

                if (shouldBeUpdatedBeforeTemperature)
                {
                    SimulationPreparations.SetParameterBeforeTemperature(entity, parameterName, expression);
                }
            }
            catch (Exception ex)
            {
                if (logError)
                {
                    Result.ValidationResult.AddError(
                        ValidationEntrySource.Reader,
                        $"Problem with setting parameter {parameterName} for {entity.Name}",
                        null,
                        ex);
                }
                else
                {
                    throw;
                }
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
                SetParameter(entity, parameterName, expression, beforeTemperature, simulation, logError: false);
            }
            catch (Exception e)
            {
                Result.ValidationResult.AddError(
                    ValidationEntrySource.Reader,
                    $"Problem with setting parameter {parameter}",
                    parameter.LineInfo,
                    exception: e);
            }
        }

        public ExpressionParser CreateExpressionParser(Simulation simulation)
        {
            var evalContext = simulation != null
                ? EvaluationContext.GetSimulationContext(simulation)
                : EvaluationContext;

            var variablesFactory = new VariablesFactory();

            var parser = new ExpressionParser(
                new CustomRealBuilder(evalContext, ReaderSettings.CaseSensitivity, false, variablesFactory),
                false);

            return parser;
        }

        public ExpressionResolver CreateExpressionResolver(Simulation simulation)
        {
            var evalContext = simulation != null
                ? EvaluationContext.GetSimulationContext(simulation)
                : EvaluationContext;

            var variablesFactory = new VariablesFactory();

            var parser = new ExpressionResolver(
                new CustomRealBuilder(evalContext,  ReaderSettings.CaseSensitivity, false, variablesFactory),
                EvaluationContext,
                false,
                ReaderSettings.CaseSensitivity,
                variablesFactory);

            return parser;
        }

        protected Dictionary<string, SubCircuit> CreateAvailableSubcircuitsCollection()
        {
            if (Parent != null)
            {
                return new Dictionary<string, SubCircuit>(Parent.AvailableSubcircuits, StringComparerProvider.Get(ReaderSettings.CaseSensitivity.IsSubcircuitNameCaseSensitive));
            }
            else
            {
                return new Dictionary<string, SubCircuit>(StringComparerProvider.Get(ReaderSettings.CaseSensitivity.IsSubcircuitNameCaseSensitive));
            }
        }

        protected Dictionary<string, SubcircuitDefinition> CreateAvailableSubcircuitDefinitions()
        {
            if (Parent != null)
            {
                return new Dictionary<string, SubcircuitDefinition>(Parent.AvailableSubcircuitDefinitions, StringComparerProvider.Get(ReaderSettings.CaseSensitivity.IsSubcircuitNameCaseSensitive));
            }
            else
            {
                return new Dictionary<string, SubcircuitDefinition>(StringComparerProvider.Get(ReaderSettings.CaseSensitivity.IsSubcircuitNameCaseSensitive));
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
                return new StochasticModelsRegistry(generators, ReaderSettings.CaseSensitivity.IsEntityNamesCaseSensitive);
            }
        }
    }
}