using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Model = SpiceSharp.Components.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class StochasticModelsRegistry : IStochasticModelsRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticModelsRegistry"/> class.
        /// </summary>
        /// <param name="modelNamesGenerators">The enumerable of model name generators.</param>
        /// <param name="isModelNameCaseSensitive">Is model names case sensitive.</param>
        public StochasticModelsRegistry(IEnumerable<IObjectNameGenerator> modelNamesGenerators, bool isModelNameCaseSensitive)
        {
            ModelNamesGenerators = modelNamesGenerators ?? throw new ArgumentNullException(nameof(modelNamesGenerators));
            IsModelNameCaseSensitive = isModelNameCaseSensitive;

            AllModels = new Dictionary<string, Entity>(StringComparerProvider.Get(isModelNameCaseSensitive));
        }

        public bool IsModelNameCaseSensitive { get; }

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with dev parameters.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, ParameterRandomness>> ModelsWithDev { get; set; } = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, ParameterRandomness>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with lot parameters.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, ParameterRandomness>> ModelsWithLot { get; set; } = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, ParameterRandomness>>();

        /// <summary>
        /// Gets or sets the dictionary of models generators.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, Func<string, SpiceSharp.Components.Model>> ModelsGenerators { get; set; } = new Dictionary<SpiceSharp.Components.Model, Func<string, SpiceSharp.Components.Model>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models.
        /// </summary>
        protected Dictionary<BaseSimulation, Dictionary<SpiceSharp.Components.Model, List<SpiceSharp.Components.Model>>> StochasticModels { get; set; } = new Dictionary<BaseSimulation, Dictionary<Model, List<Model>>>();

        /// <summary>
        /// Gets or sets the list of all models in the registry.
        /// </summary>
        protected Dictionary<string, Entity> AllModels { get; set; }

        /// <summary>
        /// Gets the object model name generators.
        /// </summary>
        protected IEnumerable<IObjectNameGenerator> ModelNamesGenerators { get; }

        /// <summary>
        /// Registers that a model has a dev parameter.
        /// </summary>
        /// <param name="model">A model.</param>
        /// <param name="generator">A model generator.</param>
        /// <param name="parameter">A parameter.</param>
        /// <param name="tolerance">A tolerance (value of dev).</param>
        /// <param name="distribution">Distribution name.</param>
        public void RegisterModelDev(
            SpiceSharp.Components.Model model,
            Func<string, SpiceSharp.Components.Model> generator,
            Parameter parameter,
            Parameter tolerance,
            string distribution)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (tolerance == null)
            {
                throw new ArgumentNullException(nameof(tolerance));
            }

            if (!ModelsWithDev.ContainsKey(model))
            {
                ModelsWithDev[model] = new Dictionary<Parameter, ParameterRandomness>();
            }

            ModelsWithDev[model][parameter] = new ParameterRandomness
            {
                RandomDistributionName = distribution,
                Tolerance = tolerance,
            };

            if (!ModelsGenerators.ContainsKey(model))
            {
                ModelsGenerators[model] = generator;
            }
        }

        /// <summary>
        /// Registers that a model has a lot parameter.
        /// </summary>
        /// <param name="model">A model.</param>
        /// <param name="generator">A model generator.</param>
        /// <param name="parameter">A parameter.</param>
        /// <param name="tolerance">A tolerance (value of lot).</param>
        /// <param name="distributionName">Distribution name.</param>
        public void RegisterModelLot(
            SpiceSharp.Components.Model model, 
            Func<string, SpiceSharp.Components.Model> generator, 
            Parameter parameter, 
            Parameter tolerance,
            string distributionName)
        {
            if (!ModelsWithLot.ContainsKey(model))
            {
                ModelsWithLot[model] = new Dictionary<Parameter, ParameterRandomness>();
            }

            ModelsWithLot[model][parameter] = new ParameterRandomness
            {
                RandomDistributionName = distributionName,
                Tolerance = tolerance,
            };

            if (!ModelsGenerators.ContainsKey(model))
            {
                ModelsGenerators[model] = generator;
            }
        }

        /// <summary>
        /// Provides a model for component.
        /// </summary>
        /// <param name="componentName">A component name.</param>
        /// <param name="model">A model for component.</param>
        /// <returns>
        /// If a model is stochastic (dev, lot) then a copy of model with be returned.
        /// If a model is not stochastic then a raw model is returned.
        /// </returns>
        public Model ProvideStochasticModel(string componentName, BaseSimulation simulation, SpiceSharp.Components.Model model)
        {
            if (ModelsGenerators.ContainsKey(model))
            {
                string modelId = string.Format("{0}#{1}_{2}", model.Name, componentName, simulation.Name);
                var modelForComponent = ModelsGenerators[model](modelId);

                if (!StochasticModels.ContainsKey(simulation))
                {
                    StochasticModels[simulation] = new Dictionary<Model, List<Model>>();
                }

                if (!StochasticModels[simulation].ContainsKey(model))
                {
                    StochasticModels[simulation][model] = new List<SpiceSharp.Components.Model>();
                }

                StochasticModels[simulation][model].Add(modelForComponent);
                return modelForComponent;
            }

            return model;
        }

        /// <summary>
        /// Gets the stochastic models.
        /// </summary>
        /// <returns>
        /// A dictionary of base models and their stochastic models.
        /// </returns>
        public Dictionary<SpiceSharp.Components.Model, List<SpiceSharp.Components.Model>> GetStochasticModels(BaseSimulation simulation)
        {
            return StochasticModels.ContainsKey(simulation)
                ? StochasticModels[simulation]
                : new Dictionary<Model, List<Model>>();
        }

        /// <summary>
        /// Gets the stochastic model DEV parameters.
        /// </summary>
        /// <param name="baseModel">A base model.</param>
        /// <returns>
        /// A dictionary of DEV parameters and their tolerance value.
        /// </returns>
        public Dictionary<Parameter, ParameterRandomness> GetStochasticModelDevParameters(SpiceSharp.Components.Model baseModel)
        {
            if (ModelsWithDev.ContainsKey(baseModel))
            {
                return ModelsWithDev[baseModel];
            }

            return null;
        }

        /// <summary>
        /// Registers a model in the registry.
        /// </summary>
        /// <param name="model">A model to register.</param>
        public void RegisterModelInstance(SpiceSharp.Components.Model model)
        {
            AllModels[model.Name] = model;
        }

        /// <summary>
        /// Gets the stochastic model LOT parameters.
        /// </summary>
        /// <param name="baseModel">A base model.</param>
        /// <returns>
        /// A dictionary of LOT parameters and their tolerance value.
        /// </returns>
        public Dictionary<Parameter, ParameterRandomness> GetStochasticModelLotParameters(SpiceSharp.Components.Model baseModel)
        {
            if (ModelsWithLot.ContainsKey(baseModel))
            {
                return ModelsWithLot[baseModel];
            }

            return null;
        }

        public void SetModel<T>(Entity entity, BaseSimulation simulation, string modelName, string exceptionMessage, Action<T> setModelAction, IResultService result)
            where T : SpiceSharp.Components.Model
        {
            var model = FindModel<T>(modelName);

            if (model == null)
            {
                throw new ModelNotFoundException(exceptionMessage);
            }

            var stochasticModel = (T) ProvideStochasticModel(entity.Name, simulation, model);
            setModelAction(stochasticModel);

            if (stochasticModel != null)
            {
                if (!result.Circuit.Contains(stochasticModel.Name))
                {
                    result.Circuit.Add(stochasticModel);
                }
            }
        }

        public T FindModel<T>(string modelName)
            where T : SpiceSharp.Components.Model
        {
            foreach (var generator in ModelNamesGenerators)
            {
                var modelNameToSearch = generator.Generate(modelName);

                if (AllModels.TryGetValue(modelNameToSearch, out var model))
                {
                    return (T)model;
                }
            }

            return null;
        }

        public IModelsRegistry CreateChildRegistry(List<IObjectNameGenerator> generators)
        {
            var result = new StochasticModelsRegistry(generators, IsModelNameCaseSensitive);

            result.AllModels = new Dictionary<string, Entity>(AllModels, StringComparerProvider.Get(IsModelNameCaseSensitive));
            result.ModelsWithDev = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, ParameterRandomness>>(ModelsWithDev);
            result.ModelsWithLot = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, ParameterRandomness>>(ModelsWithLot);
            result.StochasticModels = new Dictionary<BaseSimulation, Dictionary<Model, List<Model>>>(StochasticModels);
            return result;
        }
    }
}
