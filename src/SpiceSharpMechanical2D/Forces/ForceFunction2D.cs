using SpiceSharpMechanical2D.Mathematics;

namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Evaluates a world-frame force at a transient timepoint.
    /// </summary>
    /// <param name="time">The currently probed transient time in seconds.</param>
    /// <returns>The world-frame force.</returns>
    /// <remarks>
    /// SpiceSharp may evaluate the same timepoint more than once during Newton
    /// iteration. Implementations must be deterministic, side-effect-free, and
    /// return finite values for every evaluated time.
    /// </remarks>
    public delegate Vector2D ForceFunction2D(double time);
}
