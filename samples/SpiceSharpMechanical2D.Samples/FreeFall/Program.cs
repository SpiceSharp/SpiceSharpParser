using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharpMechanical2D.Samples.FreeFall
{
    internal static class Program
    {
        private static void Main()
        {
            var body = new RigidBody2D(
                "projectile",
                mass: 1.0,
                inertia: 0.1,
                initialPosition: new Vector2D(0.0, 10.0),
                initialLinearVelocity: new Vector2D(2.0, 0.0));
            var gravity = new Gravity2D(
                "gravity",
                body.Name,
                new Vector2D(0.0, -9.80665));
            var method = new Trapezoidal
            {
                InitialStep = 0.01,
                MaxStep = 0.01,
                StopTime = 2.0,
            };
            var simulation = new Transient("tran", method);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var vx = new RealPropertyExport(simulation, body.Name, "vx");
            var vy = new RealPropertyExport(simulation, body.Name, "vy");
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var omega = new RealPropertyExport(simulation, body.Name, "omega");
            var circuit = new Circuit(
                body,
                gravity);

            Console.WriteLine("time,x,y,vx,vy,angle,omega");
            foreach (int exportType in simulation.Run(circuit))
            {
                if (exportType == Transient.ExportTransient)
                {
                    Console.WriteLine(FormattableString.Invariant(
                        $"{simulation.Time:R},{x.Value:R},{y.Value:R},{vx.Value:R},{vy.Value:R},{angle.Value:R},{omega.Value:R}"));
                }
            }
        }
    }
}
