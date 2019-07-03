using System;
using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public interface IStochasticModelsRegistry : IModelsRegistry
    {
        void RegisterModelDev(
            Model model,
            Func<string, Model> generator,
            SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter parameter,
            SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter tolerance,
            string distributionName);

        void RegisterModelLot(
            Model model,
            Func<string, Model> generator,
            SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter parameter,
            SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter tolerance,
            string distributionName);

        Dictionary<Model, List<Model>> GetStochasticModels();

        Dictionary<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, ParameterRandomness> GetStochasticModelDevParameters(Model baseModel);

        Dictionary<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, ParameterRandomness> GetStochasticModelLotParameters(Model baseModel);
    }
}