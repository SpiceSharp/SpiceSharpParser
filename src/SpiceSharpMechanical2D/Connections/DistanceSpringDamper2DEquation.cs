using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Connections;

internal readonly struct DistanceSpringDamper2DEvaluation
{
    public DistanceSpringDamper2DEvaluation(
        Vector2D anchorA,
        Vector2D anchorB,
        double regularizedLength,
        double normalSpeed,
        double forceMagnitude,
        Vector2D forceOnA)
    {
        AnchorA = anchorA;
        AnchorB = anchorB;
        RegularizedLength = regularizedLength;
        NormalSpeed = normalSpeed;
        ForceMagnitude = forceMagnitude;
        ForceOnA = forceOnA;
    }

    public Vector2D AnchorA { get; }

    public Vector2D AnchorB { get; }

    public double RegularizedLength { get; }

    public double NormalSpeed { get; }

    public double ForceMagnitude { get; }

    public Vector2D ForceOnA { get; }
}

internal static class DistanceSpringDamper2DEquation
{
    public const int LoadCount = 6;
    public const int StateCount = 12;

    public static DistanceSpringDamper2DEvaluation Evaluate(
        MechanicalAnchor2D endpointA,
        ConnectionBodyState2D stateA,
        MechanicalAnchor2D endpointB,
        ConnectionBodyState2D stateB,
        double restLength,
        double stiffness,
        double damping,
        double lengthRegularization,
        double[] loads,
        double[,] jacobian)
    {
        EndpointKinematics kinematicsA = GetKinematics(endpointA, stateA);
        EndpointKinematics kinematicsB = GetKinematics(endpointB, stateB);
        Vector2D separation = kinematicsB.Point - kinematicsA.Point;
        double length = SmoothFunctions.RegularizedLength(separation, lengthRegularization);
        Vector2D direction = separation / length;
        Vector2D relativeVelocity = kinematicsB.Velocity - kinematicsA.Velocity;
        double normalSpeed = Vector2D.Dot(direction, relativeVelocity);
        double forceMagnitude = (stiffness * (length - restLength)) + (damping * normalSpeed);
        Vector2D forceOnA = direction * forceMagnitude;
        Vector2D forceOnB = -forceOnA;

        loads[0] = forceOnA.X;
        loads[1] = forceOnA.Y;
        loads[2] = Vector2D.Cross(kinematicsA.Radius, forceOnA);
        loads[3] = forceOnB.X;
        loads[4] = forceOnB.Y;
        loads[5] = Vector2D.Cross(kinematicsB.Radius, forceOnB);

        for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
        {
            bool belongsToA = stateIndex < 6;
            int localStateIndex = belongsToA ? stateIndex : stateIndex - 6;
            EndpointDifferential differentialA = belongsToA
                ? GetDifferential(endpointA, stateA, kinematicsA.Radius, localStateIndex)
                : default;
            EndpointDifferential differentialB = belongsToA
                ? default
                : GetDifferential(endpointB, stateB, kinematicsB.Radius, localStateIndex);

            Vector2D separationDerivative = differentialB.Point - differentialA.Point;
            double lengthDerivative = Vector2D.Dot(direction, separationDerivative);
            Vector2D directionDerivative =
                (separationDerivative - (direction * lengthDerivative)) / length;
            Vector2D relativeVelocityDerivative =
                differentialB.Velocity - differentialA.Velocity;
            double normalSpeedDerivative =
                Vector2D.Dot(directionDerivative, relativeVelocity) +
                Vector2D.Dot(direction, relativeVelocityDerivative);
            double forceMagnitudeDerivative =
                (stiffness * lengthDerivative) + (damping * normalSpeedDerivative);
            Vector2D forceDerivative =
                (direction * forceMagnitudeDerivative) +
                (directionDerivative * forceMagnitude);

            jacobian[0, stateIndex] = forceDerivative.X;
            jacobian[1, stateIndex] = forceDerivative.Y;
            jacobian[2, stateIndex] =
                Vector2D.Cross(differentialA.Radius, forceOnA) +
                Vector2D.Cross(kinematicsA.Radius, forceDerivative);
            jacobian[3, stateIndex] = -forceDerivative.X;
            jacobian[4, stateIndex] = -forceDerivative.Y;
            jacobian[5, stateIndex] =
                Vector2D.Cross(differentialB.Radius, forceOnB) -
                Vector2D.Cross(kinematicsB.Radius, forceDerivative);
        }

        return new DistanceSpringDamper2DEvaluation(
            kinematicsA.Point,
            kinematicsB.Point,
            length,
            normalSpeed,
            forceMagnitude,
            forceOnA);
    }

    private static EndpointKinematics GetKinematics(
        MechanicalAnchor2D endpoint,
        ConnectionBodyState2D state)
    {
        if (endpoint.IsWorld)
            return new EndpointKinematics(endpoint.Point, Vector2D.Zero, Vector2D.Zero);

        Vector2D radius = endpoint.Point.Rotate(state.Angle);
        return new EndpointKinematics(
            state.Position + radius,
            state.LinearVelocity + (radius.Perpendicular() * state.AngularVelocity),
            radius);
    }

    private static EndpointDifferential GetDifferential(
        MechanicalAnchor2D endpoint,
        ConnectionBodyState2D state,
        Vector2D radius,
        int stateIndex)
    {
        if (endpoint.IsWorld)
            return default;

        switch (stateIndex)
        {
            case 0:
                return new EndpointDifferential(Vector2D.UnitX, Vector2D.Zero, Vector2D.Zero);
            case 1:
                return new EndpointDifferential(Vector2D.UnitY, Vector2D.Zero, Vector2D.Zero);
            case 2:
                return new EndpointDifferential(
                    radius.Perpendicular(),
                    radius * -state.AngularVelocity,
                    radius.Perpendicular());
            case 3:
                return new EndpointDifferential(Vector2D.Zero, Vector2D.UnitX, Vector2D.Zero);
            case 4:
                return new EndpointDifferential(Vector2D.Zero, Vector2D.UnitY, Vector2D.Zero);
            case 5:
                return new EndpointDifferential(
                    Vector2D.Zero,
                    radius.Perpendicular(),
                    Vector2D.Zero);
            default:
                return default;
        }
    }

    private readonly struct EndpointKinematics
    {
        public EndpointKinematics(Vector2D point, Vector2D velocity, Vector2D radius)
        {
            Point = point;
            Velocity = velocity;
            Radius = radius;
        }

        public Vector2D Point { get; }

        public Vector2D Velocity { get; }

        public Vector2D Radius { get; }
    }

    private readonly struct EndpointDifferential
    {
        public EndpointDifferential(Vector2D point, Vector2D velocity, Vector2D radius)
        {
            Point = point;
            Velocity = velocity;
            Radius = radius;
        }

        public Vector2D Point { get; }

        public Vector2D Velocity { get; }

        public Vector2D Radius { get; }
    }
}
