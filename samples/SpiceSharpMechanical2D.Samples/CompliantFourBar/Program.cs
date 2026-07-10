using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Joints;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharpMechanical2D.Samples.CompliantFourBar
{
    internal static class Program
    {
        private static void Main()
        {
            const double groundLength = 0.6;
            const double crankLength = 0.2;
            const double couplerLength = 0.5;
            const double rockerLength = 0.4;
            const double initialAngle = 0.6;
            const double initialOmega = 1.8;
            Vector2D pin = crankLength * new Vector2D(
                Math.Cos(initialAngle),
                Math.Sin(initialAngle));
            Vector2D ground = new Vector2D(groundLength, 0.0);
            Vector2D delta = ground - pin;
            double distance = delta.Length;
            double along = ((couplerLength * couplerLength) - (rockerLength * rockerLength)
                + (distance * distance)) / (2.0 * distance);
            double height = Math.Sqrt((couplerLength * couplerLength) - (along * along));
            Vector2D unit = delta / distance;
            Vector2D couplerRocker = pin + (along * unit) + (height * unit.Perpendicular());
            Vector2D couplerVector = couplerRocker - pin;
            Vector2D rockerVector = couplerRocker - ground;
            Vector2D pinVelocity = initialOmega * pin.Perpendicular();
            Vector2D columnA = couplerVector.Perpendicular();
            Vector2D columnB = -rockerVector.Perpendicular();
            Vector2D rhs = -pinVelocity;
            double determinant = Vector2D.Cross(columnA, columnB);
            double couplerOmega = Vector2D.Cross(rhs, columnB) / determinant;
            double rockerOmega = Vector2D.Cross(columnA, rhs) / determinant;
            Vector2D couplerRockerVelocity = rockerOmega * rockerVector.Perpendicular();
            var crank = new RigidBody2D(
                "crank",
                0.2,
                0.001,
                pin / 2.0,
                initialAngle,
                initialOmega * (pin / 2.0).Perpendicular(),
                initialOmega);
            var coupler = new RigidBody2D(
                "coupler",
                0.3,
                0.006,
                (pin + couplerRocker) / 2.0,
                Math.Atan2(couplerVector.Y, couplerVector.X),
                (pinVelocity + couplerRockerVelocity) / 2.0,
                couplerOmega);
            var rocker = new RigidBody2D(
                "rocker",
                0.25,
                0.003,
                (ground + couplerRocker) / 2.0,
                Math.Atan2(rockerVector.Y, rockerVector.X),
                couplerRockerVelocity / 2.0,
                rockerOmega);
            var groundCrank = Revolute(
                "ground-crank",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(crank.Name, new Vector2D(-crankLength / 2.0, 0.0)));
            var crankCoupler = Revolute(
                "crank-coupler",
                MechanicalAnchor2D.Body(crank.Name, new Vector2D(crankLength / 2.0, 0.0)),
                MechanicalAnchor2D.Body(coupler.Name, new Vector2D(-couplerLength / 2.0, 0.0)));
            var couplerRockerJoint = Revolute(
                "coupler-rocker",
                MechanicalAnchor2D.Body(coupler.Name, new Vector2D(couplerLength / 2.0, 0.0)),
                MechanicalAnchor2D.Body(rocker.Name, new Vector2D(rockerLength / 2.0, 0.0)));
            var rockerGround = Revolute(
                "rocker-ground",
                MechanicalAnchor2D.Body(rocker.Name, new Vector2D(-rockerLength / 2.0, 0.0)),
                MechanicalAnchor2D.World(ground));
            var method = new Trapezoidal
            {
                InitialStep = 0.00075,
                MaxStep = 0.00075,
                StopTime = 3.5,
            };
            var simulation = new Transient("four-bar", method);
            var crankAngle = new RealPropertyExport(simulation, crank.Name, "angle");
            var couplerAngle = new RealPropertyExport(simulation, coupler.Name, "angle");
            var rockerAngle = new RealPropertyExport(simulation, rocker.Name, "angle");
            var closureErrorX = new RealPropertyExport(
                simulation,
                couplerRockerJoint.Name,
                "anchorerrorx");
            var closureErrorY = new RealPropertyExport(
                simulation,
                couplerRockerJoint.Name,
                "anchorerrory");
            var circuit = new Circuit(
                crank,
                coupler,
                rocker,
                groundCrank,
                crankCoupler,
                couplerRockerJoint,
                rockerGround,
                new AppliedTorque2D("drive", crank.Name, 0.025));

            Console.WriteLine("time,crank_angle,coupler_angle,rocker_angle,closure_error");
            foreach (int exportType in simulation.Run(circuit))
            {
                if (exportType != Transient.ExportTransient)
                    continue;

                double closureError = Math.Sqrt(
                    (closureErrorX.Value * closureErrorX.Value)
                    + (closureErrorY.Value * closureErrorY.Value));
                Console.WriteLine(FormattableString.Invariant(
                    $"{simulation.Time:R},{crankAngle.Value:R},{couplerAngle.Value:R},{rockerAngle.Value:R},{closureError:R}"));
            }
        }

        private static RevoluteJoint2D Revolute(
            string name,
            MechanicalAnchor2D endpointA,
            MechanicalAnchor2D endpointB) =>
            new RevoluteJoint2D(name, endpointA, endpointB, 6.0e4, 90.0);
    }
}
