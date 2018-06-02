using System.Collections.Generic;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Context
{
    public interface IResultService
    {
        SimulationConfiguration SimulationConfiguration { get; }

        IEnumerable<Simulation> Simulations { get; }

        Circuit Circuit { get; }

        void AddWarning(string warning);

        void AddComment(CommentLine statement);

        void AddExport(Export export);

        void AddPlot(Plot plot);

        void AddEntity(Entity entity);

        void AddSimulation(BaseSimulation simulation);

        void SetInitialVoltageCondition(string nodeName, double initialVoltage);

        bool FindObject(string objectName, out Entity @object);
    }
}
