using SpiceSharpParser.ModelReader.Netlist.Spice.Processors;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls
{
    /// <summary>
    /// Base for all control processors
    /// </summary>
    public abstract class BaseControl : StatementProcessor<Control>, IGenerator
    {
        /// <summary>
        /// Gets name of Spice element
        /// </summary>
        public abstract string TypeName
        {
            get;
        }
    }
}
