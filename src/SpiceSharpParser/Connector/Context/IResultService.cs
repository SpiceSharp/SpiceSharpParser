using System.Collections.Generic;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Connector.Processors.Controls.Plots;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Connector.Context
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

        bool FindObject(string modelNameToSearch, out Entity model);
    }
}
