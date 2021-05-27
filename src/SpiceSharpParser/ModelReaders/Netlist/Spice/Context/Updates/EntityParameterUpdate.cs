using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public abstract class EntityParameterUpdate
    {
        public string ParameterName { get; set; }

        public abstract double GetValue(EvaluationContext context);
    }
}