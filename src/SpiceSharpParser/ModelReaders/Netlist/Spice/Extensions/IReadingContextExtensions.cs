using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IReadingContext"/>
    /// </summary>
    public static class IReadingContextExtensions
    {
        /// <summary>
        /// Sets entity parameters
        /// </summary>
        /// <param name="context">Reading context</param>
        /// <param name="entity">Entity to set parameters</param>
        /// <param name="parameters">Parameters to set</param>
        public static void SetParameters(this IReadingContext context, Entity entity, ParameterCollection parameters)
        {
            foreach (Parameter parameter in parameters)
            {
                if (parameter is AssignmentParameter ap)
                {
                    if (context.SetEntityParameter(entity, ap.Name, ap.Value) == false)
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
