using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ICircuitContext
    {
        /// <summary>
        /// Gets the simulation parameters.
        /// </summary>
        ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets the parent of the context.
        /// </summary>
        ICircuitContext Parent { get; }

        /// <summary>
        /// Gets exporter mapper.
        /// </summary>
        IMapper<Exporter> Exporters { get; }

        /// <summary>
        /// Gets or set circuit evaluator.
        /// </summary>
        ICircuitEvaluator Evaluator { get; }

        /// <summary>
        /// Gets the children of the reading context.
        /// </summary>
        ICollection<ICircuitContext> Children { get; }

        /// <summary>
        /// Gets the list of available subcircuit for the context.
        /// </summary>
        ICollection<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the result service for the context.
        /// </summary>
        IResultService Result { get; }

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

        /// <summary>
        /// Gets case-sensitivity settings.
        /// </summary>
        ISpiceNetlistCaseSensitivitySettings CaseSensitivity { get; }

        /// <summary>
        /// Gets working directory.
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Sets parameter of entity to value of expression.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="expression">Value expression.</param>
        /// <param name="beforeTemperature">Should be re-evaluated before temperature.</param>
        /// <param name="onload">Should be re-evaluated OnBeforeLoad.</param>
        void SetParameter(IEntity entity, string parameterName, string expression, bool beforeTemperature = true, bool onload = true, Simulation simulation = null);

        /// <summary>
        /// Sets parameter of entity to value of expression.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="valueExpression">Value expression.</param>
        /// <param name="beforeTemperature">Should be re-evaluated before temperature.</param>
        /// <param name="onload">Should be re-evaluated OnBeforeLoad.</param>
        void SetParameter(IEntity entity, string parameterName, Parameter valueExpression, bool beforeTemperature = true, bool onload = true);

        /// <summary>
        /// Sets the initial voltage.
        /// </summary>
        /// <param name="nodeName">Node name.</param>
        /// <param name="expression">Node voltage expression.</param>
        void SetICVoltage(string nodeName, string expression);

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component.</param>
        /// <param name="parameters">Parameters of component.</param>
        void CreateNodes(SpiceSharp.Components.IComponent component, ParameterCollection parameters);

        /// <summary>
        /// Reads the statements with given order.
        /// </summary>
        /// <param name="statements">Statements.</param>
        /// <param name="orderer">Orderer of statements.</param>
        void Read(Statements statements, ISpiceStatementsOrderer orderer);
    }
}