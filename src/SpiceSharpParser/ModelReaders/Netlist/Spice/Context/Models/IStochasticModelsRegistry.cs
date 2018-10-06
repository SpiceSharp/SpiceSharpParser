using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IStochasticModelsRegistry : IModelsRegistry
    {
        void RegisterModelDev(Model model, Func<string, Model> generator, Models.Netlist.Spice.Objects.Parameter parameter, Models.Netlist.Spice.Objects.Parameter percent);

        void RegisterModelLot(Model model, Func<string, Model> generator, Models.Netlist.Spice.Objects.Parameter parameter, Models.Netlist.Spice.Objects.Parameter percent);

        Dictionary<Model, List<Model>> GetStochasticModels();

        Dictionary<Models.Netlist.Spice.Objects.Parameter, Models.Netlist.Spice.Objects.Parameter> GetStochasticModelDevParameters(Model baseModel);

        Dictionary<Models.Netlist.Spice.Objects.Parameter, Models.Netlist.Spice.Objects.Parameter> GetStochasticModelLotParameters(Model baseModel);
    }
}