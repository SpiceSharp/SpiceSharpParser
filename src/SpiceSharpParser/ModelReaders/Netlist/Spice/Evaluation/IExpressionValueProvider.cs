namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IExpressionValueProvider
    {
        double GetExpressionValue(string expression, object context, bool @throw = true);
    }
}
