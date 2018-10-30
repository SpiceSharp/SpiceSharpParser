namespace SpiceSharpParser.Common.Evaluation
{
    using System.Collections.Generic;
    using System.Threading;

    public class ExpressionParserWithCache : IExpressionParser
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The expression parser for caching.
        /// </summary>
        protected readonly IExpressionParser _parser;

        /// <summary>
        ///  The dictionary of parse results.
        /// </summary>
        protected Dictionary<string, ExpressionParseResult> _parseResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionParserWithCache"/> class.
        /// </summary>
        /// <param name="parser">A expression parser</param>
        public ExpressionParserWithCache(IExpressionParser parser)
        {
            _parser = parser;
            _parseResults = new Dictionary<string, ExpressionParseResult>();
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="context">The parser context.</param>
        /// <returns>Returns the result of parsing.</returns>
        public ExpressionParseResult Parse(string expression, ExpressionParserContext context)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (!_parseResults.ContainsKey(expression))
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        var parseResult = _parser.Parse(expression, context);
                        _parseResults[expression] = parseResult;

                        return parseResult;
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return _parseResults[expression];
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}
