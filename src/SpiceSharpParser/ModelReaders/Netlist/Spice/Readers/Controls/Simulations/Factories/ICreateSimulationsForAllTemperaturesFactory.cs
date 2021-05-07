using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public interface ICreateSimulationsForAllTemperaturesFactory
    {
        List<Simulation> CreateSimulations(Control statement, IReadingContext context, Func<string, Control, IReadingContext, Simulation> createSimulation);
    }
}