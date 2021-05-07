using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A SPICE control.
    /// </summary>
    public class Control : Statement
    {
        public Control()
            : base(null)
        {
        }

        public Control(string name, ParameterCollection parameters, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the name of control.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters of control.
        /// </summary>
        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        public override int EndLineNumber => Parameters.LastOrDefault()?.LineInfo.LineNumber ?? base.EndLineNumber;

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new Control(Name, (ParameterCollection)Parameters.Clone(), LineInfo);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (Parameters.Any())
            {
                return $".{Name} {Parameters}";
            }
            else
            {
                return $".{Name}";
            }
        }
    }
}