using SpiceSharp.Behaviors;
using SpiceSharp.Physics2D.Mathematics;

namespace SpiceSharp.Physics2D.Joints;

public interface IWeldJoint2DBehavior : IBehavior
{
    Vector2D AnchorError { get; }

    Vector2D AnchorVelocityError { get; }

    double RelativeAngleError { get; }

    Vector2D ForceOnA { get; }

    Vector2D ForceOnB { get; }

    double TorqueOnA { get; }

    double TorqueOnB { get; }

    double StoredElasticEnergy { get; }

    double DissipatedPower { get; }
}
