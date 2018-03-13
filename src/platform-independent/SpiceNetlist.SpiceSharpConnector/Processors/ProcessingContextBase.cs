using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Evaluation;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    //TODO: Add comments
    public abstract class ProcessingContextBase
    {
        public string ContextName { get; protected set; }

        public ProcessingContextBase Parent { get; protected set; }

        public abstract IEnumerable<Simulation> Simulations { get; }

        public List<SubCircuit> AvailableSubcircuits { get; protected set; }

        public SimulationConfiguration SimulationConfiguration { get; } = new SimulationConfiguration();

        public IEvaluator Evaluator { get; protected set; }

        public virtual INetlistAdder Adder { get; protected set; }

        public Netlist Netlist { get; protected set; }

        public virtual NameGenerator NameGenerator { get; protected set; }

        public string Path { get; protected set; }

        public abstract double ParseDouble(string expression);

        public abstract void SetICVoltage(string nodeName, string value);

        public abstract void SetParameter(Entity entity, string propertyName, string expression);

        public abstract void SetParameters(Entity entity, ParameterCollection parameters, int toSkip = 0);

        public abstract T FindModel<T>(string modelName)
            where T : Entity;

        public abstract void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters);
    }
}
