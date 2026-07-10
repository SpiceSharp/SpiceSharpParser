using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharpMechanical2D.Joints;

internal static class JointValidation
{
    public static void ValidateTopology(
        string jointName,
        MechanicalAnchor2D endpointA,
        MechanicalAnchor2D endpointB)
    {
        endpointA.Validate(nameof(endpointA));
        endpointB.Validate(nameof(endpointB));
        if (endpointA.IsWorld && endpointB.IsWorld)
        {
            throw new ArgumentException(
                $"Joint '{jointName}' requires at least one rigid-body endpoint.");
        }

        if (!endpointA.IsWorld
            && !endpointB.IsWorld
            && string.Equals(endpointA.BodyName, endpointB.BodyName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Joint '{jointName}' cannot connect rigid body '{endpointA.BodyName}' to itself.");
        }
    }

    public static IRigidBody2DBehavior ResolveBody(
        string jointName,
        MechanicalAnchor2D endpoint,
        ISimulation simulation)
    {
        if (endpoint.IsWorld)
        {
            return null;
        }

        try
        {
            return new Reference(endpoint.BodyName)
                .GetContainer(simulation)
                .GetValue<IRigidBody2DBehavior>();
        }
        catch (Exception exception)
        {
            throw new SpiceSharpException(
                $"Joint '{jointName}' could not resolve rigid body '{endpoint.BodyName}'.",
                exception);
        }
    }

    public static Vector2D NormalizeAxis(string jointName, Vector2D axis)
    {
        if (!IsFinite(axis.X) || !IsFinite(axis.Y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(axis),
                $"Prismatic joint '{jointName}' requires a finite guide axis.");
        }

        double scale = Math.Max(Math.Abs(axis.X), Math.Abs(axis.Y));
        if (!(scale > 0.0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(axis),
                $"Prismatic joint '{jointName}' requires a nonzero guide axis.");
        }

        double scaledX = axis.X / scale;
        double scaledY = axis.Y / scale;
        double scaledLength = Math.Sqrt((scaledX * scaledX) + (scaledY * scaledY));
        return new Vector2D(scaledX / scaledLength, scaledY / scaledLength);
    }

    public static ConnectionBodyState2D GetInitialState(IRigidBody2DBehavior body)
    {
        if (body == null)
        {
            return default;
        }

        var parameterized = body as IParameterized<RigidBody2DParameters>;
        RigidBody2DParameters parameters = parameterized?.Parameters
            ?? throw new SpiceSharpException(
                $"Rigid-body behavior '{body.Name}' does not expose its initial parameters.");
        return new ConnectionBodyState2D(
            new Vector2D(parameters.InitialPositionX, parameters.InitialPositionY),
            parameters.InitialAngle,
            new Vector2D(parameters.InitialVelocityX, parameters.InitialVelocityY),
            parameters.InitialAngularVelocity);
    }

    public static void ValidateNonnegativeFinite(double value, string parameterName)
    {
        if (!IsFinite(value) || value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "The value must be nonnegative and finite.");
        }
    }

    public static void ValidateFinite(double value, string parameterName)
    {
        if (!IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "The value must be finite.");
        }
    }

    private static bool IsFinite(double value) =>
        !double.IsNaN(value) && !double.IsInfinity(value);
}
