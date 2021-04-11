using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class StochasticModelsRegistry : IStochasticModelsRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticModelsRegistry"/> class.
        /// </summary>
        /// <param name="modelNamesGenerators">The enumerable of model name generators.</param>
        /// <param name="isModelNameCaseSensitive">Is model names case sensitive.</param>
        public StochasticModelsRegistry(IEnumerable<INameGenerator> modelNamesGenerators, bool isModelNameCaseSensitive)
        {
            NamesGenerators = modelNamesGenerators ?? throw new ArgumentNullException(nameof(modelNamesGenerators));
            IsModelNameCaseSensitive = isModelNameCaseSensitive;

            AllModels = new Dictionary<string, Model>(StringComparerProvider.Get(isModelNameCaseSensitive));
        }

        public bool IsModelNameCaseSensitive { get; }

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with dev parameters.
        /// </summary>
        protected Dictionary<string, Dictionary<Parameter, ParameterRandomness>> ModelsWithDev { get; set; } = new Dictionary<string, Dictionary<Parameter, ParameterRandomness>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with lot parameters.
        /// </summary>
        protected Dictionary<string, Dictionary<Parameter, ParameterRandomness>> ModelsWithLot { get; set; } = new Dictionary<string, Dictionary<Parameter, ParameterRandomness>>();

        /// <summary>
        /// Gets or sets the dictionary of models generators.
        /// </summary>
        protected Dictionary<string, Func<string, Model>> ModelsGenerators { get; set; } = new Dictionary<string, Func<string, Model>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models.
        /// </summary>
        protected Dictionary<Simulation, Dictionary<string, List<Model>>> StochasticModels { get; set; } = new Dictionary<Simulation, Dictionary<string, List<Model>>>();

        /// <summary>
        /// Gets or sets the list of all models in the registry.
        /// </summary>
        protected Dictionary<string, Model> AllModels { get; set; }

        /// <summary>
        /// Gets the object model name generators.
        /// </summary>
        protected IEnumerable<INameGenerator> NamesGenerators { get; }

        /// <summary>
        /// Registers that a model has a dev parameter.
        /// </summary>
        /// <param name="model">A model.</param>
        /// <param name="generator">A model generator.</param>
        /// <param name="parameter">A parameter.</param>
        /// <param name="tolerance">A tolerance (value of dev).</param>
        /// <param name="distributionName">Distribution name.</param>
        public void RegisterModelDev(
            Model model,
            Func<string, Model> generator,
            Parameter parameter,
            Parameter tolerance,
            string distributionName)
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

            if (!ModelsWithDev.ContainsKey(model.Name))
            {
                ModelsWithDev[model.Name] = new Dictionary<Parameter, ParameterRandomness>();
            }

            ModelsWithDev[model.Name][parameter] = new ParameterRandomness
            {
                RandomDistributionName = distributionName,
                Tolerance = tolerance,
            };

            if (!ModelsGenerators.ContainsKey(model.Name))
            {
                ModelsGenerators[model.Name] = generator;
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
            Model model,
            Func<string, Model> generator,
            Parameter parameter,
            Parameter tolerance,
            string distributionName)
        {
            if (!ModelsWithLot.ContainsKey(model.Name))
            {
                ModelsWithLot[model.Name] = new Dictionary<Parameter, ParameterRandomness>();
            }

            ModelsWithLot[model.Name][parameter] = new ParameterRandomness
            {
                RandomDistributionName = distributionName,
                Tolerance = tolerance,
            };

            if (!ModelsGenerators.ContainsKey(model.Name))
            {
                ModelsGenerators[model.Name] = generator;
            }
        }

        /// <summary>
        /// Provides a model for component.
        /// </summary>
        /// <param name="componentName">A component name.</param>
        /// <param name="simulation">Simulation.</param>
        /// <param name="model">A model for component.</param>
        /// <returns>
        /// If a model is stochastic (dev, lot) then a copy of model with be returned.
        /// If a model is not stochastic then a raw model is returned.
        /// </returns>
        public Model ProvideStochasticModel(string componentName, Simulation simulation, Model model)
        {
            if (ModelsGenerators.Any(m => m.Key == model.Name))
            {
                var modelForComponentGenerator = ModelsGenerators.First(m => m.Key == model.Name);
                string modelId = $"{model.Name}#{componentName}_{simulation.Name}";

                var modelForComponent = modelForComponentGenerator.Value(modelId);

                if (!StochasticModels.ContainsKey(simulation))
                {
                    StochasticModels[simulation] = new Dictionary<string, List<Model>>();
                }

                if (!StochasticModels[simulation].ContainsKey(model.Name))
                {
                    StochasticModels[simulation][model.Name] = new List<Model>();
                }

                StochasticModels[simulation][model.Name].Add(modelForComponent);
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
        public Dictionary<string, List<Model>> GetStochasticModels(Simulation simulation)
        {
            return StochasticModels.ContainsKey(simulation)
                ? StochasticModels[simulation]
                : new Dictionary<string, List<Model>>();
        }

        /// <summary>
        /// Gets the stochastic model DEV parameters.
        /// </summary>
        /// <param name="baseModel">A base model.</param>
        /// <returns>
        /// A dictionary of DEV parameters and their tolerance value.
        /// </returns>
        public Dictionary<Parameter, ParameterRandomness> GetStochasticModelDevParameters(string baseModel)
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
        public void RegisterModelInstance(Model model)
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
        public Dictionary<Parameter, ParameterRandomness> GetStochasticModelLotParameters(string baseModel)
        {
            if (ModelsWithLot.ContainsKey(baseModel))
            {
                return ModelsWithLot[baseModel];
            }

            return null;
        }

        public void SetModel(Entity entity, Simulation simulation, Parameter modelNameParameter, string exceptionMessage, Action<Context.Models.Model> setModelAction, IResultService result)
        {
            var model = FindModelEntity(modelNameParameter.Image);

            if (model == null)
            {
                result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader, ValidationEntryLevel.Warning, exceptionMessage, modelNameParameter.LineInfo));
                return;
            }

            var stochasticModel = ProvideStochasticModel(entity.Name, simulation, new Model(model.Name, model, model.ParameterSets.First()));
            setModelAction(stochasticModel);

            if (stochasticModel != null)
            {
                if (!result.Circuit.Contains(stochasticModel.Name))
                {
                    result.Circuit.Add(stochasticModel.Entity);
                }
            }
        }

        public Model FindModel(string modelName)
        {
            foreach (var generator in NamesGenerators)
            {
                var modelNameToSearch = generator.GenerateObjectName(modelName);

                if (AllModels.TryGetValue(modelNameToSearch, out var model))
                {
                    return model;
                }
            }

            return null;
        }

        public IEntity FindModelEntity(string modelName)
        {
            foreach (var generator in NamesGenerators)
            {
                var modelNameToSearch = generator.GenerateObjectName(modelName);

                if (AllModels.TryGetValue(modelNameToSearch, out var model))
                {
                    return model.Entity;
                }
            }

            return null;
        }

        public IModelsRegistry CreateChildRegistry(List<INameGenerator> generators)
        {
            var result = new StochasticModelsRegistry(generators, IsModelNameCaseSensitive)
            {
                AllModels = new Dictionary<string, Model>(AllModels, StringComparerProvider.Get(IsModelNameCaseSensitive)),
                ModelsWithDev = new Dictionary<string, Dictionary<Parameter, ParameterRandomness>>(ModelsWithDev),
                ModelsWithLot = new Dictionary<string, Dictionary<Parameter, ParameterRandomness>>(ModelsWithLot),
                StochasticModels = new Dictionary<Simulation, Dictionary<string, List<Model>>>(StochasticModels),
            };

            return result;
        }
    }
}