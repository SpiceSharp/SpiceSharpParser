using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpiceSharpParser.CustomComponents.Digital
{
    internal static class DigitalParameterOverrides
    {
        public static void AddRatio(
            IDictionary<string, string> destination,
            string name,
            double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            ValidateFinite(name, value.Value);
            if (value.Value <= 0.0 || value.Value >= 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value.Value,
                    "The threshold ratio must be greater than zero and less than one.");
            }

            destination[name] = Format(value.Value);
        }

        public static void AddPositive(
            IDictionary<string, string> destination,
            string name,
            double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            ValidateFinite(name, value.Value);
            if (value.Value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value.Value,
                    "The value must be greater than zero.");
            }

            destination[name] = Format(value.Value);
        }

        public static void AddNonNegative(
            IDictionary<string, string> destination,
            string name,
            double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            ValidateFinite(name, value.Value);
            if (value.Value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value.Value,
                    "The value must not be negative.");
            }

            destination[name] = Format(value.Value);
        }

        private static void ValidateFinite(string name, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "The value must be finite.");
            }
        }

        private static string Format(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
