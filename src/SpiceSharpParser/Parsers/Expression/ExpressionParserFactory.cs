using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParserFactory : IExpressionParserFactory
    {
        private readonly ISpiceNetlistCaseSensitivitySettings caseSensitivitySettings;

        public ExpressionParserFactory(ISpiceNetlistCaseSensitivitySettings caseSensitivitySettings)
        {
            this.caseSensitivitySettings = caseSensitivitySettings;
        }

        public ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true, bool applyVoltage = false)
        {
            var parser = new ExpressionParser(context, throwOnErrors, caseSensitivitySettings);
            return parser;
        }
    }
}