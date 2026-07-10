using System;

namespace SpiceSharpMechanical2D.Connections;

/// <summary>
/// Identifies either a rigid body's rotation or a fixed world-frame angle.
/// </summary>
public readonly struct RotationalEndpoint2D
{
    private RotationalEndpoint2D(string bodyName, double fixedAngle, bool isWorld)
    {
        BodyName = bodyName;
        FixedAngle = fixedAngle;
        IsWorld = isWorld;
    }

    /// <summary>
    /// Gets the referenced body name. This value is <see langword="null"/> for a world endpoint.
    /// </summary>
    public string BodyName { get; }

    /// <summary>
    /// Gets the fixed world-frame angle. This value is ignored for a body endpoint.
    /// </summary>
    public double FixedAngle { get; }

    /// <summary>
    /// Gets a value indicating whether this endpoint is fixed in the world frame.
    /// </summary>
    public bool IsWorld { get; }

    /// <summary>
    /// Creates an endpoint that follows a rigid body's rotation.
    /// </summary>
    /// <param name="bodyName">The rigid-body entity name.</param>
    /// <returns>The body endpoint.</returns>
    public static RotationalEndpoint2D Body(string bodyName)
    {
        if (string.IsNullOrWhiteSpace(bodyName))
            throw new ArgumentException("A rigid-body name is required.", nameof(bodyName));
        return new RotationalEndpoint2D(bodyName, 0.0, false);
    }

    /// <summary>
    /// Creates an endpoint fixed at an angle in the world frame.
    /// </summary>
    /// <param name="fixedAngle">The fixed world-frame angle in radians.</param>
    /// <returns>The world endpoint.</returns>
    public static RotationalEndpoint2D World(double fixedAngle = 0.0)
    {
        ValidateFinite(fixedAngle, nameof(fixedAngle));
        return new RotationalEndpoint2D(null, fixedAngle, true);
    }

    internal void Validate(string parameterName)
    {
        if (IsWorld)
            ValidateFinite(FixedAngle, parameterName);
        else if (string.IsNullOrWhiteSpace(BodyName))
            throw new ArgumentException("A body endpoint must reference a rigid-body name.", parameterName);
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(parameterName, "The endpoint angle must be finite.");
    }
}
