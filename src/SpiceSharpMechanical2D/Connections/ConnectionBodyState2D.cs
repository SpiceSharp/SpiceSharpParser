using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Connections;

internal readonly struct ConnectionBodyState2D
{
    public ConnectionBodyState2D(
        Vector2D position,
        double angle,
        Vector2D linearVelocity,
        double angularVelocity)
    {
        Position = position;
        Angle = angle;
        LinearVelocity = linearVelocity;
        AngularVelocity = angularVelocity;
    }

    public Vector2D Position { get; }

    public double Angle { get; }

    public Vector2D LinearVelocity { get; }

    public double AngularVelocity { get; }
}
