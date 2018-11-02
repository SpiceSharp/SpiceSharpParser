using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class DiodeGenerator : IComponentGenerator
    {
        /// <summary>
        /// Gets generated types.
        /// </summary>
        /// <returns>
        /// Generated types.
        /// </returns>
        public IEnumerable<string> GeneratedTypes => new List<string>() { "D" };

        public SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 3)
            {
                throw new System.Exception("Model expected");
            }

            Diode diode = new Diode(componentIdentifier);
            context.CreateNodes(diode, parameters);

            context.ModelsRegistry.SetModel<DiodeModel>(
              diode,
              parameters.GetString(2),
              $"Could not find model {parameters.GetString(2)} for diode {originalName}",
              (DiodeModel model) => diode.SetModel(model));

            // Read the rest of the parameters
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i] is WordParameter w)
                {
                    if (w.Image.ToLower() == "on")
                    {
                        diode.SetParameter("off", false);
                    }
                    else if (w.Image.ToLower() == "off")
                    {
                        diode.SetParameter("off", true);
                    }
                    else
                    {
                        throw new System.Exception("Expected on/off for diode");
                    }
                }

                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        context.SetParameter(diode, "ic", asg.Value);
                    }
                }

                if (parameters[i] is ValueParameter v1 || parameters[i] is ExpressionParameter v2)
                {
                    // TODO: Fix this please it's broken ...
                    var bp = diode.ParameterSets.Get<SpiceSharp.Components.DiodeBehaviors.BaseParameters>();
                    if (!bp.Area.Given)
                    {
                        bp.Area.Value = context.EvaluateDouble(parameters.GetString(i));
                    }
                    else
                    {
                        if (!bp.Temperature.Given)
                        {
                            bp.Temperature.Value = context.EvaluateDouble(parameters.GetString(i));
                        }
                    }
                }
            }

            return diode;
        }
    }
}
