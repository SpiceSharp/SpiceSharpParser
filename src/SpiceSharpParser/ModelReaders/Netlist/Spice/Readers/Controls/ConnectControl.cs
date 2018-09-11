using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .CONNECT <see cref="Control"/> from Spice netlist object model.
    /// </summary>
    public class ConnectControl : BaseControl
    {
        public override string SpiceCommandName => "connect";

        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            if (statement.Parameters.Count != 2)
            {
                throw new WrongParametersCountException("Wrong parameter count for .connect");
            }

            string nodeA = statement.Parameters.GetString(0);
            string nodeB = statement.Parameters.GetString(1);

            var vsrc = new VoltageSource("Voltage connector: " + nodeA + " <-> " + nodeB);
            context.CreateNodes(vsrc, statement.Parameters);
            context.Result.AddEntity(vsrc);
        }
    }
}
