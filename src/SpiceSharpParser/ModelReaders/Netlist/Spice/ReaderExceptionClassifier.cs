using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    /// <summary>
    /// Distinguishes failures caused by netlist input from unexpected reader failures.
    /// </summary>
    internal static class ReaderExceptionClassifier
    {
        public static bool IsRecoverableInputException(Exception exception)
        {
            if (exception == null || exception is OperationCanceledException)
            {
                return false;
            }

            if (exception is SpiceSharpParserException
                || exception is SpiceSharpException
                || exception is Parsers.Expression.ParserException
                || exception is FormatException
                || exception is ArithmeticException
                || exception is System.IO.IOException
                || exception is UnauthorizedAccessException
                || exception is System.Security.SecurityException
                || exception is System.Text.DecoderFallbackException
                || exception is IndexOutOfRangeException
                || exception is KeyNotFoundException
                || exception is InvalidOperationException
                || exception is NotSupportedException
                || (exception is ArgumentException && !(exception is ArgumentNullException)))
            {
                return true;
            }

            return exception.InnerException != null
                && IsRecoverableInputException(exception.InnerException);
        }
    }
}
