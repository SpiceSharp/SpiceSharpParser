using System.Text;

namespace SpiceNetlist.SpiceObjects.Parameters
{
    public class BracketParameter : Parameter
    {
        public string Name { get; set; }

        public ParameterCollection Parameters { get; set; }

        public override string Image
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Name + "(");

                foreach (Parameter parameter in Parameters)
                {
                    builder.Append(parameter.Image + ",");
                }

                builder.Append(")");
                return builder.ToString();
            }
        }
    }
}
