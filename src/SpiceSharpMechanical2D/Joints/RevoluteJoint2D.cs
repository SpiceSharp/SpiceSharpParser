using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpMechanical2D.Joints;

public sealed class RevoluteJoint2D : Entity<RevoluteJoint2DParameters>
{
    public RevoluteJoint2D(
        string name,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB,
        double stiffness,
        double damping = 0.0)
        : base(name)
    {
        JointValidation.ValidateTopology(name, endpointA, endpointB);
        EndpointA = endpointA;
        EndpointB = endpointB;
        Parameters.Stiffness = stiffness;
        Parameters.Damping = damping;
    }

    public MechanicalAnchor2D EndpointA { get; }

    public MechanicalAnchor2D EndpointB { get; }

    public double Stiffness
    {
        get => Parameters.Stiffness;
        set => Parameters.Stiffness = value;
    }

    public double Damping
    {
        get => Parameters.Damping;
        set => Parameters.Damping = value;
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
        behaviors.Add(new RevoluteJoint2DBehavior(
            context,
            bodyA,
            bodyB,
            EndpointA,
            EndpointB,
            Parameters));
        simulation.EntityBehaviors.Add(behaviors);
    }
}
