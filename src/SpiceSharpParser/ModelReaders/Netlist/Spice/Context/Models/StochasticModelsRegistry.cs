using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class StochasticModelsRegistry : IStochasticModelsRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StochasticModelsRegistry"/> class.
        /// </summary>
        /// <param name="modelNamesGenerators">The enumerable of model name generators.</param>
        public StochasticModelsRegistry(IEnumerable<IObjectNameGenerator> modelNamesGenerators, bool isModelNameCaseSensitive)
        {
            IsModelNameCaseSensitive = isModelNameCaseSensitive;
            ModelNamesGenerators = modelNamesGenerators ?? throw new ArgumentNullException(nameof(modelNamesGenerators));

            AllModels = new Dictionary<string, Entity>(StringComparerProvider.Get(isModelNameCaseSensitive));
        }

        public bool IsModelNameCaseSensitive { get; }

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with dev parameters.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, Parameter>> ModelsWithDev { get; set; } = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, Parameter>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with lot parameters.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, Parameter>> ModelsWithLot { get; set; } = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, Parameter>>();

        /// <summary>
        /// Gets or sets the dictionary of models generators.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, Func<string, SpiceSharp.Components.Model>> ModelsGenerators { get; set; } = new Dictionary<SpiceSharp.Components.Model, Func<string, SpiceSharp.Components.Model>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models.
        /// </summary>
        protected Dictionary<SpiceSharp.Components.Model, List<SpiceSharp.Components.Model>> StochasticModels { get; set; } = new Dictionary<SpiceSharp.Components.Model, List<SpiceSharp.Components.Model>>();

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
        /// <param name="percent">A percent (value of dev).</param>
        public void RegisterModelDev(SpiceSharp.Components.Model model, Func<string, SpiceSharp.Components.Model> generator, Parameter parameter, Parameter percent)
        {
            if (!ModelsWithDev.ContainsKey(model))
            {
                ModelsWithDev[model] = new Dictionary<Parameter, Parameter>();
            }

            ModelsWithDev[model][parameter] = percent;

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
        /// <param name="percent">A percent (value of lot).</param>
        public void RegisterModelLot(SpiceSharp.Components.Model model, Func<string, SpiceSharp.Components.Model> generator, Parameter parameter, Parameter percent)
        {
            if (!ModelsWithLot.ContainsKey(model))
            {
                ModelsWithLot[model] = new Dictionary<Parameter, Parameter>();
            }

            ModelsWithLot[model][parameter] = percent;

            if (!ModelsGenerators.ContainsKey(model))
            {
                ModelsGenerators[model] = generator;
            }
        }

        /// <summary>
        /// Provides a model for component.
        /// </summary>
        /// <param name="component">A component.</param>
        /// <param name="model">A model for component.</param>
        /// <returns>
        /// If a model is stochastic (dev, lot) then a copy of model with be returned.
        /// If a model is not stochastic then a raw model is returned.
        /// </returns>
        public SpiceSharp.Components.Model ProvideStochasticModel(Entity component, SpiceSharp.Components.Model model)
        {
            if (ModelsGenerators.ContainsKey(model))
            {
                string modelId = string.Format("{0}#{1}", model.Name, component.Name);
                var modelForComponent = ModelsGenerators[model](modelId);

                if (!StochasticModels.ContainsKey(model))
                {
                    StochasticModels[model] = new List<SpiceSharp.Components.Model>();
                }

                StochasticModels[model].Add(modelForComponent);
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
        public Dictionary<SpiceSharp.Components.Model, List<SpiceSharp.Components.Model>> GetStochasticModels()
        {
            return StochasticModels;
        }

        /// <summary>
        /// Gets the stochastic model DEV parameters.
        /// </summary>
        /// <param name="baseModel">A base model.</param>
        /// <returns>
        /// A dictionary of DEV parameters and their percent value.
        /// </returns>
        public Dictionary<Parameter, Parameter> GetStochasticModelDevParameters(SpiceSharp.Components.Model baseModel)
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
        /// A dictionary of LOT parameters and their percent value.
        /// </returns>
        public Dictionary<Parameter, Parameter> GetStochasticModelLotParameters(SpiceSharp.Components.Model baseModel)
        {
            if (ModelsWithLot.ContainsKey(baseModel))
            {
                return ModelsWithLot[baseModel];
            }

            return null;
        }

        public void SetModel<T>(Entity entity, string modelName, string exceptionMessage, Action<T> setModelAction)
            where T : SpiceSharp.Components.Model
        {
            var model = FindModel<T>(modelName);

            if (model == null)
            {
                throw new ModelNotFoundException(exceptionMessage);
            }

            setModelAction((T)ProvideStochasticModel(entity, model));
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
            result.ModelsWithDev = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, Parameter>>(ModelsWithDev);
            result.ModelsWithLot = new Dictionary<SpiceSharp.Components.Model, Dictionary<Parameter, Parameter>>(ModelsWithLot);

            return result;
        }
    }
}
