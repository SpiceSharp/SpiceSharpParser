using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class StochasticModelsRegistry : IStochasticModelsRegistry
    {
        /// <summary>
        /// Gets or sets the dictionary of stochastic models with dev parameters.
        /// </summary>
        protected Dictionary<Entity, Dictionary<Parameter, Parameter>> ModelsWithDev { get; set; } = new Dictionary<Entity, Dictionary<Parameter, Parameter>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models with lot parameters.
        /// </summary>
        protected Dictionary<Entity, Dictionary<Parameter, Parameter>> ModelsWithLot { get; set; } = new Dictionary<Entity, Dictionary<Parameter, Parameter>>();

        /// <summary>
        /// Gets or sets the dictionary of models generators.
        /// </summary>
        protected Dictionary<Entity, Func<string, Entity>> ModelsGenerators { get; set; } = new Dictionary<Entity, Func<string, Entity>>();

        /// <summary>
        /// Gets or sets the dictionary of stochastic models.
        /// </summary>
        protected Dictionary<Entity, List<Entity>> StochasticModels { get; set; } = new Dictionary<Entity, List<Entity>>();

        /// <summary>
        /// Gets or sets the list of all models in the registry.
        /// </summary>
        protected List<Entity> AllModels { get; set; } = new List<Entity>();

        /// <summary>
        /// Gets the object name generators ....
        /// </summary>
        public IEnumerable<IObjectNameGenerator> ModelNamesGenerators { get; }

        public StochasticModelsRegistry(IEnumerable<IObjectNameGenerator> modelNamesGenerators)
        {
            ModelNamesGenerators = modelNamesGenerators;
        }

        /// <summary>
        /// Registers that a model has a dev parameter.
        /// </summary>
        /// <param name="model">A model.</param>
        /// <param name="generator">A model generator.</param>
        /// <param name="parameter">A parameter.</param>
        /// <param name="percent">A percent (value of dev).</param>
        public void RegisterModelDev(Entity model, Func<string, Entity> generator, Parameter parameter, Parameter percent)
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
        public void RegisterModelLot(Entity model, Func<string, Entity> generator, Parameter parameter, Parameter percent)
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
        public Entity ProvideStochasticModel(Entity component, Entity model)
        {
            if (ModelsGenerators.ContainsKey(model))
            {
                var modelForComponent = ModelsGenerators[model](model.Name + "_" + component.Name);

                if (!StochasticModels.ContainsKey(model))
                {
                    StochasticModels[model] = new List<Entity>();
                }

                StochasticModels[model].Add(modelForComponent);
                return modelForComponent;
            }

            return model;
        }

        public T FindBaseModel<T>(string shortName)
            where T : Entity
        {
            foreach (var ong in ModelNamesGenerators)
            {
                var modelNameToSearch = ong.Generate(shortName);

                var model = AllModels.SingleOrDefault(p => p.Name.ToString() == modelNameToSearch);

                if (model != null)
                {
                    return (T)model;
                }
            }

            return null;
        }

        public Dictionary<Entity, List<Entity>> GetStochasticModels()
        {
            return StochasticModels;
        }

        public Dictionary<Parameter, Parameter> GetStochasticModelDevParameters(Entity baseModel)
        {
            return ModelsWithDev[baseModel];
        }

        public void RegisterModel(Entity model)
        {
            AllModels.Add(model);
        }
    }
}
