using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using System;

namespace SpiceSharpMechanical2D.Joints;

internal readonly struct WeldJoint2DEvaluation
{
    public WeldJoint2DEvaluation(
        Vector2D anchorError,
        Vector2D anchorVelocityError,
        double relativeAngleError,
        Vector2D forceOnA,
        double torqueOnA,
        double torqueOnB,
        double storedElasticEnergy,
        double dissipatedPower)
    {
        AnchorError = anchorError;
        AnchorVelocityError = anchorVelocityError;
        RelativeAngleError = relativeAngleError;
        ForceOnA = forceOnA;
        TorqueOnA = torqueOnA;
        TorqueOnB = torqueOnB;
        StoredElasticEnergy = storedElasticEnergy;
        DissipatedPower = dissipatedPower;
    }

    public Vector2D AnchorError { get; }

    public Vector2D AnchorVelocityError { get; }

    public double RelativeAngleError { get; }

    public Vector2D ForceOnA { get; }

    public double TorqueOnA { get; }

    public double TorqueOnB { get; }

    public double StoredElasticEnergy { get; }

    public double DissipatedPower { get; }
}

internal static class WeldJoint2DEquation
{
    public static WeldJoint2DEvaluation Evaluate(
        MechanicalAnchor2D endpointA,
        ConnectionBodyState2D stateA,
        MechanicalAnchor2D endpointB,
        ConnectionBodyState2D stateB,
        double positionStiffness,
        double positionDamping,
        double referenceAngle,
        double angularStiffness,
        double angularDamping,
        double[] loads,
        double[,] jacobian)
    {
        JointEndpointKinematics a = JointEquationSupport.GetKinematics(endpointA, stateA, 0);
        JointEndpointKinematics b = JointEquationSupport.GetKinematics(endpointB, stateB, 6);
        DualVector2D anchorError = b.Point - a.Point;
        DualVector2D anchorVelocityError = b.Velocity - a.Velocity;
        DualVector2D forceOnA = (anchorError * positionStiffness)
            + (anchorVelocityError * positionDamping);
        DualVector2D forceOnB = -forceOnA;
        Dual12 rawAngleError = b.Angle - a.Angle - Dual12.Constant(referenceAngle);
        Dual12 angularVelocityError = b.AngularVelocity - a.AngularVelocity;
        Dual12 angularTorque = (Dual12.Sin(rawAngleError) * angularStiffness)
            + (angularVelocityError * angularDamping);
        Dual12 torqueOnA = DualVector2D.Cross(a.Radius, forceOnA) + angularTorque;
        Dual12 torqueOnB = DualVector2D.Cross(b.Radius, forceOnB) - angularTorque;

        JointEquationSupport.WriteLoad(0, forceOnA.X, loads, jacobian);
        JointEquationSupport.WriteLoad(1, forceOnA.Y, loads, jacobian);
        JointEquationSupport.WriteLoad(2, torqueOnA, loads, jacobian);
        JointEquationSupport.WriteLoad(3, forceOnB.X, loads, jacobian);
        JointEquationSupport.WriteLoad(4, forceOnB.Y, loads, jacobian);
        JointEquationSupport.WriteLoad(5, torqueOnB, loads, jacobian);

        Vector2D anchorErrorValue = JointEquationSupport.ToVector(anchorError);
        Vector2D anchorVelocityErrorValue = JointEquationSupport.ToVector(anchorVelocityError);
        Vector2D forceValue = JointEquationSupport.ToVector(forceOnA);
        return new WeldJoint2DEvaluation(
            anchorErrorValue,
            anchorVelocityErrorValue,
            AngleMath.WrapSigned(rawAngleError.Value),
            forceValue,
            torqueOnA.Value,
            torqueOnB.Value,
            (0.5 * positionStiffness * Vector2D.Dot(anchorErrorValue, anchorErrorValue))
                + (angularStiffness * (1.0 - Math.Cos(rawAngleError.Value))),
            (positionDamping * Vector2D.Dot(anchorVelocityErrorValue, anchorVelocityErrorValue))
                + (angularDamping * angularVelocityError.Value * angularVelocityError.Value));
    }
}
