using System.Collections.Concurrent;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IReadingContext
    {
        /// <summary>
        /// Gets the context name.
        /// </summary>
        string Name { get; }

        SimulationExpressionContexts SimulationExpressionContexts { get; }

        /// <summary>
        /// Gets the reading expression context.
        /// </summary>
        ExpressionContext ReadingExpressionContext { get; }

        /// <summary>
        /// Gets the simulation parameters.
        /// </summary>
        ISimulationPreparations SimulationPreparations { get; }

        /// <summary>
        /// Gets the simulation evaluators.
        /// </summary>
        ISimulationEvaluators SimulationEvaluators { get; }

        /// <summary>
        /// Gets the reading evaluator.
        /// </summary>
        IEvaluator ReadingEvaluator { get; }

        /// <summary>
        /// Gets the parent of the context.
        /// </summary>
        IReadingContext Parent { get;  }

        /// <summary>
        /// Gets exporter mapper.
        /// </summary>
        IMapper<Exporter> Exporters { get; }

        /// <summary>
        /// Gets the children of the reading context.
        /// </summary>
        ICollection<IReadingContext> Children { get; }

        /// <summary>
        /// Gets the list of available subcircuit for the context.
        /// </summary>
        ICollection<SubCircuit> AvailableSubcircuits { get; }

        /// <summary>
        /// Gets the result service for the context.
        /// </summary>
        IResultService Result { get; }

        /// <summary>
        /// Gets the node name generator.
        /// </summary>
        INodeNameGenerator NodeNameGenerator { get; }

        /// <summary>
        /// Gets the component name generator.
        /// </summary>
        IObjectNameGenerator ComponentNameGenerator { get; }

        /// <summary>
        /// Gets the model name generator.
        /// </summary>
        IObjectNameGenerator ModelNameGenerator { get; }

        /// <summary>
        /// Gets the stochastic models registry.
        /// </summary>
        IModelsRegistry ModelsRegistry { get; }

        /// <summary>
        /// Gets or sets the statements reader.
        /// </summary>
        ISpiceStatementsReader StatementsReader { get; set; }

        /// <summary>
        /// Gets or sets the waveform reader.
        /// </summary>
        IWaveformReader WaveformReader { get; set; }

        /// <summary>
        /// Gets or sets case-sensitivity settings.
        /// </summary>
        SpiceNetlistCaseSensitivitySettings CaseSensitivity { get; set; }

        /// <summary>
        /// Gets or sets working directory.
        /// </summary>
        string WorkingDirectory { get; }


        InstanceData InstanceData { get; set; }

        ConcurrentDictionary<string, Export> ExporterInstances { get; set; }

        /// <summary>
        /// Parses an expression to double.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <returns>
        /// A value of expression.
        /// </returns>
        double EvaluateDouble(string expression);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        void SetParameter(string parameterName, double value);

        /// <summary>
        /// Sets a parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="valueExpression">Parameter value expression.</param>
        void SetParameter(string parameterName, string valueExpression);

        /// <summary>
        /// Sets parameter of entity to value of expression.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="valueExpression">Value expression.</param>
        /// <param name="beforeTemperature">Should be re-evaluated before temperature.</param>
        /// <param name="onload">Should be re-evaluated OnBeforeLoad.</param>
        void SetParameter(Entity entity, string parameterName, string valueExpression, bool beforeTemperature = true, bool onload = true);

        /// <summary>
        /// Sets the initial voltage.
        /// </summary>
        /// <param name="nodeName">Node name.</param>
        /// <param name="expression">Node voltage expression.</param>
        void SetICVoltage(string nodeName, string expression);

        /// <summary>
        /// Creates nodes for a component.
        /// </summary>
        /// <param name="component">A component</param>
        /// <param name="parameters">Parameters of component</param>
        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);

        /// <summary>
        /// Reads the statements with given order.
        /// </summary>
        /// <param name="statements">Statements.</param>
        /// <param name="orderer">Orderer of statements.</param>
        void Read(Statements statements, ISpiceStatementsOrderer orderer);

        void AddFunction(string functionName, List<string> arguments, string body);

        void SetNamedExpression(string expressionName, string expression);
    }
}
