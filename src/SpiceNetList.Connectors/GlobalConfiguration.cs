using SpiceSharp.IntegrationMethods;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class GlobalConfiguration
    {
        public double? AbsoluteTolerance { get; internal set; }

        public double? RelTolerance { get; internal set; }

        public double? Gmin { get; internal set; }

        public int? DCMaxIterations { get; internal set; }

        public int? SweepMaxIterations { get; internal set; }

        public int? TranMaxIterations { get; internal set; }

        public Trapezoidal Method { get; internal set; }

        public bool? KeepOpInfo { get; internal set; }
    }
}
