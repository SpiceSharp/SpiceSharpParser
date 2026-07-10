using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharp.Physics2D.Samples.Pendulum
{
    internal static class Program
    {
        private static void Main()
        {
            const double length = 1.0;
            const double initialAngle = 0.4;
            var localPivot = new Vector2D(0.0, length / 2.0);
            Vector2D initialRadius = localPivot.Rotate(initialAngle);
            var body = new RigidBody2D(
                "pendulum",
                mass: 1.0,
                inertia: (length * length) / 12.0,
                initialPosition: -initialRadius,
                initialAngle: initialAngle);
            var pivot = new DistanceSpringDamper2D(
                "compliant-pivot",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, localPivot),
                restLength: 0.0,
                stiffness: 2500.0,
                damping: 35.0,
                lengthRegularization: 1e-5);
            var gravity = new Gravity2D(
                "gravity",
                body.Name,
                new Vector2D(0.0, -9.80665));
            var method = new Trapezoidal
            {
                InitialStep = 0.001,
                MaxStep = 0.001,
                StopTime = 3.0,
            };
            var simulation = new Transient("tran", method);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var vx = new RealPropertyExport(simulation, body.Name, "vx");
            var vy = new RealPropertyExport(simulation, body.Name, "vy");
            var omega = new RealPropertyExport(simulation, body.Name, "omega");
            var circuit = new Circuit(
                new Resistor("validation-reference", "unused", "0", 1.0),
                body,
                pivot,
                gravity);

            Console.WriteLine("time,x,y,angle,vx,vy,omega,pivot_error");
            foreach (int exportType in simulation.Run(circuit))
            {
                if (exportType != Transient.ExportTransient)
                    continue;

                Vector2D worldPivot = new Vector2D(x.Value, y.Value) +
                    localPivot.Rotate(angle.Value);
                Console.WriteLine(FormattableString.Invariant(
                    $"{simulation.Time:R},{x.Value:R},{y.Value:R},{angle.Value:R},{vx.Value:R},{vy.Value:R},{omega.Value:R},{worldPivot.Length:R}"));
            }
        }
    }
}
