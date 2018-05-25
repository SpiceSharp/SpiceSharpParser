using System.Collections.Generic;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls.Exporters;
using SpiceSharpParser.ModelReader.Spice.Processors.Controls.Plots;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReader.Spice.Context
{
    public interface IResultService
    {
        SimulationConfiguration SimulationConfiguration { get; }

        IEnumerable<Simulation> Simulations { get; }

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
