using System;
using SpiceSharp;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

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
        public SpiceSharp.Components.Model GenerateModel(IModelGenerator modelGenerator, string id, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (!(context.ModelsRegistry is IStochasticModelsRegistry stochasticModelRegistry))
            {
                throw new Exception();
            }

            var filteredParameters = FilterDevAndLot(parameters);
            var model = modelGenerator.Generate(id, type, parameters, context);
            if (model == null)
            {
                throw new GeneralReaderException("Couldn't generate model");
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
                if (parameters[i].Image.ToUpper() == "DEV")
                {
                    stochasticModelRegistry.RegisterModelDev(model, generator, parameters[i - 1], parameters[i + 1]);
                    i++;
                }
                else if (parameters[i].Image.ToUpper() == "LOT")
                {
                    stochasticModelRegistry.RegisterModelLot(model, generator, parameters[i - 1], parameters[i + 1]);
                    i++;
                }
            }
        }

        private static ParameterCollection FilterDevAndLot(ParameterCollection parameters)
        {
            var filteredParameters = new ParameterCollection();

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Image.ToUpper() == "DEV")
                {
                    i++;
                }
                else if (parameters[i].Image.ToUpper() == "LOT")
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
