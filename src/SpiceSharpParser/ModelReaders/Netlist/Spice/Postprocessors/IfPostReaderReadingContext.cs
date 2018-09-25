using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Postprocessors
{
    public class IfPostReaderReadingContext : IReadingContext
    {
        public IfPostReaderReadingContext(IEvaluator evaluator)
        {
            ReadingEvaluator = evaluator;
        }

        public string ContextName => throw new NotImplementedException();

        public ISimulationContexts SimulationContexts => throw new NotImplementedException();

        public IReadingContext Parent => throw new NotImplementedException();

        public ICollection<IReadingContext> Children => throw new NotImplementedException();

        public ICollection<SubCircuit> AvailableSubcircuits => throw new NotImplementedException();

        public IResultService Result => throw new NotImplementedException();

        public INodeNameGenerator NodeNameGenerator => throw new NotImplementedException();

        public IObjectNameGenerator ObjectNameGenerator => throw new NotImplementedException();

        public IEvaluator ReadingEvaluator { get; }

        public IStochasticModelsRegistry StochasticModelsRegistry => throw new NotImplementedException();

        public ISpiceObjectMappings ObjectMappings => throw new NotImplementedException();

        public ISpiceStatementsReader StatementsReader { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IWaveformReader WaveformReader { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double ParseDouble(string expression)
        {
            throw new NotImplementedException();
        }

        public void SetICVoltage(string nodeName, string expression)
        {
            throw new NotImplementedException();
        }

        public void SetNodeSetVoltage(string nodeName, string expression)
        {
            throw new NotImplementedException();
        }

        public bool SetParameter(Entity entity, string parameterName, string expression)
        {
            throw new NotImplementedException();
        }

        public bool SetParameter(Entity entity, string parameterName, object @object)
        {
            throw new NotImplementedException();
        }

        public void CreateNodes(SpiceSharp.Components.Component component, ParameterCollection parameters)
        {
            throw new NotImplementedException();
        }

        public void Read(Statements statement, ISpiceStatementsOrderer orderer)
        {
            throw new NotImplementedException();
        }

        public bool SetParameter(Entity entity, string parameterName, Func<object, double> parameterValue)
        {
            throw new NotImplementedException();
        }
    }
}
