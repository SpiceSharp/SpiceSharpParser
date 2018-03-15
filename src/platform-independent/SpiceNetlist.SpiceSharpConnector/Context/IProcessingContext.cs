using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp.Circuits;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Context
{
    public interface IProcessingContext
    {
        string ContextName { get; }

        IProcessingContext Parent { get;  }

        List<SubCircuit> AvailableSubcircuits { get; }

        IEvaluator Evaluator { get; }

        IResultService Result { get; }

        NodeNameGenerator NodeNameGenerator { get; }

        ObjectNameGenerator ObjectNameGenerator { get; }

        double ParseDouble(string expression);

        void SetICVoltage(string nodeName, string expression);

        void SetParameter(Entity entity, string propertyName, string expression);

        void SetParameters(Entity entity, ParameterCollection parameters, int toSkip = 0);

        T FindModel<T>(string modelName)
            where T : Entity;

        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);
    }
}
