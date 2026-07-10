using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpMechanical2D.Joints;

public sealed class PrismaticJoint2D : Entity<PrismaticJoint2DParameters>
{
    public PrismaticJoint2D(
        string name,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        Vector2D guideAxis,
        double referenceAngle,
        double normalStiffness,
        double normalDamping,
        double angularStiffness,
        double angularDamping,
        double referenceAxialTravel = 0.0,
        double axialStiffness = 0.0,
        double axialDamping = 0.0)
        : base(name)
    {
        JointValidation.ValidateTopology(name, endpointA, endpointB);
        EndpointA = endpointA;
        EndpointB = endpointB;
        GuideAxis = JointValidation.NormalizeAxis(name, guideAxis);
        Parameters.ReferenceAngle = referenceAngle;
        Parameters.NormalStiffness = normalStiffness;
        Parameters.NormalDamping = normalDamping;
        Parameters.AngularStiffness = angularStiffness;
        Parameters.AngularDamping = angularDamping;
        Parameters.ReferenceAxialTravel = referenceAxialTravel;
        Parameters.AxialStiffness = axialStiffness;
        Parameters.AxialDamping = axialDamping;
    }

    public MechanicalAnchor2D EndpointA { get; }

    public MechanicalAnchor2D EndpointB { get; }

    public Vector2D GuideAxis { get; }

    public double ReferenceAngle
    {
        get => Parameters.ReferenceAngle;
        set => Parameters.ReferenceAngle = value;
    }

    public double NormalStiffness
    {
        get => Parameters.NormalStiffness;
        set => Parameters.NormalStiffness = value;
    }

    public double NormalDamping
    {
        get => Parameters.NormalDamping;
        set => Parameters.NormalDamping = value;
    }

    public double AngularStiffness
    {
        get => Parameters.AngularStiffness;
        set => Parameters.AngularStiffness = value;
    }

    public double AngularDamping
    {
        get => Parameters.AngularDamping;
        set => Parameters.AngularDamping = value;
    }

    public double ReferenceAxialTravel
    {
        get => Parameters.ReferenceAxialTravel;
        set => Parameters.ReferenceAxialTravel = value;
    }

    public double AxialStiffness
    {
        get => Parameters.AxialStiffness;
        set => Parameters.AxialStiffness = value;
    }

    public double AxialDamping
    {
        get => Parameters.AxialDamping;
        set => Parameters.AxialDamping = value;
    }

    public override void CreateBehaviors(ISimulation simulation)
    {
        if (simulation == null)
        {
            throw new ArgumentNullException(nameof(simulation));
        }

        if (!simulation.UsesBehaviors<ITimeBehavior>())
        {
            return;
        }

        IRigidBody2DBehavior bodyA = JointValidation.ResolveBody(Name, EndpointA, simulation);
        IRigidBody2DBehavior bodyB = JointValidation.ResolveBody(Name, EndpointB, simulation);
        var behaviors = new BehaviorContainer(Name);
        var context = new BindingContext(this, simulation, behaviors);
        behaviors.Add(new PrismaticJoint2DBehavior(
            context,
            bodyA,
            bodyB,
            EndpointA,
            EndpointB,
            GuideAxis,
            Parameters));
        simulation.EntityBehaviors.Add(behaviors);
    }
}
