using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Joints;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiceSharp.Physics2D.Tests.Joints
{
    public class MechanismVerificationTests
    {
        [Fact]
        public void SliderCrankCompletesCycleReversesAndBalancesPowerUnderPayload()
        {
            const double crankLength = 0.2;
            const double rodLength = 0.5;
            const double initialAngle = 0.45;
            const double initialOmega = 2.0;
            const double driveTorque = 0.035;
            const double payloadForce = -0.015;
            SliderCrankGeometry geometry = CreateSliderCrankGeometry(
                crankLength,
                rodLength,
                initialAngle,
                initialOmega);
            var crank = new RigidBody2D(
                "crank",
                0.25,
                0.0015,
                geometry.CrankCenter,
                initialAngle,
                geometry.CrankVelocity,
                initialOmega);
            var rod = new RigidBody2D(
                "rod",
                0.35,
                0.008,
                geometry.RodCenter,
                geometry.RodAngle,
                geometry.RodVelocity,
                geometry.RodOmega);
            var slider = new RigidBody2D(
                "slider",
                0.5,
                0.01,
                new Vector2D(geometry.SliderX, 0.0),
                0.0,
                new Vector2D(geometry.SliderVelocityX, 0.0));
            var joints = new IEntity[]
            {
                Revolute(
                    "ground-crank",
                    MechanicalAnchor2D.World(Vector2D.Zero),
                    MechanicalAnchor2D.Body(crank.Name, new Vector2D(-crankLength / 2.0, 0.0))),
                Revolute(
                    "crank-rod",
                    MechanicalAnchor2D.Body(crank.Name, new Vector2D(crankLength / 2.0, 0.0)),
                    MechanicalAnchor2D.Body(rod.Name, new Vector2D(-rodLength / 2.0, 0.0))),
                Revolute(
                    "rod-slider",
                    MechanicalAnchor2D.Body(rod.Name, new Vector2D(rodLength / 2.0, 0.0)),
                    MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero)),
                new PrismaticJoint2D(
                    "slider-guide",
                    MechanicalAnchor2D.World(Vector2D.Zero),
                    MechanicalAnchor2D.Body(slider.Name, Vector2D.Zero),
                    new Vector2D(1.0, 0.0),
                    0.0,
                    6.0e4,
                    90.0,
                    2.0e4,
                    45.0),
            };
            var entities = new List<IEntity>(joints)
            {
                new AppliedTorque2D("drive", crank.Name, driveTorque),
                new AppliedForce2D(
                    "payload",
                    slider.Name,
                    new Vector2D(payloadForce, 0.0)),
            };
            MechanismRun run = RunMechanism(
                new[]
                {
                    new BodyDefinition(crank, 0.25, 0.0015),
                    new BodyDefinition(rod, 0.35, 0.008),
                    new BodyDefinition(slider, 0.5, 0.01),
                },
                entities,
                joints,
                crank.Name,
                slider.Name,
                0.00075,
                3.5);
            double crankTravel = run.Samples.Last().CrankAngle - run.Samples.First().CrankAngle;
            int reversals = CountVelocityReversals(run.Samples);
            double maximumError = run.Samples.Max(sample => sample.MaximumJointError);
            double externalWork = Integrate(
                run.Samples,
                sample => (driveTorque * sample.CrankOmega)
                    + (payloadForce * sample.OutputVelocityX));
            double dissipatedWork = Integrate(run.Samples, sample => sample.DissipatedPower);
            double initialMechanicalEnergy = run.Samples.First().KineticEnergy
                + run.Samples.First().ElasticEnergy;
            double finalMechanicalEnergy = run.Samples.Last().KineticEnergy
                + run.Samples.Last().ElasticEnergy;
            double balanceResidual = (finalMechanicalEnergy - initialMechanicalEnergy)
                - externalWork
                + dissipatedWork;
            double balanceScale = Math.Max(
                1e-6,
                Math.Max(Math.Abs(externalWork), Math.Abs(finalMechanicalEnergy - initialMechanicalEnergy)));
            double relativeBalanceResidual = Math.Abs(balanceResidual) / balanceScale;

            Console.WriteLine(FormattableString.Invariant(
                $"Slider-crank travel/reversals/max error/power residual={crankTravel:R} rad/{reversals}/{maximumError:R} m/{relativeBalanceResidual:R}."));
            Assert.True(crankTravel > 2.0 * Math.PI, "The crank must complete at least one cycle.");
            Assert.True(reversals >= 2, "The slider must reverse at both dead centers.");
            Assert.InRange(maximumError / rodLength, 0.0, 1e-3);
            Assert.InRange(relativeBalanceResidual, 0.0, 3e-2);
        }

        [Fact]
        public void FourBarCompletesCycleWithBoundedClosureError()
        {
            const double groundLength = 0.6;
            const double crankLength = 0.2;
            const double couplerLength = 0.5;
            const double rockerLength = 0.4;
            const double initialAngle = 0.6;
            const double initialOmega = 1.8;
            FourBarGeometry geometry = CreateFourBarGeometry(
                groundLength,
                crankLength,
                couplerLength,
                rockerLength,
                initialAngle,
                initialOmega);
            var crank = new RigidBody2D(
                "fourbar-crank",
                0.2,
                0.001,
                geometry.CrankCenter,
                initialAngle,
                geometry.CrankVelocity,
                initialOmega);
            var coupler = new RigidBody2D(
                "coupler",
                0.3,
                0.006,
                geometry.CouplerCenter,
                geometry.CouplerAngle,
                geometry.CouplerVelocity,
                geometry.CouplerOmega);
            var rocker = new RigidBody2D(
                "rocker",
                0.25,
                0.003,
                geometry.RockerCenter,
                geometry.RockerAngle,
                geometry.RockerVelocity,
                geometry.RockerOmega);
            Vector2D groundRocker = new Vector2D(groundLength, 0.0);
            var joints = new IEntity[]
            {
                Revolute(
                    "fourbar-ground-crank",
                    MechanicalAnchor2D.World(Vector2D.Zero),
                    MechanicalAnchor2D.Body(crank.Name, new Vector2D(-crankLength / 2.0, 0.0))),
                Revolute(
                    "fourbar-crank-coupler",
                    MechanicalAnchor2D.Body(crank.Name, new Vector2D(crankLength / 2.0, 0.0)),
                    MechanicalAnchor2D.Body(coupler.Name, new Vector2D(-couplerLength / 2.0, 0.0))),
                Revolute(
                    "fourbar-coupler-rocker",
                    MechanicalAnchor2D.Body(coupler.Name, new Vector2D(couplerLength / 2.0, 0.0)),
                    MechanicalAnchor2D.Body(rocker.Name, new Vector2D(rockerLength / 2.0, 0.0))),
                Revolute(
                    "fourbar-rocker-ground",
                    MechanicalAnchor2D.Body(rocker.Name, new Vector2D(-rockerLength / 2.0, 0.0)),
                    MechanicalAnchor2D.World(groundRocker)),
            };
            var entities = new List<IEntity>(joints)
            {
                new AppliedTorque2D("fourbar-drive", crank.Name, 0.025),
            };
            MechanismRun run = RunMechanism(
                new[]
                {
                    new BodyDefinition(crank, 0.2, 0.001),
                    new BodyDefinition(coupler, 0.3, 0.006),
                    new BodyDefinition(rocker, 0.25, 0.003),
                },
                entities,
                joints,
                crank.Name,
                coupler.Name,
                0.00075,
                3.5);
            double crankTravel = run.Samples.Last().CrankAngle - run.Samples.First().CrankAngle;
            double maximumError = run.Samples.Max(sample => sample.MaximumJointError);

            Console.WriteLine(FormattableString.Invariant(
                $"Four-bar crank travel/maximum closure error={crankTravel:R} rad/{maximumError:R} m."));
            Assert.True(crankTravel > 2.0 * Math.PI, "The four-bar crank must complete one cycle.");
            Assert.InRange(maximumError / couplerLength, 0.0, 1e-3);
        }

        private static RevoluteJoint2D Revolute(
            string name,
            MechanicalAnchor2D endpointA,
            MechanicalAnchor2D endpointB) =>
            new RevoluteJoint2D(name, endpointA, endpointB, 6.0e4, 90.0);

        private static MechanismRun RunMechanism(
            IReadOnlyList<BodyDefinition> bodies,
            IReadOnlyList<IEntity> mechanismEntities,
            IReadOnlyList<IEntity> joints,
            string crankName,
            string outputName,
            double maximumTimestep,
            double stopTime)
        {
            var method = new Trapezoidal
            {
                InitialStep = maximumTimestep,
                MaxStep = maximumTimestep,
                StopTime = stopTime,
            };
            var simulation = new Transient("mechanism", method);
            var bodyExports = bodies.Select(body => new BodyExports(simulation, body)).ToArray();
            var crankAngle = new RealPropertyExport(simulation, crankName, "angle");
            var crankOmega = new RealPropertyExport(simulation, crankName, "omega");
            var outputX = new RealPropertyExport(simulation, outputName, "x");
            var outputVx = new RealPropertyExport(simulation, outputName, "vx");
            JointExports[] jointExports = joints.Select(joint =>
                new JointExports(simulation, joint)).ToArray();
            var entities = new List<IEntity>();
            entities.AddRange(bodies.Select(body => body.Body));
            entities.AddRange(mechanismEntities);
            var samples = new List<MechanismSample>();

            foreach (int exportType in simulation.Run(new Circuit(entities.ToArray())))
            {
                if (exportType != Transient.ExportTransient)
                    continue;

                samples.Add(new MechanismSample(
                    simulation.Time,
                    crankAngle.Value,
                    crankOmega.Value,
                    outputX.Value,
                    outputVx.Value,
                    bodyExports.Sum(export => export.KineticEnergy),
                    jointExports.Sum(export => export.ElasticEnergy.Value),
                    jointExports.Sum(export => export.DissipatedPower.Value),
                    jointExports.Max(export => export.Error)));
            }

            return new MechanismRun(samples);
        }

        private static int CountVelocityReversals(IReadOnlyList<MechanismSample> samples)
        {
            int reversals = 0;
            int previousSign = 0;
            foreach (MechanismSample sample in samples)
            {
                int sign = sample.OutputVelocityX > 1e-5
                    ? 1
                    : sample.OutputVelocityX < -1e-5 ? -1 : 0;
                if (sign == 0)
                    continue;
                if (previousSign != 0 && sign != previousSign)
                    reversals++;
                previousSign = sign;
            }

            return reversals;
        }

        private static double Integrate(
            IReadOnlyList<MechanismSample> samples,
            Func<MechanismSample, double> value)
        {
            double integral = 0.0;
            for (int index = 1; index < samples.Count; index++)
            {
                double step = samples[index].Time - samples[index - 1].Time;
                integral += 0.5 * step * (value(samples[index - 1]) + value(samples[index]));
            }

            return integral;
        }

        private static SliderCrankGeometry CreateSliderCrankGeometry(
            double crankLength,
            double rodLength,
            double angle,
            double omega)
        {
            Vector2D pin = crankLength * new Vector2D(Math.Cos(angle), Math.Sin(angle));
            double horizontalRod = Math.Sqrt(
                (rodLength * rodLength) - (pin.Y * pin.Y));
            double sliderX = pin.X + horizontalRod;
            Vector2D sliderVelocity = new Vector2D(
                omega * (-crankLength * Math.Sin(angle)
                    - ((crankLength * crankLength * Math.Sin(angle) * Math.Cos(angle))
                        / horizontalRod)),
                0.0);
            Vector2D pinVelocity = omega * pin.Perpendicular();
            Vector2D rodVector = new Vector2D(sliderX, 0.0) - pin;
            double rodOmega = Vector2D.Cross(
                rodVector,
                sliderVelocity - pinVelocity) / rodVector.LengthSquared;
            return new SliderCrankGeometry(
                pin / 2.0,
                omega * (pin / 2.0).Perpendicular(),
                (pin + new Vector2D(sliderX, 0.0)) / 2.0,
                Math.Atan2(rodVector.Y, rodVector.X),
                (pinVelocity + sliderVelocity) / 2.0,
                rodOmega,
                sliderX,
                sliderVelocity.X);
        }

        private static FourBarGeometry CreateFourBarGeometry(
            double groundLength,
            double crankLength,
            double couplerLength,
            double rockerLength,
            double angle,
            double omega)
        {
            Vector2D pin = crankLength * new Vector2D(Math.Cos(angle), Math.Sin(angle));
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
            Vector2D pinVelocity = omega * pin.Perpendicular();
            Vector2D rhs = -pinVelocity;
            Vector2D columnA = couplerVector.Perpendicular();
            Vector2D columnB = -rockerVector.Perpendicular();
            double determinant = Vector2D.Cross(columnA, columnB);
            double couplerOmega = Vector2D.Cross(rhs, columnB) / determinant;
            double rockerOmega = Vector2D.Cross(columnA, rhs) / determinant;
            Vector2D pointVelocity = rockerOmega * rockerVector.Perpendicular();
            return new FourBarGeometry(
                pin / 2.0,
                omega * (pin / 2.0).Perpendicular(),
                (pin + couplerRocker) / 2.0,
                Math.Atan2(couplerVector.Y, couplerVector.X),
                (pinVelocity + pointVelocity) / 2.0,
                couplerOmega,
                (ground + couplerRocker) / 2.0,
                Math.Atan2(rockerVector.Y, rockerVector.X),
                pointVelocity / 2.0,
                rockerOmega);
        }

        private sealed class BodyExports
        {
            private readonly BodyDefinition _definition;
            private readonly RealPropertyExport _velocityX;
            private readonly RealPropertyExport _velocityY;
            private readonly RealPropertyExport _omega;

            public BodyExports(Transient simulation, BodyDefinition definition)
            {
                _definition = definition;
                _velocityX = new RealPropertyExport(simulation, definition.Body.Name, "vx");
                _velocityY = new RealPropertyExport(simulation, definition.Body.Name, "vy");
                _omega = new RealPropertyExport(simulation, definition.Body.Name, "omega");
            }

            public double KineticEnergy =>
                0.5 * _definition.Mass
                    * ((_velocityX.Value * _velocityX.Value) + (_velocityY.Value * _velocityY.Value))
                + (0.5 * _definition.Inertia * _omega.Value * _omega.Value);
        }

        private sealed class JointExports
        {
            private readonly RealPropertyExport _errorX;
            private readonly RealPropertyExport _errorY;
            private readonly RealPropertyExport _normalError;

            public JointExports(Transient simulation, IEntity joint)
            {
                ElasticEnergy = new RealPropertyExport(simulation, joint.Name, "storedelasticenergy");
                DissipatedPower = new RealPropertyExport(simulation, joint.Name, "dissipatedpower");
                if (joint is PrismaticJoint2D)
                {
                    _normalError = new RealPropertyExport(simulation, joint.Name, "normalerror");
                }
                else
                {
                    _errorX = new RealPropertyExport(simulation, joint.Name, "anchorerrorx");
                    _errorY = new RealPropertyExport(simulation, joint.Name, "anchorerrory");
                }
            }

            public RealPropertyExport ElasticEnergy { get; }

            public RealPropertyExport DissipatedPower { get; }

            public double Error => _normalError != null
                ? Math.Abs(_normalError.Value)
                : Math.Sqrt((_errorX.Value * _errorX.Value) + (_errorY.Value * _errorY.Value));
        }

        private readonly struct BodyDefinition
        {
            public BodyDefinition(RigidBody2D body, double mass, double inertia)
            {
                Body = body;
                Mass = mass;
                Inertia = inertia;
            }

            public RigidBody2D Body { get; }
            public double Mass { get; }
            public double Inertia { get; }
        }

        private sealed class MechanismRun
        {
            public MechanismRun(IReadOnlyList<MechanismSample> samples) => Samples = samples;
            public IReadOnlyList<MechanismSample> Samples { get; }
        }

        private readonly struct MechanismSample
        {
            public MechanismSample(
                double time,
                double crankAngle,
                double crankOmega,
                double outputX,
                double outputVelocityX,
                double kineticEnergy,
                double elasticEnergy,
                double dissipatedPower,
                double maximumJointError)
            {
                Time = time;
                CrankAngle = crankAngle;
                CrankOmega = crankOmega;
                OutputX = outputX;
                OutputVelocityX = outputVelocityX;
                KineticEnergy = kineticEnergy;
                ElasticEnergy = elasticEnergy;
                DissipatedPower = dissipatedPower;
                MaximumJointError = maximumJointError;
            }

            public double Time { get; }
            public double CrankAngle { get; }
            public double CrankOmega { get; }
            public double OutputX { get; }
            public double OutputVelocityX { get; }
            public double KineticEnergy { get; }
            public double ElasticEnergy { get; }
            public double DissipatedPower { get; }
            public double MaximumJointError { get; }
        }

        private readonly struct SliderCrankGeometry
        {
            public SliderCrankGeometry(
                Vector2D crankCenter,
                Vector2D crankVelocity,
                Vector2D rodCenter,
                double rodAngle,
                Vector2D rodVelocity,
                double rodOmega,
                double sliderX,
                double sliderVelocityX)
            {
                CrankCenter = crankCenter;
                CrankVelocity = crankVelocity;
                RodCenter = rodCenter;
                RodAngle = rodAngle;
                RodVelocity = rodVelocity;
                RodOmega = rodOmega;
                SliderX = sliderX;
                SliderVelocityX = sliderVelocityX;
            }

            public Vector2D CrankCenter { get; }
            public Vector2D CrankVelocity { get; }
            public Vector2D RodCenter { get; }
            public double RodAngle { get; }
            public Vector2D RodVelocity { get; }
            public double RodOmega { get; }
            public double SliderX { get; }
            public double SliderVelocityX { get; }
        }

        private readonly struct FourBarGeometry
        {
            public FourBarGeometry(
                Vector2D crankCenter,
                Vector2D crankVelocity,
                Vector2D couplerCenter,
                double couplerAngle,
                Vector2D couplerVelocity,
                double couplerOmega,
                Vector2D rockerCenter,
                double rockerAngle,
                Vector2D rockerVelocity,
                double rockerOmega)
            {
                CrankCenter = crankCenter;
                CrankVelocity = crankVelocity;
                CouplerCenter = couplerCenter;
                CouplerAngle = couplerAngle;
                CouplerVelocity = couplerVelocity;
                CouplerOmega = couplerOmega;
                RockerCenter = rockerCenter;
                RockerAngle = rockerAngle;
                RockerVelocity = rockerVelocity;
                RockerOmega = rockerOmega;
            }

            public Vector2D CrankCenter { get; }
            public Vector2D CrankVelocity { get; }
            public Vector2D CouplerCenter { get; }
            public double CouplerAngle { get; }
            public Vector2D CouplerVelocity { get; }
            public double CouplerOmega { get; }
            public Vector2D RockerCenter { get; }
            public double RockerAngle { get; }
            public Vector2D RockerVelocity { get; }
            public double RockerOmega { get; }
        }
    }
}
