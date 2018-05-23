using SpiceSharpParser.ModelReader.Spice.Processors;
using SpiceSharpParser.ModelReader.Spice.Processors.Common;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Processors.Controls
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
