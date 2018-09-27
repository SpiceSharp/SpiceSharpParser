using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    /// <summary>
    /// Reads .OP <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class OPControl : SimulationControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
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
