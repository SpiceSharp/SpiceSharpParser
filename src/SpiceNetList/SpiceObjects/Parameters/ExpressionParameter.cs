namespace SpiceNetlist.SpiceObjects.Parameters
{
    /// <summary>
    /// An expression parameter
    /// </summary>
    public class ExpressionParameter : SingleParameter
    {
        public ExpressionParameter(string expression)
            : base(expression)
        {
        }
    }
}
