using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharp.Entities;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public interface IModelsRegistry
    {
        void SetModel(Entity entity, Simulation simulation, Parameter modelNameParameter, string exceptionMessage, Action<Context.Models.Model> setModelAction, IResultService result);

        Model FindModel(string modelName);

        IEntity FindModelEntity(string modelName);

        void RegisterModelInstance(Model model);

        IModelsRegistry CreateChildRegistry(List<INameGenerator> generators);
    }
}