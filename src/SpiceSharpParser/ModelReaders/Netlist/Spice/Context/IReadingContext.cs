using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Configurations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using ExpressionParser = SpiceSharpParser.Common.ExpressionParser;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IReadingContext
    {
        SpiceSharpModel Result { get; }

        SimulationConfiguration SimulationConfiguration { get; }

        /// <summary>
        /// Gets the simulation preparations.
        /// </summary>
        ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets the parent of the context.
        /// </summary>
        IReadingContext Parent { get; }

        /// <summary>
        /// Gets exporter mapper.
        /// </summary>
        IMapper<Exporter> Exporters { get; }

        EvaluationContext EvaluationContext { get; }

        /// <summary>
        /// Gets the children of the reading context.
        /// </summary>
        ICollection<IReadingContext> Children { get; }

        /// <summary>
        /// Gets the dictionary of available subcircuit for the context.
        /// </summary>
        Dictionary<string, SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the list of available subcircuit for the context.
        /// </summary>
        Dictionary<string, SubcircuitDefinition> AvailableSubcircuitDefinitions { get; }

        Circuit ContextEntities { get; set; }

        /// <summary>
        /// Gets or set name generator.
        /// </summary>
        INameGenerator NameGenerator { get; }

        /// <summary>
        /// Gets the stochastic models registry.
        /// </summary>
        IModelsRegistry ModelsRegistry { get; }

        /// <summary>
        /// Gets the statements reader.
        /// </summary>
        ISpiceStatementsReader StatementsReader { get; }

        /// <summary>
        /// Gets the waveform reader.
        /// </summary>
        IWaveformReader WaveformReader { get; }

        SpiceNetlistReaderSettings ReaderSettings { get; }

        IEvaluator Evaluator { get; }

        /// <summary>
        /// Sets parameter of entity to value of expression.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="expression">Value expression.</param>
        /// <param name="beforeTemperature">Should be re-evaluated before temperature.</param>
        /// <param name="simulation">Simulation.</param>
        /// <param name="logError">Should log the error.</param>
        void SetParameter(IEntity entity, string parameterName, string expression, bool beforeTemperature = true,  Simulation simulation = null, bool logError = false);

        /// <summary>
        /// Sets parameter of entity to value of expression.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="valueExpression">Value expression.</param>
        /// <param name="beforeTemperature">Should be re-evaluated before temperature.</param>
        /// <param name="simulation">Simulation.</param>
        void SetParameter(IEntity entity, string parameterName, Parameter valueExpression, bool beforeTemperature = true, Simulation simulation = null);

        /// <summary>
        /// Sets the initial voltage.
        /// </summary>
        /// <param name="nodeName">Node name.</param>
        /// <param name="expression">Node voltage expression.</param>
        void SetICVoltage(string nodeName, string expression);

        void SetNodeSetVoltage(string nodeName, string expression);

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component.</param>
        /// <param name="parameters">Parameters of component.</param>
        void CreateNodes(IComponent component, ParameterCollection parameters);

        /// <summary>
        /// Reads the statements with given order.
        /// </summary>
        /// <param name="statements">Statements.</param>
        /// <param name="orderer">Orderer of statements.</param>
        void Read(Statements statements, ISpiceStatementsOrderer orderer);

        bool FindObject(string objectId, out IEntity entity);

        ExpressionParser CreateExpressionParser(Simulation simulation);

        ExpressionResolver CreateExpressionResolver(Simulation simulation);
    }
}