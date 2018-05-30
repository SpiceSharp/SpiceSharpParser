using SpiceSharp.Simulations;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reades .OP <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class OPControl : SimulationControl
    {
        public override string SpiceName => "op";

        /// <summary>
        /// Reades <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Read(Control statement, IReadingContext context)
        {
            CreateSimulations(statement, context, CreateOperatingPointSimulation);
        }

        private OP CreateOperatingPointSimulation(string name, Control statement, IReadingContext context)
        {
            var op = new OP(name);
            SetBaseConfiguration(op.BaseConfiguration, context);
            context.Result.AddSimulation(op);

            return op;
        }
    }
}
