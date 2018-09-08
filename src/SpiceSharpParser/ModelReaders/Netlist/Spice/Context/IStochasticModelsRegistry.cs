using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IStochasticModelsRegistry : IModelsRegistry
    {
        Entity ProvideStochasticModel(Entity component, Entity model);

        void RegisterModel(Entity model);

        void RegisterModelDev(Entity model, Func<string, Entity> generator, Parameter parameter, Parameter percent);

        void RegisterModelLot(Entity model, Func<string, Entity> generator, Parameter parameter, Parameter percent);

        Dictionary<Entity, List<Entity>> GetStochasticModels();

        Dictionary<Parameter, Parameter> GetStochasticModelDevParameters(Entity baseModel);

        Dictionary<Parameter, Parameter> GetStochasticModelLotParameters(Entity baseModel);
    }
}