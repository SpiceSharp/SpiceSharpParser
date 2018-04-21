using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Connector.Processors;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Connector
{
    /// <summary>
    /// Translates a netlist to Spice#
    /// </summary>
    public class Connector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Connector"/> class.
        /// </summary>
        public Connector()
        {
            StatementsProcessor = BuiltInProcessors.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector"/> class.
        /// </summary>
        /// <param name="statementsProcessor">Statements processor</param>
        public Connector(IStatementsProcessor statementsProcessor)
        {
            StatementsProcessor = statementsProcessor ?? throw new System.ArgumentNullException(nameof(statementsProcessor));
        }

        /// <summary>
        /// Gets the statements processor
        /// </summary>
        public IStatementsProcessor StatementsProcessor { get; private set; }

        /// <summary>
        /// Translates Netlist object mode to SpiceSharp netlist
        /// </summary>
        /// <param name="netlist">A object model of the netlist</param>
        /// <returns>
        /// A new SpiceSharp netlist
        /// </returns>
        public ConnectorResult Translate(Model.Netlist netlist)
        {
            // Create result netlist
            var result = new ConnectorResult(new Circuit(), netlist.Title);

            // Create processing context
            var evaluator = new Evaluator();

            var resultService = new ResultService(result);
            var nodeNameGenerator = new NodeNameGenerator(new string[] { "0" });
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);

            var processingContext = new ProcessingContext(
                string.Empty,
                evaluator,
                resultService,
                nodeNameGenerator,
                objectNameGenerator);

            AddUserFunctions(evaluator, processingContext);

            // Process statements form input netlist using created context
            StatementsProcessor.Process(netlist.Statements, processingContext);

            return result;
        }

        /// <summary>
        /// Adds user functions to evaluator
        /// </summary>
        /// <param name="evaluator">Evaluator to set</param>
        /// <param name="context">Processing context</param>
        private void AddUserFunctions(Evaluator evaluator, IProcessingContext context)
        {
            var userFunctions = new Dictionary<string, System.Func<string[], double>>();

            // TODO: Future: add more functions to use
            CreateExportsUserFunctions(context, userFunctions);

            evaluator.ExpressionParser.UserFunctions = userFunctions;
        }

        /// <summary>
        /// Creates export user functions
        /// </summary>
        /// <param name="context">Processing context</param>
        /// <param name="userFunctions">Where to add</param>
        private void CreateExportsUserFunctions(IProcessingContext context, Dictionary<string, System.Func<string[], double>> userFunctions)
        {
            // create exports user functions for each export
            var exporters = new Dictionary<string, Export>();

            foreach (var exporter in StatementsProcessor.ExporterRegistry)
            {
                foreach (var exportType in exporter.GetSupportedTypes())
                {
                    System.Func<string[], double> eval = (args) =>
                    {
                        string exporterKey = exportType + string.Join(",", args);

                        if (!exporters.ContainsKey(exporterKey))
                        {
                            var vectorParameter = new VectorParameter();
                            foreach (var arg in args)
                            {
                                vectorParameter.Elements.Add(new WordParameter(arg));
                            }

                            var parameters = new ParameterCollection();
                            parameters.Add(vectorParameter);
                            var export = exporter.CreateExport(exportType, parameters, context.Result.Simulations.First(), context);
                            exporters[exporterKey] = export;
                        }

                        try
                        {
                            return exporters[exporterKey].Extract();
                        }
                        catch (GeneralConnectorException)
                        {
                            return double.NaN;
                        }
                    };

                    userFunctions.Add(exportType, eval);
                    userFunctions.Add(exportType.ToUpper(), eval);
                }
            }
        }
    }
}
