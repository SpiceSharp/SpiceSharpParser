using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Joints;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using System;
using System.IO;

namespace SpiceSharp.Physics2D.Samples.FeatureGallery
{
    internal static class JointDemos
    {
        public static void RevolutePendulum(TextWriter output)
        {
            const double length = 0.6;
            const double initialAngle = 0.25;
            var localPivot = new Vector2D(0.0, length);
            var body = new RigidBody2D(
                "pendulum",
                mass: 1.0,
                inertia: 0.12,
                initialPosition: -localPivot.Rotate(initialAngle),
                initialAngle: initialAngle);
            var pivot = new RevoluteJoint2D(
                "pivot",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, localPivot),
                stiffness: 1.0e5,
                damping: 90.0);
            var gravity = new Gravity2D("gravity", body.Name, new Vector2D(0.0, -9.81));
            Transient simulation = DemoSupport.CreateTransient(3.0, 0.001);
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var errorX = new RealPropertyExport(simulation, pivot.Name, "anchorerrorx");
            var errorY = new RealPropertyExport(simulation, pivot.Name, "anchorerrory");
            var forceX = new RealPropertyExport(simulation, pivot.Name, "forceonbx");
            var forceY = new RealPropertyExport(simulation, pivot.Name, "forceonby");
            var elasticEnergy = new RealPropertyExport(simulation, pivot.Name, "storedelasticenergy");
            var dissipatedPower = new RealPropertyExport(simulation, pivot.Name, "dissipatedpower");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,angle,anchor_error,reaction_force,elastic_energy,dissipated_power");

            DemoSupport.Run(simulation, new IEntity[] { body, pivot, gravity }, time =>
            {
                if (!gate.ShouldWrite(time))
                    return;

                DemoSupport.WriteRow(
                    output,
                    time,
                    angle.Value,
                    Hypot(errorX.Value, errorY.Value),
                    Hypot(forceX.Value, forceY.Value),
                    elasticEnergy.Value,
                    dissipatedPower.Value);
            });
        }

        public static void WeldFixture(TextWriter output)
        {
            var body = new RigidBody2D("fixture", mass: 1.0, inertia: 0.3);
            var weld = new WeldJoint2D(
                "weld",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(body.Name, Vector2D.Zero),
                referenceAngle: 0.0,
                positionStiffness: 4000.0,
                positionDamping: 120.0,
                angularStiffness: 1000.0,
                angularDamping: 30.0);
            var force = new AppliedForce2D("force", body.Name, new Vector2D(2.0, -1.0));
            var torque = new AppliedTorque2D("torque", body.Name, 0.25);
            Transient simulation = DemoSupport.CreateTransient(1.0, 0.001);
            var x = new RealPropertyExport(simulation, body.Name, "x");
            var y = new RealPropertyExport(simulation, body.Name, "y");
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var errorX = new RealPropertyExport(simulation, weld.Name, "anchorerrorx");
            var errorY = new RealPropertyExport(simulation, weld.Name, "anchorerrory");
            var angleError = new RealPropertyExport(simulation, weld.Name, "relativeangleerror");
            var reactionX = new RealPropertyExport(simulation, weld.Name, "forceonbx");
            var reactionY = new RealPropertyExport(simulation, weld.Name, "forceonby");
            var reactionTorque = new RealPropertyExport(simulation, weld.Name, "torqueonb");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,x,y,angle,anchor_error,angle_error,reaction_x,reaction_y,reaction_torque");

            DemoSupport.Run(simulation, new IEntity[] { body, weld, force, torque }, time =>
            {
                if (gate.ShouldWrite(time))
                {
                    DemoSupport.WriteRow(
                        output,
                        time,
                        x.Value,
                        y.Value,
                        angle.Value,
                        Hypot(errorX.Value, errorY.Value),
                        angleError.Value,
                        reactionX.Value,
                        reactionY.Value,
                        reactionTorque.Value);
                }
            });
        }

