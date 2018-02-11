using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    internal class SubCircuitGenerator : EntityGenerator
    {
        private ComponentProcessor componentProcessor;

        public SubCircuitGenerator(ComponentProcessor componentProcessor)
        {
            this.componentProcessor = componentProcessor;
        }

        public override Entity Generate(string name, string type, ParameterCollection parameters, NetList netList)
        {
            string subCircuitName = (parameters[parameters.Count - 1] as SingleParameter).RawValue;
            var subCiruitDefiniton = netList.Definitions.Find(pred => pred.Name == subCircuitName);
            componentProcessor.NamePrefix = name + "_";

            foreach (Statement statement in subCiruitDefiniton.Statements)
            {
                if (statement is Component c)
                {
                    componentProcessor.Process(statement, netList);
                }
            }

            return null;
        }

        public override List<string> GetGeneratedTypes()
        {
            return new List<string>() { "x" };
        }
    }
}