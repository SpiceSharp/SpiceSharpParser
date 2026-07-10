using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Joints;

internal readonly struct RevoluteJoint2DEvaluation
{
    public RevoluteJoint2DEvaluation(
        Vector2D anchorError,
        Vector2D anchorVelocityError,
        Vector2D forceOnA,
        double torqueOnA,
        double torqueOnB,
        double storedElasticEnergy,
        double dissipatedPower)
    {
        AnchorError = anchorError;
        AnchorVelocityError = anchorVelocityError;
        ForceOnA = forceOnA;
        TorqueOnA = torqueOnA;
        TorqueOnB = torqueOnB;
        StoredElasticEnergy = storedElasticEnergy;
        DissipatedPower = dissipatedPower;
    }

    public Vector2D AnchorError { get; }

    public Vector2D AnchorVelocityError { get; }

    public Vector2D ForceOnA { get; }

    public double TorqueOnA { get; }

    public double TorqueOnB { get; }

    public double StoredElasticEnergy { get; }

    public double DissipatedPower { get; }
}

internal static class RevoluteJoint2DEquation
{
    public static RevoluteJoint2DEvaluation Evaluate(
        MechanicalAnchor2D endpointA,
        ConnectionBodyState2D stateA,
        MechanicalAnchor2D endpointB,
        ConnectionBodyState2D stateB,
        double stiffness,
        double damping,
        double[] loads,
        double[,] jacobian)
    {
        JointEndpointKinematics a = JointEquationSupport.GetKinematics(endpointA, stateA, 0);
        JointEndpointKinematics b = JointEquationSupport.GetKinematics(endpointB, stateB, 6);
        DualVector2D error = b.Point - a.Point;
        DualVector2D velocityError = b.Velocity - a.Velocity;
        DualVector2D forceOnA = (error * stiffness) + (velocityError * damping);
        DualVector2D forceOnB = -forceOnA;
        Dual12 torqueOnA = DualVector2D.Cross(a.Radius, forceOnA);
        Dual12 torqueOnB = DualVector2D.Cross(b.Radius, forceOnB);

        JointEquationSupport.WriteLoad(0, forceOnA.X, loads, jacobian);
        JointEquationSupport.WriteLoad(1, forceOnA.Y, loads, jacobian);
        JointEquationSupport.WriteLoad(2, torqueOnA, loads, jacobian);
        JointEquationSupport.WriteLoad(3, forceOnB.X, loads, jacobian);
        JointEquationSupport.WriteLoad(4, forceOnB.Y, loads, jacobian);
        JointEquationSupport.WriteLoad(5, torqueOnB, loads, jacobian);

        Vector2D errorValue = JointEquationSupport.ToVector(error);
        Vector2D velocityErrorValue = JointEquationSupport.ToVector(velocityError);
        Vector2D forceValue = JointEquationSupport.ToVector(forceOnA);
        return new RevoluteJoint2DEvaluation(
            errorValue,
            velocityErrorValue,
            forceValue,
            torqueOnA.Value,
            torqueOnB.Value,
            0.5 * stiffness * Vector2D.Dot(errorValue, errorValue),
            damping * Vector2D.Dot(velocityErrorValue, velocityErrorValue));
    }
}
