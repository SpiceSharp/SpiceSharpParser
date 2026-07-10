using System;

namespace SpiceSharpMechanical2D.Joints;

internal readonly struct Dual12
{
    private Dual12(
        double value,
        double d0,
        double d1,
        double d2,
        double d3,
        double d4,
        double d5,
        double d6,
        double d7,
        double d8,
        double d9,
        double d10,
        double d11)
    {
        Value = value;
        D0 = d0;
        D1 = d1;
        D2 = d2;
        D3 = d3;
        D4 = d4;
        D5 = d5;
        D6 = d6;
        D7 = d7;
        D8 = d8;
        D9 = d9;
        D10 = d10;
        D11 = d11;
    }

    public double Value { get; }

    private double D0 { get; }
    private double D1 { get; }
    private double D2 { get; }
    private double D3 { get; }
    private double D4 { get; }
    private double D5 { get; }
    private double D6 { get; }
    private double D7 { get; }
    private double D8 { get; }
    private double D9 { get; }
    private double D10 { get; }
    private double D11 { get; }

    public static Dual12 Constant(double value) =>
        new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

    public static Dual12 Variable(double value, int index)
    {
        return index switch
        {
            0 => new(value, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            1 => new(value, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            2 => new(value, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            3 => new(value, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            4 => new(value, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            5 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            6 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0),
            7 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0),
            8 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0),
            9 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0),
            10 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0),
            11 => new(value, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    public double GetDerivative(int index)
    {
        return index switch
        {
            0 => D0,
            1 => D1,
            2 => D2,
            3 => D3,
            4 => D4,
            5 => D5,
            6 => D6,
            7 => D7,
            8 => D8,
            9 => D9,
            10 => D10,
            11 => D11,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    public static Dual12 operator +(Dual12 left, Dual12 right) =>
        new(
            left.Value + right.Value,
            left.D0 + right.D0,
            left.D1 + right.D1,
            left.D2 + right.D2,
            left.D3 + right.D3,
            left.D4 + right.D4,
            left.D5 + right.D5,
            left.D6 + right.D6,
            left.D7 + right.D7,
            left.D8 + right.D8,
            left.D9 + right.D9,
            left.D10 + right.D10,
            left.D11 + right.D11);

    public static Dual12 operator -(Dual12 left, Dual12 right) => left + (-right);

    public static Dual12 operator -(Dual12 value) => value * -1.0;

    public static Dual12 operator *(Dual12 value, double scalar) =>
        new(
            value.Value * scalar,
            value.D0 * scalar,
            value.D1 * scalar,
            value.D2 * scalar,
            value.D3 * scalar,
            value.D4 * scalar,
            value.D5 * scalar,
            value.D6 * scalar,
            value.D7 * scalar,
            value.D8 * scalar,
            value.D9 * scalar,
            value.D10 * scalar,
            value.D11 * scalar);

    public static Dual12 operator *(double scalar, Dual12 value) => value * scalar;

    public static Dual12 operator *(Dual12 left, Dual12 right) =>
        new(
            left.Value * right.Value,
            (left.D0 * right.Value) + (right.D0 * left.Value),
            (left.D1 * right.Value) + (right.D1 * left.Value),
            (left.D2 * right.Value) + (right.D2 * left.Value),
            (left.D3 * right.Value) + (right.D3 * left.Value),
            (left.D4 * right.Value) + (right.D4 * left.Value),
            (left.D5 * right.Value) + (right.D5 * left.Value),
            (left.D6 * right.Value) + (right.D6 * left.Value),
            (left.D7 * right.Value) + (right.D7 * left.Value),
            (left.D8 * right.Value) + (right.D8 * left.Value),
            (left.D9 * right.Value) + (right.D9 * left.Value),
            (left.D10 * right.Value) + (right.D10 * left.Value),
            (left.D11 * right.Value) + (right.D11 * left.Value));

    public static Dual12 Sin(Dual12 value) => Scale(Math.Sin(value.Value), Math.Cos(value.Value), value);

    public static Dual12 Cos(Dual12 value) => Scale(Math.Cos(value.Value), -Math.Sin(value.Value), value);

    private static Dual12 Scale(double value, double derivativeScale, Dual12 source) =>
        new(
            value,
            derivativeScale * source.D0,
            derivativeScale * source.D1,
            derivativeScale * source.D2,
            derivativeScale * source.D3,
            derivativeScale * source.D4,
            derivativeScale * source.D5,
            derivativeScale * source.D6,
            derivativeScale * source.D7,
            derivativeScale * source.D8,
            derivativeScale * source.D9,
            derivativeScale * source.D10,
            derivativeScale * source.D11);
}

internal readonly struct DualVector2D
{
    public DualVector2D(Dual12 x, Dual12 y)
    {
        X = x;
        Y = y;
    }

    public Dual12 X { get; }

    public Dual12 Y { get; }

    public static DualVector2D operator +(DualVector2D left, DualVector2D right) =>
        new(left.X + right.X, left.Y + right.Y);

    public static DualVector2D operator -(DualVector2D left, DualVector2D right) =>
        new(left.X - right.X, left.Y - right.Y);

    public static DualVector2D operator -(DualVector2D value) => new(-value.X, -value.Y);

    public static DualVector2D operator *(DualVector2D value, Dual12 scalar) =>
        new(value.X * scalar, value.Y * scalar);

    public static DualVector2D operator *(DualVector2D value, double scalar) =>
        new(value.X * scalar, value.Y * scalar);

    public DualVector2D Perpendicular() => new(-Y, X);

    public static Dual12 Dot(DualVector2D left, DualVector2D right) =>
        (left.X * right.X) + (left.Y * right.Y);

    public static Dual12 Cross(DualVector2D left, DualVector2D right) =>
        (left.X * right.Y) - (left.Y * right.X);

    public static DualVector2D Rotate(double x, double y, Dual12 angle)
    {
        Dual12 cosine = Dual12.Cos(angle);
        Dual12 sine = Dual12.Sin(angle);
        return new(
            (cosine * Dual12.Constant(x)) - (sine * Dual12.Constant(y)),
            (sine * Dual12.Constant(x)) + (cosine * Dual12.Constant(y)));
    }
}
