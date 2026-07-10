using SpiceSharp.Physics2D.Mathematics;

namespace SpiceSharp.Physics2D.Connections;

internal readonly struct RotationalSpringDamper2DEvaluation
{
    public RotationalSpringDamper2DEvaluation(
        double angleError,
        double relativeAngularVelocity,
        double torqueOnA)
    {
        AngleError = angleError;
        RelativeAngularVelocity = relativeAngularVelocity;
        TorqueOnA = torqueOnA;
    }

    public double AngleError { get; }

    public double RelativeAngularVelocity { get; }

    public double TorqueOnA { get; }
}

internal static class RotationalSpringDamper2DEquation
{
    public const int LoadCount = 2;
    public const int StateCount = 4;

    public static RotationalSpringDamper2DEvaluation Evaluate(
        double angleA,
        double angularVelocityA,
        double angleB,
        double angularVelocityB,
        double referenceAngle,
        double stiffness,
        double damping,
        double[] loads,
        double[,] jacobian)
    {
        double error = AngleMath.WrapSigned(angleB - angleA - referenceAngle);
        double relativeAngularVelocity = angularVelocityB - angularVelocityA;
        double torqueOnA = (stiffness * error) + (damping * relativeAngularVelocity);

        loads[0] = torqueOnA;
        loads[1] = -torqueOnA;

        jacobian[0, 0] = -stiffness;
        jacobian[0, 1] = -damping;
        jacobian[0, 2] = stiffness;
        jacobian[0, 3] = damping;
        jacobian[1, 0] = stiffness;
        jacobian[1, 1] = damping;
        jacobian[1, 2] = -stiffness;
        jacobian[1, 3] = -damping;

        return new RotationalSpringDamper2DEvaluation(
            error,
            relativeAngularVelocity,
            torqueOnA);
    }
}
