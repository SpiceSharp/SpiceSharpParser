using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    /// <summary>
    /// Stochastic models generator.
    /// </summary>
    public class StochasticModelsGenerator : IModelsGenerator
    {
        /// <summary>
        /// Generates entity.
        /// </summary>
        /// <param name="modelGenerator">The model generator.</param>
        /// <param name="id">The identifier for identity.</param>
        /// <param name="originalName">Original name of entity.</param>
        /// <param name="type">The type of entity.</param>
        /// <param name="parameters">Parameters for entity.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of entity.
        /// </returns>
        public SpiceSharp.Components.Model GenerateModel(IModelGenerator modelGenerator, string id, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            if (modelGenerator == null)
            {
                throw new ArgumentNullException(nameof(modelGenerator));
            }

            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (originalName == null)
            {
                throw new ArgumentNullException(nameof(originalName));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!(context.ModelsRegistry is IStochasticModelsRegistry stochasticModelRegistry))
            {
                throw new ReadingException("Model registry is not stochastic models registry");
            }

            var filteredParameters = FilterDevAndLot(parameters);
            var model = modelGenerator.Generate(id, type, filteredParameters, context);
            if (model == null)
            {
                throw new ReadingException($"Couldn't generate model {id}", parameters.LineInfo);
            }

            context.ModelsRegistry.RegisterModelInstance(model);

            RegisterDevAndLotModels(parameters, stochasticModelRegistry, model, (modelId) =>
            {
                var stochasticCandidate = modelGenerator.Generate(modelId, type, filteredParameters, context);
                context.ModelsRegistry.RegisterModelInstance(stochasticCandidate);
                return stochasticCandidate;
            });
            return model;
        }

        private static void RegisterDevAndLotModels(ParameterCollection parameters, IStochasticModelsRegistry stochasticModelRegistry, SpiceSharp.Components.Model model, Func<string, SpiceSharp.Components.Model> generator)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i].Image;
                string distribution = null;

                if (parameter.Contains("/"))
                {
                    var parts = parameter.Split('/');
                    parameter = parts[0].ToUpper();
                    distribution = parts[1];
                }
                else
                {
                    parameter = parameter.ToUpper();
                }

                if (parameter == "DEV")
                {
                    stochasticModelRegistry.RegisterModelDev(
                        model,
                        generator,
                        parameters[i - 1],
                        parameters[i + 1],
                        distribution);

                    i++;
                }
                else if (parameter == "LOT")
                {
                    stochasticModelRegistry.RegisterModelLot(
                        model,
                        generator,
                        parameters[i - 1],
                        parameters[i + 1],
                        distribution);
                    i++;
                }
            }
        }

        private static ParameterCollection FilterDevAndLot(ParameterCollection parameters)
        {
            var filteredParameters = new ParameterCollection();

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i].Image.ToUpper();

                if (parameter.Contains("DEV"))
                {
                    i++;
                }
                else if (parameter.Contains("LOT"))
                {
                    i++;
                }
                else
                {
                    filteredParameters.Add(parameters[i]);
                }
            }

            return filteredParameters;
        }
    }
}