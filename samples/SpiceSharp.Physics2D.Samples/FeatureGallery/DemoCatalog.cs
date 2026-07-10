using System;
using System.Collections.Generic;
using System.IO;

namespace SpiceSharp.Physics2D.Samples.FeatureGallery
{
    internal sealed class DemoDefinition
    {
        public DemoDefinition(string name, string description, Action<TextWriter> run)
        {
            Name = name;
            Description = description;
            Run = run;
        }

        public string Name { get; }

        public string Description { get; }

        public Action<TextWriter> Run { get; }
    }

    internal static class DemoCatalog
    {
        public static IReadOnlyList<DemoDefinition> All { get; } = new[]
        {
            new DemoDefinition("api-oscillator", "Phase 00 two-state SpiceSharp API proof", InfrastructureDemos.ApiOscillator),
            new DemoDefinition("math-tour", "Phase 01 vectors, rotations, angles, and smoothing", InfrastructureDemos.MathTour),
            new DemoDefinition("coordinate-coast", "Phase 02 generalized coordinate with constant velocity", MotionDemos.CoordinateCoast),
            new DemoDefinition("rigid-body-coast", "Phase 03 inertial translation and rotation", MotionDemos.RigidBodyCoast),
            new DemoDefinition("projectile", "Phase 04 projectile under gravity", ForceDemos.Projectile),
            new DemoDefinition("time-force", "Phase 04 deterministic time-dependent force", ForceDemos.TimeDependentForce),
            new DemoDefinition("drag-decay", "Phase 04 linear and angular drag", ForceDemos.DragDecay),
            new DemoDefinition("point-force", "Phase 04 off-center force and torque", ForceDemos.PointForce),
            new DemoDefinition("two-body-spring", "Phase 05 distance spring between two bodies", ConnectionDemos.TwoBodySpring),
            new DemoDefinition("torsional-spring", "Phase 05 smooth periodic rotational spring", ConnectionDemos.TorsionalSpring),
            new DemoDefinition("revolute-pendulum", "Phase 06 compliant revolute joint", JointDemos.RevolutePendulum),
            new DemoDefinition("weld-fixture", "Phase 06 compliant weld under force and torque", JointDemos.WeldFixture),
            new DemoDefinition("prismatic-slider", "Phase 06 free axial prismatic motion", JointDemos.PrismaticSlider),
            new DemoDefinition("rotating-guide", "Phase 06 body-attached rotating prismatic guide", JointDemos.RotatingGuide),
        };
    }
}
