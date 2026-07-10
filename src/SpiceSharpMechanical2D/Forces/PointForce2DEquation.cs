using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Forces
{
    internal static class PointForce2DEquation
    {
        public static PointForce2DContribution Evaluate(
            double angle,
            Vector2D localPoint,
            Vector2D force,
            ForceCoordinateSystem2D forceCoordinates)
        {
            Vector2D worldOffset = localPoint.Rotate(angle);
            Vector2D offsetDerivative = worldOffset.Perpendicular();
            Vector2D worldForce;
            Vector2D forceDerivative;

            if (forceCoordinates == ForceCoordinateSystem2D.BodyLocal)
            {
                worldForce = force.Rotate(angle);
                forceDerivative = worldForce.Perpendicular();
            }
            else
            {
                worldForce = force;
                forceDerivative = Vector2D.Zero;
            }

            double torque = Vector2D.Cross(worldOffset, worldForce);
            double torqueDerivative = Vector2D.Cross(offsetDerivative, worldForce)
                + Vector2D.Cross(worldOffset, forceDerivative);
            return new PointForce2DContribution(
                worldForce,
                torque,
                forceDerivative,
                torqueDerivative);
        }
    }

    internal readonly struct PointForce2DContribution
    {
        public PointForce2DContribution(
            Vector2D worldForce,
            double torque,
            Vector2D worldForceDerivativeByAngle,
            double torqueDerivativeByAngle)
        {
            WorldForce = worldForce;
            Torque = torque;
            WorldForceDerivativeByAngle = worldForceDerivativeByAngle;
            TorqueDerivativeByAngle = torqueDerivativeByAngle;
        }

        public Vector2D WorldForce { get; }

        public double Torque { get; }

        public Vector2D WorldForceDerivativeByAngle { get; }

        public double TorqueDerivativeByAngle { get; }
    }
}
