using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class SubCircuitGenerator : EntityGenerator
    {
        private ComponentProcessor componentProcessor;
        private ModelProcessor modelProcessor;

        public SubCircuitGenerator(ComponentProcessor componentProcessor, ModelProcessor modelProcessor)
        {
            this.componentProcessor = componentProcessor;
            this.modelProcessor = modelProcessor;
        }

        public override Entity Generate(Identifier id, string name, string type, ParameterCollection parameters, ProcessingContext context)
        {
            SubCircuit subCiruitDefiniton;
            ProcessingContext newContext;

            ProcessParamters(name, parameters, context, out subCiruitDefiniton, out newContext);

            foreach (Statement statement in subCiruitDefiniton.Statements.OrderBy(s => (s is Model ? 0 : 1)))
            {
                if (statement is Component c)
                {
                    componentProcessor.Process(c, newContext);
                }

                if (statement is Model m)
                {
                    modelProcessor.Process(m, newContext);
                }
            }

            return null;
        }

        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "x" };
        }

        private static void ProcessParamters(string name, ParameterCollection parameters, ProcessingContext context, out SubCircuit subCiruitDefiniton, out ProcessingContext newContext)
        {
            int parametersCount = 0;

            var subCktParameters = new List<AssignmentParameter>();
            while (parameters[parameters.Count - parametersCount - 1] is AssignmentParameter a)
            {
                subCktParameters.Add(a);
                parametersCount++;
            }

            var pinInstanceNames = new List<string>();
            for (var i = 0; i < parameters.Count - parametersCount - 1; i++)
            {
                pinInstanceNames.Add(parameters.GetString(i));
            }

            string subCircuitName = parameters.GetString(parameters.Count - parametersCount - 1);

            subCiruitDefiniton = context.AvailableSubcircuits.Find(pred => pred.Name == subCircuitName);

            if (subCiruitDefiniton == null)
            {
                throw new System.Exception("Can't find " + subCircuitName + " subcircuit");
            }

            Dictionary<string, double> subCktParamters = ResolveSubcircuitParameters(context, subCiruitDefiniton, subCktParameters);

            newContext = new ProcessingContext(name, context, subCiruitDefiniton, pinInstanceNames, subCktParamters);
        }

        private static Dictionary<string, double> ResolveSubcircuitParameters(ProcessingContext context, SubCircuit subCiruitDefiniton, List<AssignmentParameter> subcktParameters)
        {
            var newContextParameters = new Dictionary<string, double>();
            foreach (var defaultParameter in subCiruitDefiniton.DefaultParameters)
            {
                newContextParameters[defaultParameter.Name] = context.ParseDouble(defaultParameter.Value);
            }

            foreach (var instanceParameter in subcktParameters)
            {
                newContextParameters[instanceParameter.Name] = context.ParseDouble(instanceParameter.Value);
            }

            return newContextParameters;
        }
    }
}