using SpiceSharpParser.Model.SpiceObjects.Parameters;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.Model.SpiceObjects
{
    /// <summary>
    /// A vector parameter
    /// </summary>
    public class VectorParameter : Parameter
    {
        /// <summary>
        /// Gets or sets the elements of the vector
        /// </summary>
        public List<SingleParameter> Elements { get; set; } = new List<SingleParameter>();

        /// <summary>
        /// Gets the string represenation of the vector
        /// </summary>
        public override string Image
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                foreach (var parameter in Elements)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(",");
                    }

                    builder.Append(parameter.Image);
                }

                return builder.ToString();
            }
        }
    }
}
