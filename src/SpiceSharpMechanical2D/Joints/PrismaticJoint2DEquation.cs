using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;
using System;

namespace SpiceSharpMechanical2D.Joints;

internal readonly struct PrismaticJoint2DEvaluation
{
    public PrismaticJoint2DEvaluation(
        double normalError,
        double normalVelocityError,
        double axialTravel,
        double axialVelocity,
        double relativeAngleError,
        Vector2D forceOnA,
        double torqueOnA,
        double torqueOnB,
        double storedElasticEnergy,
        double dissipatedPower)
    {
        NormalError = normalError;
        NormalVelocityError = normalVelocityError;
        AxialTravel = axialTravel;
        AxialVelocity = axialVelocity;
        RelativeAngleError = relativeAngleError;
        ForceOnA = forceOnA;
        TorqueOnA = torqueOnA;
        TorqueOnB = torqueOnB;
        StoredElasticEnergy = storedElasticEnergy;
        DissipatedPower = dissipatedPower;
    }

    public double NormalError { get; }

    public double NormalVelocityError { get; }

    public double AxialTravel { get; }

    public double AxialVelocity { get; }

    public double RelativeAngleError { get; }

    public Vector2D ForceOnA { get; }

    public double TorqueOnA { get; }

    public double TorqueOnB { get; }

    public double StoredElasticEnergy { get; }

    public double DissipatedPower { get; }
}

internal static class PrismaticJoint2DEquation
{
    public static PrismaticJoint2DEvaluation Evaluate(
        MechanicalAnchor2D endpointA,
        ConnectionBodyState2D stateA,
        MechanicalAnchor2D endpointB,
        ConnectionBodyState2D stateB,
        Vector2D guideAxis,
        double normalStiffness,
        double normalDamping,
        double referenceAngle,
        double angularStiffness,
        double angularDamping,
        double referenceAxialTravel,
        double axialStiffness,
        double axialDamping,
        double[] loads,
        double[,] jacobian)
    {
        JointEndpointKinematics a = JointEquationSupport.GetKinematics(endpointA, stateA, 0);
        JointEndpointKinematics b = JointEquationSupport.GetKinematics(endpointB, stateB, 6);
        DualVector2D axis = endpointA.IsWorld
            ? new DualVector2D(
                Dual12.Constant(guideAxis.X),
                Dual12.Constant(guideAxis.Y))
            : DualVector2D.Rotate(guideAxis.X, guideAxis.Y, a.Angle);
        DualVector2D normal = axis.Perpendicular();
        DualVector2D displacement = b.Point - a.Point;
        DualVector2D pointVelocityError = b.Velocity - a.Velocity;
        Dual12 normalError = DualVector2D.Dot(normal, displacement);
        Dual12 axialTravel = DualVector2D.Dot(axis, displacement);
        Dual12 normalVelocityError = DualVector2D.Dot(normal, pointVelocityError)
            - (a.AngularVelocity * DualVector2D.Dot(axis, displacement));
        Dual12 axialVelocity = DualVector2D.Dot(axis, pointVelocityError)
            + (a.AngularVelocity * DualVector2D.Dot(normal, displacement));
        Dual12 normalEffort = (normalError * normalStiffness)
            + (normalVelocityError * normalDamping);
        Dual12 axialError = axialTravel - Dual12.Constant(referenceAxialTravel);
        Dual12 axialEffort = (axialError * axialStiffness)
            + (axialVelocity * axialDamping);

        DualVector2D forceOnA = (normal * normalEffort) + (axis * axialEffort);
        DualVector2D forceOnB = -forceOnA;
        Dual12 normalAngleJacobianA = -DualVector2D.Dot(axis, displacement)
            - DualVector2D.Dot(normal, a.Radius.Perpendicular());
        Dual12 normalAngleJacobianB =
            DualVector2D.Dot(normal, b.Radius.Perpendicular());
        Dual12 axialAngleJacobianA = DualVector2D.Dot(normal, displacement)
            - DualVector2D.Dot(axis, a.Radius.Perpendicular());
        Dual12 axialAngleJacobianB =
            DualVector2D.Dot(axis, b.Radius.Perpendicular());
        Dual12 rawAngleError = b.Angle - a.Angle - Dual12.Constant(referenceAngle);
        Dual12 angularVelocityError = b.AngularVelocity - a.AngularVelocity;
        Dual12 angularTorque = (Dual12.Sin(rawAngleError) * angularStiffness)
            + (angularVelocityError * angularDamping);
        Dual12 torqueOnA = -(normalAngleJacobianA * normalEffort)
            - (axialAngleJacobianA * axialEffort)
            + angularTorque;
        Dual12 torqueOnB = -(normalAngleJacobianB * normalEffort)
            - (axialAngleJacobianB * axialEffort)
            - angularTorque;

        JointEquationSupport.WriteLoad(0, forceOnA.X, loads, jacobian);
        JointEquationSupport.WriteLoad(1, forceOnA.Y, loads, jacobian);
        JointEquationSupport.WriteLoad(2, torqueOnA, loads, jacobian);
        JointEquationSupport.WriteLoad(3, forceOnB.X, loads, jacobian);
        JointEquationSupport.WriteLoad(4, forceOnB.Y, loads, jacobian);
        JointEquationSupport.WriteLoad(5, torqueOnB, loads, jacobian);

        Vector2D forceValue = JointEquationSupport.ToVector(forceOnA);
        return new PrismaticJoint2DEvaluation(
            normalError.Value,
            normalVelocityError.Value,
            axialTravel.Value,
            axialVelocity.Value,
            AngleMath.WrapSigned(rawAngleError.Value),
            forceValue,
            torqueOnA.Value,
            torqueOnB.Value,
            (0.5 * normalStiffness * normalError.Value * normalError.Value)
                + (0.5 * axialStiffness * axialError.Value * axialError.Value)
                + (angularStiffness * (1.0 - Math.Cos(rawAngleError.Value))),
            (normalDamping * normalVelocityError.Value * normalVelocityError.Value)
                + (axialDamping * axialVelocity.Value * axialVelocity.Value)
                + (angularDamping * angularVelocityError.Value * angularVelocityError.Value));
    }
}
