using System;
using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Connections;

/// <summary>
/// Identifies either a body-local attachment point or a fixed point in the world frame.
/// </summary>
public readonly struct MechanicalAnchor2D
{
    private MechanicalAnchor2D(string bodyName, Vector2D point, bool isWorld)
    {
        BodyName = bodyName;
        Point = point;
        IsWorld = isWorld;
    }

    /// <summary>
    /// Gets the referenced body name. This value is <see langword="null"/> for a world anchor.
    /// </summary>
    public string BodyName { get; }

    /// <summary>
    /// Gets the body-local point for a body anchor, or the fixed world point for a world anchor.
    /// </summary>
    public Vector2D Point { get; }

    /// <summary>
    /// Gets a value indicating whether this anchor is fixed in the world frame.
    /// </summary>
    public bool IsWorld { get; }

    /// <summary>
    /// Creates an attachment point expressed in a rigid body's local frame.
    /// </summary>
    /// <param name="bodyName">The rigid-body entity name.</param>
    /// <param name="localPoint">The attachment point in the body's local frame.</param>
    /// <returns>The body anchor.</returns>
    public static MechanicalAnchor2D Body(string bodyName, Vector2D localPoint)
    {
        if (string.IsNullOrWhiteSpace(bodyName))
            throw new ArgumentException("A rigid-body name is required.", nameof(bodyName));
        ValidateFinite(localPoint, nameof(localPoint));
        return new MechanicalAnchor2D(bodyName, localPoint, false);
    }

    /// <summary>
    /// Creates an attachment point fixed in the world frame.
    /// </summary>
    /// <param name="worldPoint">The fixed point in the world frame.</param>
    /// <returns>The world anchor.</returns>
    public static MechanicalAnchor2D World(Vector2D worldPoint)
    {
        ValidateFinite(worldPoint, nameof(worldPoint));
        return new MechanicalAnchor2D(null, worldPoint, true);
    }

    internal void Validate(string parameterName)
    {
        ValidateFinite(Point, parameterName);
        if (!IsWorld && string.IsNullOrWhiteSpace(BodyName))
            throw new ArgumentException("A body anchor must reference a rigid-body name.", parameterName);
    }

    private static void ValidateFinite(Vector2D value, string parameterName)
    {
        if (double.IsNaN(value.X) || double.IsInfinity(value.X) ||
            double.IsNaN(value.Y) || double.IsInfinity(value.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Anchor coordinates must be finite.");
        }
    }
}