        public static void PrismaticSlider(TextWriter output)
        {
            var slider = new RigidBody2D(
                "slider",
                mass: 1.0,
                inertia: 0.2,
                initialPosition: new Vector2D(0.0, 0.02),
                initialAngle: 0.015,
                initialLinearVelocity: new Vector2D(0.5, 0.0));
            var guide = new PrismaticJoint2D(
                "guide",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero),
                guideAxis: Vector2D.UnitX,
                referenceAngle: 0.0,
                normalStiffness: 3000.0,
                normalDamping: 80.0,
                angularStiffness: 800.0,
                angularDamping: 25.0);
            var axialForce = new AppliedForce2D("axial-force", slider.Name, new Vector2D(0.5, 0.0));
            Transient simulation = DemoSupport.CreateTransient(2.0, 0.002);
            var x = new RealPropertyExport(simulation, slider.Name, "x");
            var y = new RealPropertyExport(simulation, slider.Name, "y");
            var vx = new RealPropertyExport(simulation, slider.Name, "vx");
            var angle = new RealPropertyExport(simulation, slider.Name, "angle");
            var normalError = new RealPropertyExport(simulation, guide.Name, "normalerror");
            var axialTravel = new RealPropertyExport(simulation, guide.Name, "axialtravel");
            var angleError = new RealPropertyExport(simulation, guide.Name, "relativeangleerror");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,x,y,vx,angle,axial_travel,normal_error,angle_error");

            DemoSupport.Run(simulation, new IEntity[] { slider, guide, axialForce }, time =>
            {
                if (gate.ShouldWrite(time))
                {
                    DemoSupport.WriteRow(
                        output,
                        time,
                        x.Value,
                        y.Value,
                        vx.Value,
                        angle.Value,
                        axialTravel.Value,
                        normalError.Value,
                        angleError.Value);
                }
            });
        }

        public static void RotatingGuide(TextWriter output)
        {
            const double initialOmega = 0.8;
            const double initialTravel = 0.5;
            var carrier = new RigidBody2D(
                "carrier",
                mass: 1.0,
                inertia: 0.2,
                initialAngularVelocity: initialOmega);
            var slider = new RigidBody2D(
                "rotating-slider",
                mass: 0.4,
                inertia: 0.05,
                initialPosition: new Vector2D(initialTravel, 0.0),
                initialLinearVelocity: new Vector2D(0.15, initialOmega * initialTravel),
                initialAngularVelocity: initialOmega);
            var carrierPivot = new RevoluteJoint2D(
                "carrier-pivot",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(carrier.Name, Vector2D.Zero),
                stiffness: 4.0e4,
                damping: 80.0);
            var guide = new PrismaticJoint2D(
                "rotating-guide",
                MechanicalAnchor2D.Body(carrier.Name, Vector2D.Zero),
                MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero),
                guideAxis: Vector2D.UnitX,
                referenceAngle: 0.0,
                normalStiffness: 4.0e4,
                normalDamping: 80.0,
                angularStiffness: 2000.0,
                angularDamping: 20.0);
            var drive = new AppliedTorque2D("drive", carrier.Name, 0.02);
            Transient simulation = DemoSupport.CreateTransient(2.0, 0.001);
            var carrierAngle = new RealPropertyExport(simulation, carrier.Name, "angle");
            var sliderX = new RealPropertyExport(simulation, slider.Name, "x");
            var sliderY = new RealPropertyExport(simulation, slider.Name, "y");
            var axialTravel = new RealPropertyExport(simulation, guide.Name, "axialtravel");
            var normalError = new RealPropertyExport(simulation, guide.Name, "normalerror");
            var angleError = new RealPropertyExport(simulation, guide.Name, "relativeangleerror");
            var gate = new SampleGate(0.1);
            output.WriteLine("time,carrier_angle,guide_axis_x,guide_axis_y,slider_x,slider_y,axial_travel,normal_error,angle_error");

            DemoSupport.Run(
                simulation,
                new IEntity[] { carrier, slider, carrierPivot, guide, drive },
                time =>
                {
                    if (!gate.ShouldWrite(time))
                        return;

                    Vector2D worldAxis = Vector2D.UnitX.Rotate(carrierAngle.Value);
                    DemoSupport.WriteRow(
                        output,
                        time,
                        carrierAngle.Value,
                        worldAxis.X,
                        worldAxis.Y,
                        sliderX.Value,
                        sliderY.Value,
                        axialTravel.Value,
                        normalError.Value,
                        angleError.Value);
                });
        }

        private static double Hypot(double x, double y) => Math.Sqrt((x * x) + (y * y));
    }
}
