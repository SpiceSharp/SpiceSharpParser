using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IProcessingContext"/>
    /// </summary>
    public static class IProcessingContextExtensions
    {
        /// <summary>
        /// Sets entity parameters
        /// </summary>
        /// <param name="context">Processing context</param>
        /// <param name="entity">Entity to set parameters</param>
        /// <param name="parameters">Parameters to set</param>
        public static void SetParameters(this IProcessingContext context, Entity entity, ParameterCollection parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    if (context.SetParameter(entity, ap.Name, ap.Value) == false)
                    {
                        context.Result.AddWarning("Couldn't set parameter " + ap.Name);
                    }
                }
                else
                {
                    context.Result.AddWarning("Unsupported parameter: " + parameter.Image);
                }
            }
        }
    }
}
