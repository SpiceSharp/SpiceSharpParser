using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public interface IModelsRegistry
    {
        void SetModel<T>(Entity entity, BaseSimulation simulation, string modelName, string exceptionMessage, Action<T> setModelAction, IResultService result)
            where T : Model;

        T FindModel<T>(string modelName)
            where T : Model;

        void RegisterModelInstance(Model model);

        IModelsRegistry CreateChildRegistry(List<INameGenerator> generators);
    }
}