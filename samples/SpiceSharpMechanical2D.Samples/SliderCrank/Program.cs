using SpiceSharp;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Forces;
using SpiceSharpMechanical2D.Joints;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharpMechanical2D.Samples.SliderCrank
{
    internal static class Program
    {
        private static void Main()
        {
            const double crankLength = 0.2;
            const double rodLength = 0.5;
            const double initialAngle = 0.45;
            const double initialOmega = 2.0;
            Vector2D pin = crankLength * new Vector2D(
                Math.Cos(initialAngle),
                Math.Sin(initialAngle));
            double horizontalRod = Math.Sqrt(
                (rodLength * rodLength) - (pin.Y * pin.Y));
            double sliderX = pin.X + horizontalRod;
            var sliderVelocity = new Vector2D(
                initialOmega * (-crankLength * Math.Sin(initialAngle)
                    - ((crankLength * crankLength * Math.Sin(initialAngle)
                        * Math.Cos(initialAngle)) / horizontalRod)),
                0.0);
            Vector2D pinVelocity = initialOmega * pin.Perpendicular();
            Vector2D rodVector = new Vector2D(sliderX, 0.0) - pin;
            double rodOmega = Vector2D.Cross(
                rodVector,
                sliderVelocity - pinVelocity) / rodVector.LengthSquared;
            var crank = new RigidBody2D(
                "crank",
                0.25,
                0.0015,
                pin / 2.0,
                initialAngle,
                initialOmega * (pin / 2.0).Perpendicular(),
                initialOmega);
            var rod = new RigidBody2D(
                "rod",
                0.35,
                0.008,
                (pin + new Vector2D(sliderX, 0.0)) / 2.0,
                Math.Atan2(rodVector.Y, rodVector.X),
                (pinVelocity + sliderVelocity) / 2.0,
                rodOmega);
            var slider = new RigidBody2D(
                "slider",
                0.5,
                0.01,
                new Vector2D(sliderX, 0.0),
                initialLinearVelocity: sliderVelocity);
            var groundCrank = Revolute(
                "ground-crank",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(crank.Name, new Vector2D(-crankLength / 2.0, 0.0)));
            var crankRod = Revolute(
                "crank-rod",
                MechanicalAnchor2D.Body(crank.Name, new Vector2D(crankLength / 2.0, 0.0)),
                MechanicalAnchor2D.Body(rod.Name, new Vector2D(-rodLength / 2.0, 0.0)));
            var rodSlider = Revolute(
                "rod-slider",
                MechanicalAnchor2D.Body(rod.Name, new Vector2D(rodLength / 2.0, 0.0)),
                MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero));
            var guide = new PrismaticJoint2D(
                "slider-guide",
                MechanicalAnchor2D.World(Vector2D.Zero),
                MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero),
                new Vector2D(1.0, 0.0),
                referenceAngle: 0.0,
                normalStiffness: 6.0e4,
                normalDamping: 90.0,
                angularStiffness: 2.0e4,
                angularDamping: 45.0);
            var method = new Trapezoidal
            {
                InitialStep = 0.00075,
                MaxStep = 0.00075,
                StopTime = 3.5,
            };
            var simulation = new Transient("slider-crank", method);
            var crankAngle = new RealPropertyExport(simulation, crank.Name, "angle");
            var sliderPosition = new RealPropertyExport(simulation, slider.Name, "x");
            var sliderVelocityX = new RealPropertyExport(simulation, slider.Name, "vx");
            var guideError = new RealPropertyExport(simulation, guide.Name, "normalerror");
            var crankRodErrorX = new RealPropertyExport(simulation, crankRod.Name, "anchorerrorx");
            var crankRodErrorY = new RealPropertyExport(simulation, crankRod.Name, "anchorerrory");
            var circuit = new Circuit(
                crank,
                rod,
                slider,
                groundCrank,
                crankRod,
                rodSlider,
                guide,
                new AppliedTorque2D("drive", crank.Name, 0.035),
                new AppliedForce2D("payload", slider.Name, new Vector2D(-0.015, 0.0)));

            Console.WriteLine("time,crank_angle,slider_x,slider_vx,guide_error,crank_rod_error");
            foreach (int exportType in simulation.Run(circuit))
            {
                if (exportType != Transient.ExportTransient)
                    continue;

                double crankRodError = Math.Sqrt(
                    (crankRodErrorX.Value * crankRodErrorX.Value)
                    + (crankRodErrorY.Value * crankRodErrorY.Value));
                Console.WriteLine(FormattableString.Invariant(
                    $"{simulation.Time:R},{crankAngle.Value:R},{sliderPosition.Value:R},{sliderVelocityX.Value:R},{guideError.Value:R},{crankRodError:R}"));
            }
        }

        private static RevoluteJoint2D Revolute(
            string name,
            MechanicalAnchor2D endpointA,
            MechanicalAnchor2D endpointB) =>
            new RevoluteJoint2D(name, endpointA, endpointB, 6.0e4, 90.0);
    }
}
