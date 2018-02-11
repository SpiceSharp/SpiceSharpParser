using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.BipolarBehaviors;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components
{
    class BipolarJunctionTransistorGenerator : EntityGenerator
    {
        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList currentNetList)
        {
            BipolarJunctionTransistor bjt = new BipolarJunctionTransistor(name);

            // If the component is of the format QXXX NC NB NE MNAME off we will insert NE again before the model name
            if (parameters.Count == 5 && parameters[4] is WordParameter w && w.RawValue == "off")
            {
                parameters.Insert(3, parameters[2]);
            }

            // If the component is of the format QXXX NC NB NE MNAME we will insert NE again before the model name
            if (parameters.Count == 4)
            {
                parameters.Insert(3, parameters[2]);
            }

            CreateNodes(parameters, bjt);

            if (parameters.Count < 5)
            {
                throw new System.Exception();
            }

            bjt.SetModel((BipolarJunctionTransistorModel)currentNetList.FindModel((parameters[4] as SingleParameter).RawValue));

            for (int i = 5; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter is SingleParameter s)
                {
                    if (s is WordParameter)
                    {
                        switch (s.RawValue.ToLower())
                        {
                            case "on": bjt.ParameterSets.SetProperty("off", false); break;
                            case "off": bjt.ParameterSets.SetProperty("on", false); break;
                            default: throw new System.Exception();
                        }
                    }
                    else
                    {
                        BaseParameters bp = bjt.ParameterSets[typeof(BaseParameters)] as BaseParameters;
                        //TODO ?????
                        if (!bp.Area.Given)
                        {
                            bp.Area.Set(currentNetList.ParseDouble(s.RawValue));
                        }
                        //TODO ?????
                        if (!bp.Temperature.Given)
                        {
                            bp.Area.Set(currentNetList.ParseDouble(s.RawValue));
                        }
                    }
                }

                if (parameter is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() == "ic")
                    {
                        bjt.ParameterSets.SetProperty("ic", currentNetList.ParseDouble(asg.Value));
                    }
                }
            }

            return bjt;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "q" };
        }
    }
}
