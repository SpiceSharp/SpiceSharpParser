using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpMechanical2D.Tests.Forces
{
    internal static class ForceTestSimulation
    {
        public static BodyRun Run(
            RigidBody2D body,
            double maximumTimestep,
            double stopTime,
            params IEntity[] loads)
        {
            var method = new Trapezoidal
            {
                InitialStep = maximumTimestep,
                MaxStep = maximumTimestep,
                StopTime = stopTime,
            };
            var simulation = new Transient("tran", method);
            var positionX = new RealPropertyExport(simulation, body.Name, "positionx");
            var positionY = new RealPropertyExport(simulation, body.Name, "positiony");
            var angle = new RealPropertyExport(simulation, body.Name, "angle");
            var velocityX = new RealPropertyExport(simulation, body.Name, "velocityx");
            var velocityY = new RealPropertyExport(simulation, body.Name, "velocityy");
            var angularVelocity = new RealPropertyExport(
                simulation,
                body.Name,
                "angularvelocity");
            var kineticEnergy = new RealPropertyExport(
                simulation,
                body.Name,
                "kineticenergy");
            var entities = new List<IEntity>
            {
                body,
            };
            entities.AddRange(loads);
            var samples = new List<BodySample>();

            foreach (int exportType in simulation.Run(new Circuit(entities.ToArray())))
            {
                if (exportType == Transient.ExportOperatingPoint
                    || exportType == Transient.ExportTransient)
                {
                    samples.Add(new BodySample(
                        exportType,
                        simulation.Time,
                        positionX.Value,
                        positionY.Value,
                        angle.Value,
                        velocityX.Value,
                        velocityY.Value,
                        angularVelocity.Value,
                        kineticEnergy.Value));
                }
            }

            return new BodyRun(simulation, samples);
        }
    }

    internal sealed class BodyRun
    {
        public BodyRun(Transient simulation, IReadOnlyList<BodySample> samples)
        {
            Simulation = simulation;
            Samples = samples;
        }

        public Transient Simulation { get; }

        public IReadOnlyList<BodySample> Samples { get; }

        public IReadOnlyList<BodySample> TransientSamples => Samples
            .Where(sample => sample.ExportType == Transient.ExportTransient)
            .ToArray();

        public BodySample FinalTransient => Samples
            .Last(sample => sample.ExportType == Transient.ExportTransient);
    }

    internal readonly struct BodySample
    {
        public BodySample(
            int exportType,
            double time,
            double positionX,
            double positionY,
            double angle,
            double velocityX,
            double velocityY,
            double angularVelocity,
            double kineticEnergy)
        {
            ExportType = exportType;
            Time = time;
            PositionX = positionX;
            PositionY = positionY;
            Angle = angle;
            VelocityX = velocityX;
            VelocityY = velocityY;
            AngularVelocity = angularVelocity;
            KineticEnergy = kineticEnergy;
        }

        public int ExportType { get; }

        public double Time { get; }

        public double PositionX { get; }

        public double PositionY { get; }

        public double Angle { get; }

        public double VelocityX { get; }

        public double VelocityY { get; }

        public double AngularVelocity { get; }

        public double KineticEnergy { get; }

        public Vector2D Position => new Vector2D(PositionX, PositionY);

        public Vector2D LinearVelocity => new Vector2D(VelocityX, VelocityY);
    }
}
