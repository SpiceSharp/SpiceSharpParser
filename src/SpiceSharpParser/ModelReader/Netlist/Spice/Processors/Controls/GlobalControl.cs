﻿using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls
{
    /// <summary>
    /// Processes .GLOBAL <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class GlobalControl : BaseControl
    {
        /// <summary>
        /// Gets name of Spice element
        /// </summary>
        public override string TypeName => "global";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if (param is Model.Netlist.Spice.Objects.Parameters.SingleParameter sp)
                {
                    if (sp is Model.Netlist.Spice.Objects.Parameters.WordParameter
                        || sp is Model.Netlist.Spice.Objects.Parameters.IdentifierParameter
                        || sp is Model.Netlist.Spice.Objects.Parameters.ValueParameter)
                    {
                        context.NodeNameGenerator.SetGlobal(sp.Image);
                    }
                }
            }
        }
    }
}