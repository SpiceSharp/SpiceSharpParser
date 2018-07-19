using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IParameterUpdater
    {
        void Update(IReadingContext context, List<KeyValuePair<Parameter, double>> parameterValues, BaseSimulation simulation);
    }
}