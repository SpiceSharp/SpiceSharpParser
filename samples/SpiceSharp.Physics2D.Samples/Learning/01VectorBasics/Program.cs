using SpiceSharp.Physics2D.Mathematics;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// Vector2D uses double precision. It is the basic position, velocity,
// direction, and force value used by the rest of the library.
var a = new Vector2D(3.0, 4.0);
var b = new Vector2D(1.0, 0.0);
Vector2D sum = a + b;
Vector2D rotated = a.Rotate(Math.PI / 2.0);
Vector2D direction = a.Normalized(1e-12);

Console.WriteLine($"a                  = ({a.X}, {a.Y})");
Console.WriteLine($"length of a        = {a.Length}");
Console.WriteLine($"a + b              = ({sum.X:G6}, {sum.Y:G6})");
Console.WriteLine($"dot(a, b)          = {Vector2D.Dot(a, b)}");
Console.WriteLine($"cross(a, b)        = {Vector2D.Cross(a, b)}");
Console.WriteLine($"a rotated 90 deg   = ({rotated.X:G6}, {rotated.Y:G6})");
Console.WriteLine($"unit direction a   = ({direction.X:G6}, {direction.Y:G6})");
