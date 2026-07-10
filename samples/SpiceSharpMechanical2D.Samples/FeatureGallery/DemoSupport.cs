using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.IO;

namespace SpiceSharpMechanical2D.Samples.FeatureGallery
{
    internal static class DemoSupport
    {
        public static Transient CreateTransient(double stopTime, double maximumStep) =>
            new Transient(
                "tran",
                new Trapezoidal
                {
                    InitialStep = maximumStep,
                    MaxStep = maximumStep,
                    StopTime = stopTime,
                });

        public static void Run(
            Transient simulation,
            IEntity[] entities,
            Action<double> export)
        {
            foreach (int exportType in simulation.Run(new Circuit(entities)))
            {
                if (exportType == Transient.ExportTransient)
                    export(simulation.Time);
            }
        }

        public static void WriteRow(TextWriter output, params double[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (index > 0)
                    output.Write(',');
                output.Write(values[index].ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }

            output.WriteLine();
        }
    }

    internal sealed class SampleGate
    {
        private readonly double _interval;
        private double _next;

        public SampleGate(double interval)
        {
            _interval = interval;
        }

        public bool ShouldWrite(double time)
        {
            if (time + 1e-12 < _next)
                return false;

            do
            {
                _next += _interval;
            }
            while (_next <= time + 1e-12);

            return true;
        }
    }
}
