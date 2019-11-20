using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public interface ICreateSimulationsForAllTemperaturesFactory
    {
        List<BaseSimulation> CreateSimulations(Control statement, ICircuitContext context, Func<string, Control, ICircuitContext, BaseSimulation> createSimulation);
    }
}