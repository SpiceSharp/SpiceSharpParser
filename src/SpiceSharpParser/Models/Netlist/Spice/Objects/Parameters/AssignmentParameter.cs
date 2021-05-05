using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// An assignment parameter.
    /// </summary>
    public class AssignmentParameter : Parameter
    {
        public AssignmentParameter()
        {
        }

        public AssignmentParameter(string name, List<string> arguments, List<string> values, bool hasFunctionSyntax, SpiceLineInfo lineInfo)
            : base(lineInfo)
        {
            Name = name;
            Arguments = arguments;
            Values = values;
            HasFunctionSyntax = hasFunctionSyntax;
        }

        /// <summary>
        /// Gets or sets the name of the assignment parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the arguments of the assignment parameters.
        /// </summary>
        public List<string> Arguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assignment parameter has "()" function syntax.
        /// </summary>
        public bool HasFunctionSyntax { get; set; }

        /// <summary>
        /// Gets or sets the values of assignment parameter.
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the value of assignment parameter.
        /// </summary>
        public override string Value
        {
            get => Values[0];

            set
            {
                if (Values.Count == 0)
                {
                    Values.Insert(0, "0");
                }

                Values[0] = value;
            }
        }

        /// <summary>
        /// Gets the string representation of the parameter.
        /// </summary>
        public override string ToString()
        {
            if (Arguments?.Count > 0)
            {
                return Name + "(" + string.Join(",", Arguments) + ") =" + Value;
            }
            else
            {
                if (HasFunctionSyntax)
                {
                    return Name + "() = " + Values;
                }

                return $"{Name}={{{string.Join(",", Values.ToArray())}}}";
            }
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            return new AssignmentParameter(Name, Arguments?.ToList(), Values?.ToList(), HasFunctionSyntax, LineInfo);
        }
    }
}