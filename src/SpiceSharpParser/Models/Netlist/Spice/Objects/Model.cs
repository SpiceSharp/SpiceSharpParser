using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A SPICE model.
    /// </summary>
    public class Model : Statement
    {
        public Model(string name, ParameterCollection parameters, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets parameters of the model.
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
            return new Model(Name, (ParameterCollection)Parameters.Clone(), LineInfo);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $".MODEL {Name} {Parameters}";
        }
    }
}