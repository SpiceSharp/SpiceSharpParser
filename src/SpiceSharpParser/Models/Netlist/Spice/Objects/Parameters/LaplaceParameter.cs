namespace SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters
{
    public class LaplaceParameter : Parameter
    {
        public LaplaceParameter(string inputExpression, string transferFunction, SpiceLineInfo lineInfo)
            : base($"{{{inputExpression}}} = {{{transferFunction}}}", lineInfo)
        {
            InputExpression = inputExpression;
            TransferFunction = transferFunction;
        }

        public string InputExpression { get; }

        public string TransferFunction { get; }

        public override string ToString()
        {
            return $"{{{InputExpression}}} = {{{TransferFunction}}}";
        }

        public override SpiceObject Clone()
        {
            return new LaplaceParameter(InputExpression, TransferFunction, LineInfo);
        }
    }
}
