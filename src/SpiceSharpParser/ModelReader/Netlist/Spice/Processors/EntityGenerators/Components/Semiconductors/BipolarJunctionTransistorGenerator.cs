using System.Collections.Generic;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Exceptions;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.BipolarBehaviors;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors.EntityGenerators.Components.Semiconductors
{
    public class BipolarJunctionTransistorGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier name, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            BipolarJunctionTransistor bjt = new BipolarJunctionTransistor(name);

            // If the component is of the format QXXX NC NB NE MNAME off we will insert NE again before the model name
            if (parameters.Count == 5 && parameters[4] is WordParameter w && w.Image == "off")
            {
                parameters.Insert(3, parameters[2]);
            }

            // If the component is of the format QXXX NC NB NE MNAME we will insert NE again before the model name
            if (parameters.Count == 4)
            {
                parameters.Insert(3, parameters[2]);
            }

            context.CreateNodes(bjt, parameters);

            if (parameters.Count < 5)
            {
                throw new System.Exception();
            }

            var model = context.FindModel<BipolarJunctionTransistorModel>(parameters.GetString(4));
            if (model == null)
            {
                throw new ModelNotFoundException($"Could not find model {parameters.GetString(4)} for BJT {name}");
            }

            bjt.SetModel(model);

            for (int i = 5; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter is SingleParameter s)
                {
                    if (s is WordParameter)
                    {
                        switch (s.Image.ToLower())
                        {
                            case "on": bjt.SetParameter("off", false); break;
                            case "off": bjt.SetParameter("on", false); break;
                            default: throw new System.Exception();
                        }
                    }
                    else
                    {
                        //TODO: Fix this please it's broken ...
                        BaseParameters bp = bjt.ParameterSets.Get<BaseParameters>();
                        if (!bp.Area.Given)
                        {
                            bp.Area.Value = context.ParseDouble(s.Image);
                        }

                        if (!bp.Temperature.Given)
                        {
                            bp.Area.Value = context.ParseDouble(s.Image);
                        }
                    }
                }

                if (parameter is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        bjt.SetParameter("ic", context.ParseDouble(asg.Value));
                    }
                }
            }

            return bjt;
        }

        /// <summary>
        /// Gets the generated types
        /// </summary>
        /// <returns>
        /// A list of generated types
        /// </returns>
        public override IEnumerable<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "q" };
        }
    }
}
