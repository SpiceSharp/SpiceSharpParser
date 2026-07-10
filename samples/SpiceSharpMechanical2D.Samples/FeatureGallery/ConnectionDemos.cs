using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using System;
using System.IO;

namespace SpiceSharpMechanical2D.Samples.FeatureGallery
{
    internal static class ConnectionDemos
    {
        public static void TwoBodySpring(TextWriter output)
        {
            var bodyA = new RigidBody2D(
                "mass-a",
                mass: 1.0,
                inertia: 0.1,
                initialPosition: new Vector2D(-0.6, 0.0));
            var bodyB = new RigidBody2D(
                "mass-b",
                mass: 2.0,
                inertia: 0.1,
                initialPosition: new Vector2D(0.6, 0.0));
            var spring = new DistanceSpringDamper2D(
                "spring",
                MechanicalAnchor2D.Body(bodyA.Name, Vector2D.Zero),
                MechanicalAnchor2D.Body(bodyB.Name, Vector2D.Zero),
                restLength: 1.0,
                stiffness: 20.0,
                damping: 0.35,
                lengthRegularization: 1e-9);
            Transient simulation = DemoSupport.CreateTransient(3.0, 0.005);
            var xA = new RealPropertyExport(simulation, bodyA.Name, "x");
            var xB = new RealPropertyExport(simulation, bodyB.Name, "x");
            var vxA = new RealPropertyExport(simulation, bodyA.Name, "vx");
            var vxB = new RealPropertyExport(simulation, bodyB.Name, "vx");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,x_a,x_b,distance,extension,vx_a,vx_b");

            DemoSupport.Run(simulation, new IEntity[] { bodyA, bodyB, spring }, time =>
            {
                if (!gate.ShouldWrite(time))
                    return;

                double distance = xB.Value - xA.Value;
                DemoSupport.WriteRow(
                    output,
                    time,
                    xA.Value,
                    xB.Value,
                    distance,
                    distance - 1.0,
                    vxA.Value,
                    vxB.Value);
            });
        }

        public static void TorsionalSpring(TextWriter output)
        {
            const double stiffness = 4.0;
            var body = new RigidBody2D(
                "rotor",
                mass: 1.0,
                inertia: 0.25,
                initialAngle: 0.45);
            var spring = new RotationalSpringDamper2D(
                "torsional-spring",
                RotationalEndpoint2D.World(),
                RotationalEndpoint2D.Body(body.Name),
                referenceAngle: 0.0,
                stiffness: stiffness,
                damping: 0.08);
            Transient simulation = DemoSupport.CreateTransient(3.0, 0.005);
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var omega = new RealPropertyExport(simulation, body.Name, "omega");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,angle,wrapped_error,omega,elastic_energy");

            DemoSupport.Run(simulation, new IEntity[] { body, spring }, time =>
            {
                if (!gate.ShouldWrite(time))
                    return;

                double error = AngleMath.WrapSigned(angle.Value);
                double energy = stiffness * (1.0 - Math.Cos(angle.Value));
                DemoSupport.WriteRow(output, time, angle.Value, error, omega.Value, energy);
            });
        }
    }
}
