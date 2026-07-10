using SpiceSharp.Behaviors;
using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Joints;

public interface IRevoluteJoint2DBehavior : IBehavior
{
    Vector2D AnchorError { get; }

    Vector2D AnchorVelocityError { get; }

    Vector2D ForceOnA { get; }

    Vector2D ForceOnB { get; }

    double TorqueOnA { get; }

    double TorqueOnB { get; }

    double StoredElasticEnergy { get; }

    double DissipatedPower { get; }
}
