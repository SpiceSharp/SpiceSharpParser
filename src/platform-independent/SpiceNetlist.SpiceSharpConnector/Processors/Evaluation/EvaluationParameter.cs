namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    public class EvaluationParameter : SpiceSharp.Parameter
    {
        public EvaluationParameter(Evaluator evaluator, string sweepParameterName)
        {
            SweepParameterName = sweepParameterName;
            Evaluator = evaluator;
        }

        public Evaluator Evaluator { get; }

        public string SweepParameterName { get; }

        public override object Clone()
        {
            return base.Clone();
        }

        public override void CopyFrom(SpiceSharp.Parameter source)
        {
            base.CopyFrom(source);
        }

        public override void CopyTo(SpiceSharp.Parameter target)
        {
            base.CopyTo(target);
        }

        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Set(double value)
        {
            base.Set(value);

            Evaluator.Parameters[SweepParameterName] = value;
            Evaluator.Refresh();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
