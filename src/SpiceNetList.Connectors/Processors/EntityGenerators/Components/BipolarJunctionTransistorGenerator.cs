using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
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
            if (parameters.Values.Count == 5 && parameters.Values[4] is WordParameter w && w.RawValue == "off")
                parameters.Values.Insert(3, parameters.Values[2]);
            // If the component is of the format QXXX NC NB NE MNAME we will insert NE again before the model name
            if (parameters.Values.Count == 4)
                parameters.Values.Insert(3, parameters.Values[2]);

            //TODO Refactor this .....
            Identifier[] nodes = new Identifier[4];
            nodes[0] = (parameters.Values[0] as SingleParameter).RawValue;
            nodes[1] = (parameters.Values[1] as SingleParameter).RawValue;
            nodes[2] = (parameters.Values[2] as SingleParameter).RawValue;
            nodes[3] = (parameters.Values[3] as SingleParameter).RawValue;
            bjt.Connect(nodes);


            if (parameters.Values.Count < 5)
                throw new System.Exception();

            bjt.SetModel((BipolarJunctionTransistorModel)currentNetList.FindModel((parameters.Values[4] as SingleParameter).RawValue));

            for (int i = 5; i < parameters.Values.Count; i++)
            {
                var parameter = parameters.Values[i];

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
                        ModelBaseParameters mp = bjt.Model.ParameterSets[typeof(ModelBaseParameters)] as ModelBaseParameters;
                        BaseParameters bp = bjt.ParameterSets[typeof(SpiceSharp.Components.BipolarBehaviors.BaseParameters)] as BaseParameters;
                        //TODO ?????
                        if (bp.Area.Given == false)
                        {
                            bp.Area.Set(currentNetList.ParseDouble(s.RawValue));
                        }
                        //TODO ?????
                        if (bp.Temperature.Given == false)
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
