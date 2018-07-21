using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Plots;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Prints;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Simulations;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Context
{
    public interface IResultService
    {
        SimulationConfiguration SimulationConfiguration { get; }

        MonteCarloResult MonteCarlo { get; }

        IEnumerable<Simulation> Simulations { get; }

        IEnumerable<Export> Exports { get; }

        Circuit Circuit { get; }

        void AddWarning(string warning);

        void AddComment(CommentLine statement);

        void AddExport(Export export);

        void AddPlot(XyPlot plot);

        void AddEntity(Entity entity);

        void AddSimulation(BaseSimulation simulation);

        bool FindObject(string objectName, out Entity @object);

        void AddPrint(Print print);
    }
}
