using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Core;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using System.IO;

namespace SpiceSharpMechanical2D.Samples.FeatureGallery
{
    internal static class MotionDemos
    {
        public static void CoordinateCoast(TextWriter output)
        {
            var coordinate = new MechanicalCoordinate(
                "slider-coordinate",
                generalizedMass: 2.0,
                initialPosition: 0.25,
                initialVelocity: 0.5);
            Transient simulation = DemoSupport.CreateTransient(2.0, 0.02);
            var position = new RealPropertyExport(simulation, coordinate.Name, "position");
            var velocity = new RealPropertyExport(simulation, coordinate.Name, "velocity");
            var energy = new RealPropertyExport(simulation, coordinate.Name, "kineticenergy");
            var gate = new SampleGate(0.25);
            output.WriteLine("time,position,velocity,kinetic_energy");

            DemoSupport.Run(simulation, new[] { coordinate }, time =>
            {
                if (gate.ShouldWrite(time))
                    DemoSupport.WriteRow(output, time, position.Value, velocity.Value, energy.Value);
            });
        }

        public static void RigidBodyCoast(TextWriter output)
        {
            var body = new RigidBody2D(
                "coasting-body",
                mass: 1.5,
                inertia: 0.4,
                initialPosition: new Vector2D(-0.5, 0.25),
                initialAngle: 0.2,
                initialLinearVelocity: new Vector2D(1.0, 0.4),
                initialAngularVelocity: 0.75);
            Transient simulation = DemoSupport.CreateTransient(2.0, 0.02);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var vx = new RealPropertyExport(simulation, body.Name, "vx");
            var vy = new RealPropertyExport(simulation, body.Name, "vy");
            var omega = new RealPropertyExport(simulation, body.Name, "omega");
            var energy = new RealPropertyExport(simulation, body.Name, "kineticenergy");
            var gate = new SampleGate(0.25);
            output.WriteLine("time,x,y,angle,vx,vy,omega,kinetic_energy");

            DemoSupport.Run(simulation, new[] { body }, time =>
            {
                if (gate.ShouldWrite(time))
                {
                    DemoSupport.WriteRow(
                        output,
                        time,
                        x.Value,
                        y.Value,
                        angle.Value,
                        vx.Value,
                        vy.Value,
                        omega.Value,
                        energy.Value);
                }
            });
        }
    }
}
