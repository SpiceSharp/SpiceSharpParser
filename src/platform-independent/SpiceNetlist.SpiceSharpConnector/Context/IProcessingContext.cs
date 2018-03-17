using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Evaluation;
using SpiceSharp.Circuits;

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

        T FindModel<T>(string modelName)
            where T : Entity;

        void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);
    }
}
