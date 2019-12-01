using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Model = SpiceSharp.Components.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public interface IModelsRegistry
    {
        void SetModel<T>(Entity entity, BaseSimulation simulation, Parameter modelNameParameter, string exceptionMessage, Action<T> setModelAction, IResultService result)
            where T : Model;

        T FindModel<T>(string modelName)
            where T : Model;

        void RegisterModelInstance(Model model);

        IModelsRegistry CreateChildRegistry(List<INameGenerator> generators);
    }
}