using System;
using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public interface IStochasticModelsRegistry : IModelsRegistry
    {
        void RegisterModelDev(Model model, Func<string, Model> generator, SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter parameter, SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter percent);

        void RegisterModelLot(Model model, Func<string, Model> generator, SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter parameter, SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter percent);

        Dictionary<Model, List<Model>> GetStochasticModels();

        Dictionary<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter> GetStochasticModelDevParameters(Model baseModel);

        Dictionary<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter> GetStochasticModelLotParameters(Model baseModel);
    }
}