using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Connector.Processors;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Connector.UserFunctions
{
    public class ExportUserFunctions
    {
        /// <summary>
        /// Creates export user functions.
        /// </summary>
        /// <param name="processingContext">Processing context</param>
        /// <param name="userFunctions">Where to add</param>
        public static void Create(IProcessingContext processingContext, Dictionary<string, UserFunction> userFunctions, IStatementsProcessor statementsProcessor)
        {
            // create exports user functions for each export
            var exporters = new Dictionary<string, Processors.Controls.Exporters.Export>();

            foreach (var exporter in statementsProcessor.ExporterRegistry)
            {
                foreach (var exportType in exporter.GetSupportedTypes())
                {
                    UserFunction userFunction = new UserFunction();

                    // @ is a special function for now
                    if (exportType == "@")
                    {
                        userFunction.VirtualParameters = true;
                        userFunction.Name = "Exporter: @";
                        userFunction.Logic = (args, simulation) =>
                        {
                            string exporterKey = exportType + string.Join(",", args);

                            if (!exporters.ContainsKey(exporterKey))
                            {
                                var parameters = new ParameterCollection();
                                parameters.Add(new WordParameter(args[1].ToString()));
                                parameters.Add(new WordParameter(args[0].ToString()));

                                var export = exporter.CreateExport(exportType, parameters, (Simulation)simulation ?? processingContext.Result.Simulations.First(), processingContext);
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
                    }
                    else
                    {
                        userFunction.VirtualParameters = true;
                        userFunction.Name = "Exporter: " + exportType;
                        userFunction.Logic = (args, simulation) =>
                        {
                            string exporterKey = exportType + string.Join(",", args);

                            if (!exporters.ContainsKey(exporterKey))
                            {
                                var vectorParameter = new VectorParameter();
                                foreach (var arg in args)
                                {
                                    vectorParameter.Elements.Add(new WordParameter(arg.ToString()));
                                }

                                var parameters = new ParameterCollection();
                                parameters.Add(vectorParameter);
                                var export = exporter.CreateExport(exportType, parameters, (Simulation)simulation ?? processingContext.Result.Simulations.First(), processingContext);
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
                    }

                    userFunctions.Add(exportType, userFunction);
                    if (exportType != exportType.ToUpper())
                    {
                        userFunctions.Add(exportType.ToUpper(), userFunction);
                    }
                }
            }
        }
    }
}
