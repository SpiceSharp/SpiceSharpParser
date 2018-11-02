using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IModelsRegistry
    {
        void SetModel<T>(Entity entity, string modelName, string exceptionMessage, Action<T> setModelAction) where T : Model;

        T FindModel<T>(string modelName) where T : SpiceSharp.Components.Model;

        void RegisterModelInstance(Model model);
    }
}
