using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Connections;

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
        double rawError = angleB - angleA - referenceAngle;
        double error = AngleMath.WrapSigned(rawError);
        double relativeAngularVelocity = angularVelocityB - angularVelocityA;
        double tangentStiffness = stiffness * System.Math.Cos(rawError);
        double torqueOnA = (stiffness * System.Math.Sin(rawError))
            + (damping * relativeAngularVelocity);

        loads[0] = torqueOnA;
        loads[1] = -torqueOnA;

        jacobian[0, 0] = -tangentStiffness;
        jacobian[0, 1] = -damping;
        jacobian[0, 2] = tangentStiffness;
        jacobian[0, 3] = damping;
        jacobian[1, 0] = tangentStiffness;
        jacobian[1, 1] = damping;
        jacobian[1, 2] = -tangentStiffness;
        jacobian[1, 3] = -damping;

        return new RotationalSpringDamper2DEvaluation(
            error,
            relativeAngularVelocity,
            torqueOnA);
    }
}
