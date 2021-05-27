using System.Text;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A bracket parameter.
    /// </summary>
    public class BracketParameter : Parameter
    {
        public BracketParameter()
        {
        }

        public BracketParameter(string name, ParameterCollection parameters, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the name of the bracket parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters inside the bracket.
        /// </summary>
        public ParameterCollection Parameters { get; set; }

        public override string Value { get => ToString(); }

        /// <summary>
        /// Gets the string representation of the bracket parameter.
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if (Parameters.Count > 0)
            {
                builder.Append(Name + "(");

                for (var i = 0; i < Parameters.Count; i++)
                {
                    builder.Append(Parameters[i]);

                    if (i != Parameters.Count - 1)
                    {
                        builder.Append(",");
                    }
                }

                builder.Append(")");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new BracketParameter(Name, Parameters, LineInfo);
        }
    }
}