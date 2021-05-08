using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A SPICE component.
    /// </summary>
    public class Component : Statement
    {
        public Component(string name, ParameterCollection pinsAndParameters, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            PinsAndParameters = pinsAndParameters;
        }

        /// <summary>
        /// Gets or sets the name of component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets name parameter.
        /// </summary>
        public Parameter NameParameter { get; set; }

        /// <summary>
        /// Gets or sets pins and components parameters.
        /// </summary>
        public ParameterCollection PinsAndParameters { get; set; }

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        public override int EndLineNumber => PinsAndParameters.LastOrDefault()?.LineInfo.LineNumber ?? base.EndLineNumber;

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var clone = new Component(Name, (ParameterCollection)PinsAndParameters.Clone(), LineInfo);
            clone.NameParameter = (Parameter)NameParameter.Clone();
            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Name} {PinsAndParameters}";
        }
    }
}