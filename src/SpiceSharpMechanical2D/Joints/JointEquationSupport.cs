using SpiceSharpMechanical2D.Connections;
using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Joints;

internal static class JointEquationSupport
{
    public const int LoadCount = 6;
    public const int StateCount = 12;

    public static JointEndpointKinematics GetKinematics(
        MechanicalAnchor2D endpoint,
        ConnectionBodyState2D state,
        int stateOffset)
    {
        if (endpoint.IsWorld)
        {
            return new JointEndpointKinematics(
                new DualVector2D(
                    Dual12.Constant(endpoint.Point.X),
                    Dual12.Constant(endpoint.Point.Y)),
                new DualVector2D(Dual12.Constant(0.0), Dual12.Constant(0.0)),
                new DualVector2D(Dual12.Constant(0.0), Dual12.Constant(0.0)),
                Dual12.Constant(0.0),
                Dual12.Constant(0.0));
        }

        var position = new DualVector2D(
            Dual12.Variable(state.Position.X, stateOffset),
            Dual12.Variable(state.Position.Y, stateOffset + 1));
        Dual12 angle = Dual12.Variable(state.Angle, stateOffset + 2);
        var linearVelocity = new DualVector2D(
            Dual12.Variable(state.LinearVelocity.X, stateOffset + 3),
            Dual12.Variable(state.LinearVelocity.Y, stateOffset + 4));
        Dual12 angularVelocity = Dual12.Variable(state.AngularVelocity, stateOffset + 5);
        DualVector2D radius = DualVector2D.Rotate(endpoint.Point.X, endpoint.Point.Y, angle);
        return new JointEndpointKinematics(
            position + radius,
            linearVelocity + (radius.Perpendicular() * angularVelocity),
            radius,
            angle,
            angularVelocity);
    }

    public static void WriteLoad(
        int loadIndex,
        Dual12 load,
        double[] loads,
        double[,] jacobian)
    {
        loads[loadIndex] = load.Value;
        for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
        {
            jacobian[loadIndex, stateIndex] = load.GetDerivative(stateIndex);
        }
    }

    public static Vector2D ToVector(DualVector2D value) =>
        new(value.X.Value, value.Y.Value);
}

internal readonly struct JointEndpointKinematics
{
    public JointEndpointKinematics(
        DualVector2D point,
        DualVector2D velocity,
        DualVector2D radius,
        Dual12 angle,
        Dual12 angularVelocity)
    {
        Point = point;
        Velocity = velocity;
        Radius = radius;
        Angle = angle;
        AngularVelocity = angularVelocity;
    }

    public DualVector2D Point { get; }

    public DualVector2D Velocity { get; }

    public DualVector2D Radius { get; }

    public Dual12 Angle { get; }

    public Dual12 AngularVelocity { get; }
}
