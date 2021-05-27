using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Generic;
using Component = SpiceSharpParser.Models.Netlist.Spice.Objects.Component;

namespace SpiceSharpParser.ModelWriters.CSharp.Entities.Components
{
    public class MosfetWriter : BaseWriter, IWriter<Component>
    {
        public MosfetWriter()
        {
            // MOS1
            Mosfets.Add("Mosfet1Model", (_) => "Mosfet1");

            // MOS2
            Mosfets.Add("Mosfet2Model", (_) => "Mosfet2");

            // MOS3
            Mosfets.Add("Mosfet3Model", (_) => "Mosfet3");
        }

        private Dictionary<string, Func<string, string>> Mosfets { get; } = new ();

        public List<CSharpStatement> Write(Component @object, IWriterContext context)
        {
            var result = new List<CSharpStatement>();

            if (@object.PinsAndParameters.Count < 5)
            {
                result.Add(new CSharpComment("Skipped, wrong pins/parameters count:" + @object));
                return result;
            }

            var pins = @object.PinsAndParameters.Take(4);
            var parameters = @object.PinsAndParameters.Skip(4);
            var name = @object.Name;

            var mosfetId = context.GetNewIdentifier(name);
            var modelNameParameter = parameters.Get(0);

            var modelType = context.FindModelType(modelNameParameter.Value);
            if (Mosfets.ContainsKey(modelType))
            {
                var modelName = modelNameParameter.Value;
                result.Add(new CSharpNewStatement(
                    mosfetId,
                    $@"new {Mosfets[modelType]}(""{name}"", ""{pins[0].Value}"", ""{pins[1].Value}"", ""{pins[2].Value}"",  ""{pins[3].Value}"", ""{modelName}"")"));
            }

            for (var i = 1; i < parameters.Count; i++)
            {
                if (parameters[i] is AssignmentParameter asg)
                {
                    if (asg.Name.ToLower() != "m")
                    {
                        result.Add(SetParameter(mosfetId, asg.Name, asg.Value, context));
                    }
                }
            }

            SetParallelParameter(result, mosfetId, parameters, context);

            return result;
        }
    }
}
