using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Connections;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Physics2D.Joints;

public sealed class WeldJoint2D : Entity<WeldJoint2DParameters>
{
    public WeldJoint2D(
        string name,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        double referenceAngle,
        double positionStiffness,
        double positionDamping,
        double angularStiffness,
        double angularDamping)
        : base(name)
    {
        JointValidation.ValidateTopology(name, endpointA, endpointB);
        EndpointA = endpointA;
        EndpointB = endpointB;
        Parameters.ReferenceAngle = referenceAngle;
        Parameters.PositionStiffness = positionStiffness;
        Parameters.PositionDamping = positionDamping;
        Parameters.AngularStiffness = angularStiffness;
        Parameters.AngularDamping = angularDamping;
    }

    public MechanicalAnchor2D EndpointA { get; }

    public MechanicalAnchor2D EndpointB { get; }

    public double ReferenceAngle
    {
        get => Parameters.ReferenceAngle;
        set => Parameters.ReferenceAngle = value;
    }

    public double PositionStiffness
    {
        get => Parameters.PositionStiffness;
        set => Parameters.PositionStiffness = value;
    }

    public double PositionDamping
    {
        get => Parameters.PositionDamping;
        set => Parameters.PositionDamping = value;
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
        behaviors.Add(new WeldJoint2DBehavior(
            context,
            bodyA,
            bodyB,
            EndpointA,
            EndpointB,
            Parameters));
        simulation.EntityBehaviors.Add(behaviors);
    }
}
