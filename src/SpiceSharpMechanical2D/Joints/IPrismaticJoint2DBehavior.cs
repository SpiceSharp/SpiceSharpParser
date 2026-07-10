using SpiceSharp.Behaviors;
using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Joints;

public interface IPrismaticJoint2DBehavior : IBehavior
{
    double NormalError { get; }

    double NormalVelocityError { get; }

    double AxialTravel { get; }

    double AxialVelocity { get; }

    double RelativeAngleError { get; }

    Vector2D ForceOnA { get; }

    Vector2D ForceOnB { get; }

    double TorqueOnA { get; }

    double TorqueOnB { get; }

    double StoredElasticEnergy { get; }

    double DissipatedPower { get; }
}
