using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public interface IResultService
    {
        SimulationConfiguration SimulationConfiguration { get; }

        IEnumerable<Simulation> Simulations { get; }

        void AddWarning(string warning);

        void AddComment(CommentLine statement);

        void AddExport(SpiceSharp.Parser.Readers.Export export);

        void AddPlot(Plot plot);

        void AddEntity(Entity entity);

        void AddSimulation(BaseSimulation simulation);

        void SetInitialVoltageCondition(string nodeName, double initialVoltage);

        bool FindObject(string modelNameToSearch, out Entity model);
    }
}
