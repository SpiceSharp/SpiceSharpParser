using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    /// <summary>
    /// A vector parameter.
    /// </summary>
    public class VectorParameter : Parameter
    {
        public VectorParameter(List<SingleParameter> elements)
        {
            Elements = elements;
        }

        public VectorParameter()
        {
            Elements = new List<SingleParameter>();
        }

        public override SpiceLineInfo LineInfo => Elements.FirstOrDefault()?.LineInfo;

        /// <summary>
        /// Gets the elements of the vector.
        /// </summary>
        public List<SingleParameter> Elements { get; }

        public override string Value { get => ToString(); }

        /// <summary>
        /// Gets the string representation of the vector.
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var parameter in Elements)
            {
                if (builder.Length != 0)
                {
                    builder.Append(",");
                }

                builder.Append(parameter);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var result = new VectorParameter();

            foreach (var element in Elements)
            {
                result.Elements.Add((SingleParameter)element.Clone());
            }

            return result;
        }
    }
}