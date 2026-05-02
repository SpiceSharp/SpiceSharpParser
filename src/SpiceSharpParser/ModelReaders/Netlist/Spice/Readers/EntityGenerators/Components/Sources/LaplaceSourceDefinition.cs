using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceSourceDefinition
    {
        public LaplaceSourceDefinition(
            string sourceName,
            string outputPositiveNode,
            string outputNegativeNode,
            string inputExpression,
            string transferExpression,
            LaplaceSourceInput input,
            LaplaceTransferFunction transferFunction,
            double delay,
            SpiceLineInfo lineInfo)
        {
            SourceName = sourceName;
            OutputPositiveNode = outputPositiveNode;
            OutputNegativeNode = outputNegativeNode;
            InputExpression = inputExpression;
            TransferExpression = transferExpression;
            Input = input;
            TransferFunction = transferFunction;
            Delay = delay;
            LineInfo = lineInfo;
        }

        public string SourceName { get; }

        public string OutputPositiveNode { get; }

        public string OutputNegativeNode { get; }

        public string InputExpression { get; }

        public string TransferExpression { get; }

        public LaplaceSourceInput Input { get; }

        public LaplaceTransferFunction TransferFunction { get; }

        public double Delay { get; }

        public SpiceLineInfo LineInfo { get; }
    }
}
