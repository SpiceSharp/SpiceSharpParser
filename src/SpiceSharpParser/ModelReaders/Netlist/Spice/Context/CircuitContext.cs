using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    /// <summary>
    /// Reading context.
    /// </summary>
    public class CircuitContext : ICircuitContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitContext"/> class.
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
        public CircuitContext(
            string contextName,
            ICircuitContext parent,
            ICircuitEvaluator evaluator,
            ISimulationPreparations simulationPreparations,
            IResultService resultService,
            INameGenerator nameGenerator,
            ISpiceStatementsReader statementsReader,
            IWaveformReader waveformReader,
            ISpiceNetlistCaseSensitivitySettings caseSettings,
            IMapper<Exporter> exporters,
            string workingDirectory)
        {
            Name = contextName ?? throw new ArgumentNullException(nameof(contextName));
            Evaluator = evaluator;
            SimulationPreparations = simulationPreparations;
            Result = resultService ?? throw new ArgumentNullException(nameof(resultService));
            NameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
            Parent = parent;
            Children = new List<ICircuitContext>();
            CaseSensitivity = caseSettings;
            StatementsReader = statementsReader;
            WaveformReader = waveformReader;
            AvailableSubcircuits = CreateAvailableSubcircuitsCollection();
            ModelsRegistry = CreateModelsRegistry();
            Exporters = exporters;
            WorkingDirectory = workingDirectory;
        }

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
        public ICircuitContext Parent { get; }

        /// <summary>
        /// Gets exporter mapper.
        /// </summary>
        public IMapper<Exporter> Exporters { get; }

        /// <summary>
        /// Gets the evaluator.
        /// </summary>
        public ICircuitEvaluator Evaluator { get; }

        /// <summary>
        /// Gets simulation parameters.
        /// </summary>
        public ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets available subcircuits in context.
        /// </summary>
        public ICollection<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the result service.
        /// </summary>
        public IResultService Result { get; }

        /// <summary>
        /// Gets the name generator.
        /// </summary>
        public INameGenerator NameGenerator { get;  }

        /// <summary>
        /// Gets the children of the reading context.
        /// </summary>
        public ICollection<ICircuitContext> Children { get; }

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

            var nodeId = NameGenerator.GenerateNodeName(nodeName);
            SimulationPreparations.SetICVoltage(nodeId, expression);
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
                string pinName = parameters.Get(i).Image;
                nodes[i] = NameGenerator.GenerateNodeName(pinName);
            }

            component.Connect(nodes);
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

        public void SetParameter(IEntity entity, string parameterName, string expression, bool beforeTemperature = true, bool onload = true, Simulation simulation = null)
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

            try
            {
                entity.SetParameter(parameterName, value);
                var context = Evaluator.GetEvaluationContext();

                bool isDynamic = context.HaveSpiceProperties(expression)
                                 || context.HaveFunctions(expression)
                                 || context.GetExpressionParameters(expression, false).Any();

                if (isDynamic)
                {
                    SimulationPreparations.SetParameter(entity, parameterName, expression, beforeTemperature, onload);
                }
            }
            catch (Exception e)
            {
                Result.Validation.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Problem with setting parameter = {parameterName} with value = {value}",
                        exception: e));
            }
        }

        public void SetParameter(IEntity entity, string parameterName, Parameter parameter, bool beforeTemperature = true, bool onload = true)
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
                expression = sp.Image;
            }

            if (parameter is AssignmentParameter asg)
            {
                expression = asg.Value;
            }

            try
            {
                double value = Evaluator.EvaluateDouble(expression);

                entity.SetParameter(parameterName, value);

                var context = Evaluator.GetEvaluationContext();

                bool isDynamic = context.HaveSpiceProperties(expression)
                                 || context.HaveFunctions(expression)
                                 || context.GetExpressionParameters(expression, false).Any();
                if (isDynamic)
                {
                    SimulationPreparations.SetParameter(entity, parameterName, expression, beforeTemperature, onload);
                }
            }
            catch (Exception ex)
            {
                Result.Validation.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Reader,
                        ValidationEntryLevel.Warning,
                        $"Exception during evaluation of parameter with expression: `{expression}`: {ex}",
                        parameter.LineInfo));
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

        protected IModelsRegistry CreateModelsRegistry()
        {
            if (Parent != null)
            {
                var generators = new List<INameGenerator>();
                ICircuitContext current = this;
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