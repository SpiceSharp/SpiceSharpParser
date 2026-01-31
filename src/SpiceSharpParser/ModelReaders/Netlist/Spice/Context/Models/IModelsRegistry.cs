using System;
using System.Collections.Generic;
using SpiceSharp.Entities;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public interface IModelsRegistry
    {
        void SetModel(Entity entity, double? l, double? w, ISimulationWithEvents simulation, Parameter modelNameParameter, string exceptionMessage, Action<Model> setModelAction, IReadingContext context);

        Model FindModel(string modelName);

        IEntity FindModelEntity(string modelName, double? l, double? w);

        void RegisterModelInstance(Model model);

        IModelsRegistry CreateChildRegistry(List<INameGenerator> generators);
    }
}