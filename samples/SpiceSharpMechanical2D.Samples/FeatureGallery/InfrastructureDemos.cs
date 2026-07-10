using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.ApiProbe;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using System;
using System.IO;

namespace SpiceSharpMechanical2D.Samples.FeatureGallery
{
    internal static class InfrastructureDemos
    {
        public static void ApiOscillator(TextWriter output)
        {
            var probe = new TransientApiProbe("oscillator", initialA: 1.0, initialB: 0.0);
            Transient simulation = DemoSupport.CreateTransient(2.0 * Math.PI, 0.01);
            var a = new RealPropertyExport(simulation, probe.Name, "a");
            var b = new RealPropertyExport(simulation, probe.Name, "b");
            var gate = new SampleGate(Math.PI / 4.0);
            output.WriteLine("time,a,b,exact_a,exact_b");

            DemoSupport.Run(
                simulation,
                new IEntity[] { new Resistor("validation-reference", "unused", "0", 1.0), probe },
                time =>
            {
                if (gate.ShouldWrite(time))
                    DemoSupport.WriteRow(output, time, a.Value, b.Value, Math.Cos(time), -Math.Sin(time));
            });
        }

        public static void MathTour(TextWriter output)
        {
            var vector = new Vector2D(3.0, 4.0);
            Vector2D rotated = vector.Rotate(Math.PI / 2.0);
            Vector2D normalized = vector.Normalized(1e-12);
            output.WriteLine("quantity,value_1,value_2");
            output.WriteLine(FormattableString.Invariant($"vector,{vector.X:R},{vector.Y:R}"));
            output.WriteLine(FormattableString.Invariant($"rotated_90,{rotated.X:R},{rotated.Y:R}"));
            output.WriteLine(FormattableString.Invariant($"normalized,{normalized.X:R},{normalized.Y:R}"));
            output.WriteLine(FormattableString.Invariant($"length,{vector.Length:R},0"));
            output.WriteLine(FormattableString.Invariant(
                $"dot_with_unit_x,{Vector2D.Dot(vector, Vector2D.UnitX):R},0"));
            output.WriteLine(FormattableString.Invariant(
                $"cross_with_unit_y,{Vector2D.Cross(vector, Vector2D.UnitY):R},0"));
            output.WriteLine(FormattableString.Invariant(
                $"wrapped_angle,{AngleMath.WrapSigned(3.5 * Math.PI):R},0"));
            output.WriteLine(FormattableString.Invariant(
                $"smooth_positive,{SmoothFunctions.PositivePart(-0.02, 0.01):R},{SmoothFunctions.PositivePartDerivative(-0.02, 0.01):R}"));
            output.WriteLine(FormattableString.Invariant(
                $"tanh_friction,{SmoothFunctions.TanhFriction(0.03, 0.01):R},{SmoothFunctions.TanhFrictionDerivative(0.03, 0.01):R}"));
        }
    }
}
