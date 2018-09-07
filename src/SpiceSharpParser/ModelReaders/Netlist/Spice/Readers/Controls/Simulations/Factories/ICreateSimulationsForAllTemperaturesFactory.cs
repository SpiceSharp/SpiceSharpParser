using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public interface ICreateSimulationsForAllTemperaturesFactory
    {
        IEnumerable<BaseSimulation> CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, BaseSimulation> createSimulation);
    }
}