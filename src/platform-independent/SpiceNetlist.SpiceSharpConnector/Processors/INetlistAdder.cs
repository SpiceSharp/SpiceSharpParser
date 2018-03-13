using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Plots;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public interface INetlistAdder
    {
        void AddWarning(string warning);

        void AddComment(CommentLine statement);

        void AddExport(SpiceSharp.Parser.Readers.Export export);

        void AddPlot(Plot plot);

        void AddEntity(Entity entity);

        void AddSimulation(BaseSimulation simulation);
    }
}
