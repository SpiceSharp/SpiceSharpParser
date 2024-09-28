using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using System;
using System.Collections.Generic;

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

        Model ProvideStochasticModel(string componentName, ISimulationWithEvents simulation, Model model);

        Dictionary<string, List<Model>> GetStochasticModels(ISimulationWithEvents simulation);

        Dictionary<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, ParameterRandomness> GetStochasticModelDevParameters(string baseModel);

        Dictionary<SpiceSharpParser.Models.Netlist.Spice.Objects.Parameter, ParameterRandomness> GetStochasticModelLotParameters(string baseModel);
    }
}