using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Physics2D.Tests.Connections
{
    internal static class ConnectionTestSimulation
    {
        public static ConnectionRun Run(
            RigidBody2D bodyA,
            RigidBody2D bodyB,
            double maximumTimestep,
            double stopTime,
            params IEntity[] connections)
        {
            var method = new Trapezoidal
            {
                InitialStep = maximumTimestep,
                MaxStep = maximumTimestep,
                StopTime = stopTime,
            };
            var simulation = new Transient("tran", method);
            var xA = new RealPropertyExport(simulation, bodyA.Name, "x");
            var yA = new RealPropertyExport(simulation, bodyA.Name, "y");
            var angleA = new RealPropertyExport(simulation, bodyA.Name, "angle");
            var vxA = new RealPropertyExport(simulation, bodyA.Name, "vx");
            var vyA = new RealPropertyExport(simulation, bodyA.Name, "vy");
            var omegaA = new RealPropertyExport(simulation, bodyA.Name, "omega");
            RealPropertyExport xB = bodyB == null
                ? null
                : new RealPropertyExport(simulation, bodyB.Name, "x");
            RealPropertyExport yB = bodyB == null
                ? null
                : new RealPropertyExport(simulation, bodyB.Name, "y");
            RealPropertyExport angleB = bodyB == null
                ? null
                : new RealPropertyExport(simulation, bodyB.Name, "angle");
            RealPropertyExport vxB = bodyB == null
                ? null
                : new RealPropertyExport(simulation, bodyB.Name, "vx");
            RealPropertyExport vyB = bodyB == null
                ? null
                : new RealPropertyExport(simulation, bodyB.Name, "vy");
            RealPropertyExport omegaB = bodyB == null
                ? null
                : new RealPropertyExport(simulation, bodyB.Name, "omega");
            var entities = new List<IEntity>
            {
                new Resistor("validation-reference", "unused", "0", 1.0),
                bodyA,
            };
            if (bodyB != null)
                entities.Add(bodyB);
            entities.AddRange(connections);
            var samples = new List<ConnectionSample>();

            foreach (int exportType in simulation.Run(new Circuit(entities.ToArray())))
            {
                if (exportType != Transient.ExportTransient)
                    continue;

                samples.Add(new ConnectionSample(
                    simulation.Time,
                    xA.Value,
                    yA.Value,
                    angleA.Value,
                    vxA.Value,
                    vyA.Value,
                    omegaA.Value,
                    xB == null ? 0.0 : xB.Value,
                    yB == null ? 0.0 : yB.Value,
                    angleB == null ? 0.0 : angleB.Value,
                    vxB == null ? 0.0 : vxB.Value,
                    vyB == null ? 0.0 : vyB.Value,
                    omegaB == null ? 0.0 : omegaB.Value));
            }

            return new ConnectionRun(samples);
        }
    }

    internal sealed class ConnectionRun
    {
        public ConnectionRun(IReadOnlyList<ConnectionSample> samples)
        {
            Samples = samples;
        }

        public IReadOnlyList<ConnectionSample> Samples { get; }

        public ConnectionSample Final => Samples.Last();
    }

    internal readonly struct ConnectionSample
    {
        public ConnectionSample(
            double time,
            double xA,
            double yA,
            double angleA,
            double velocityXA,
            double velocityYA,
            double angularVelocityA,
            double xB,
            double yB,
            double angleB,
            double velocityXB,
            double velocityYB,
            double angularVelocityB)
        {
            Time = time;
            XA = xA;
            YA = yA;
            AngleA = angleA;
            VelocityXA = velocityXA;
            VelocityYA = velocityYA;
            AngularVelocityA = angularVelocityA;
            XB = xB;
            YB = yB;
            AngleB = angleB;
            VelocityXB = velocityXB;
            VelocityYB = velocityYB;
            AngularVelocityB = angularVelocityB;
        }

        public double Time { get; }

        public double XA { get; }

        public double YA { get; }

        public double AngleA { get; }

        public double VelocityXA { get; }

        public double VelocityYA { get; }

        public double AngularVelocityA { get; }

        public double XB { get; }

        public double YB { get; }

        public double AngleB { get; }

        public double VelocityXB { get; }

        public double VelocityYB { get; }

        public double AngularVelocityB { get; }
    }
}
