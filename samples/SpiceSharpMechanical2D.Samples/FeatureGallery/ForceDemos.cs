using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using System.IO;

namespace SpiceSharpMechanical2D.Samples.FeatureGallery
{
    internal static class ForceDemos
    {
        public static void Projectile(TextWriter output)
        {
            var body = new RigidBody2D(
                "projectile",
                mass: 1.0,
                inertia: 0.1,
                initialPosition: new Vector2D(0.0, 1.0),
                initialLinearVelocity: new Vector2D(3.0, 5.0));
            var gravity = new Gravity2D("gravity", body.Name, new Vector2D(0.0, -9.81));
            Transient simulation = DemoSupport.CreateTransient(1.0, 0.01);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var vx = new RealPropertyExport(simulation, body.Name, "vx");
            var vy = new RealPropertyExport(simulation, body.Name, "vy");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,x,y,vx,vy");

            RunBody(simulation, new IEntity[] { body, gravity }, gate, output, time =>
                new[] { time, x.Value, y.Value, vx.Value, vy.Value });
        }

        public static void TimeDependentForce(TextWriter output)
        {
            var body = new RigidBody2D("ramped-force-body", mass: 2.0, inertia: 0.2);
            var force = new AppliedForce2D(
                "ramp",
                body.Name,
                time => new Vector2D(4.0 * time, -2.0 * time));
            Transient simulation = DemoSupport.CreateTransient(1.0, 0.01);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var vx = new RealPropertyExport(simulation, body.Name, "vx");
            var vy = new RealPropertyExport(simulation, body.Name, "vy");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,force_x,force_y,x,y,vx,vy");

            RunBody(simulation, new IEntity[] { body, force }, gate, output, time =>
                new[] { time, 4.0 * time, -2.0 * time, x.Value, y.Value, vx.Value, vy.Value });
        }

        public static void DragDecay(TextWriter output)
        {
            var body = new RigidBody2D(
                "dragged-body",
                mass: 2.0,
                inertia: 0.5,
                initialLinearVelocity: new Vector2D(3.0, -1.0),
                initialAngularVelocity: 2.0);
            var linearDrag = new LinearDrag2D(
                "linear-drag",
                body.Name,
                damping: 1.2,
                mediumVelocity: new Vector2D(0.5, 0.0));
            var angularDrag = new AngularDrag2D(
                "angular-drag",
                body.Name,
                damping: 0.4,
                mediumAngularVelocity: 0.25);
            Transient simulation = DemoSupport.CreateTransient(3.0, 0.02);
            var vx = new RealPropertyExport(simulation, body.Name, "vx");
            var vy = new RealPropertyExport(simulation, body.Name, "vy");
            var omega = new RealPropertyExport(simulation, body.Name, "omega");
            var energy = new RealPropertyExport(simulation, body.Name, "kineticenergy");
            var gate = new SampleGate(0.25);
            output.WriteLine("time,vx,vy,omega,kinetic_energy");

            RunBody(
                simulation,
                new IEntity[] { body, linearDrag, angularDrag },
                gate,
                output,
                time => new[] { time, vx.Value, vy.Value, omega.Value, energy.Value });
        }

        public static void PointForce(TextWriter output)
        {
            var body = new RigidBody2D("lever", mass: 1.0, inertia: 0.5);
            var pointForce = new PointForce2D(
                "off-center-force",
                body.Name,
                localPoint: new Vector2D(0.75, 0.0),
                force: new Vector2D(0.0, 1.0),
                forceCoordinates: ForceCoordinateSystem2D.World);
            Transient simulation = DemoSupport.CreateTransient(1.0, 0.005);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var omega = new RealPropertyExport(simulation, body.Name, "omega");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,x,y,angle,omega");

            RunBody(simulation, new IEntity[] { body, pointForce }, gate, output, time =>
                new[] { time, x.Value, y.Value, angle.Value, omega.Value });
        }

        private static void RunBody(
            Transient simulation,
            IEntity[] entities,
            SampleGate gate,
            TextWriter output,
            System.Func<double, double[]> values)
        {
            DemoSupport.Run(simulation, entities, time =>
            {
                if (gate.ShouldWrite(time))
                    DemoSupport.WriteRow(output, values(time));
            });
        }
    }
}
