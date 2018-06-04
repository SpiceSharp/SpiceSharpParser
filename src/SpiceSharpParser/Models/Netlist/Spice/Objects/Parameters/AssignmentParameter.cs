using System.Collections.Generic;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// An assigment parameter.
    /// </summary>
    public class AssignmentParameter : Parameter
    {
        /// <summary>
        /// Gets or sets the name of the assigment parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the arguments of the assigment parameters.
        /// </summary>
        public List<string> Arguments { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether the assigment parameter has "()" function syntax.
        /// </summary>
        public bool HasFunctionSyntax { get; set; }

        /// <summary>
        /// Gets or sets the value of assigment parameter.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the string represenation of the parameter.
        /// </summary>
        public override string Image
        {
            get
            {
                if (Arguments.Count > 0)
                {
                    return Name + "(" + string.Join(",", Arguments) + ") =" + Value;
                }
                else
                {
                    if (HasFunctionSyntax)
                    {
                        return Name + "() = " + Value;
                    }

                    return Name + "=" + Value;
                }
            }
        }

        public override SpiceObject Clone()
        {
            return new AssignmentParameter()
            {
                Arguments = new List<string>(this.Arguments.ToArray()),
                Value = this.Value,
                Name = this.Name,
            };
        }
    }
}
