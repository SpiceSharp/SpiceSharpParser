using SpiceNetlist.SpiceObjects.Parameters;
using System.Collections.Generic;
using System.Text;

namespace SpiceNetlist.SpiceObjects
{
    public class VectorParameter : Parameter
    {
        public List<SingleParameter> Elements { get; set; } = new List<SingleParameter>();

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
