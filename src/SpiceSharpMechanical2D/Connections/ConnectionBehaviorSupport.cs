using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharpMechanical2D.Connections;

internal static class ConnectionBehaviorSupport
{
    public static IRigidBody2DBehavior ResolveBody(string bodyName, ISimulation simulation)
    {
        if (string.IsNullOrWhiteSpace(bodyName))
            throw new ArgumentException("A rigid-body entity name is required.", nameof(bodyName));

        return new Reference(bodyName)
            .GetContainer(simulation)
            .GetValue<IRigidBody2DBehavior>();
    }

}
