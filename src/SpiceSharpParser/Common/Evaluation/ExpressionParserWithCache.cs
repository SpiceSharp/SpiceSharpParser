namespace SpiceSharpParser.Common.Evaluation
{
    using System.Collections.Generic;
    using System.Threading;

    public class ExpressionParserWithCache : IExpressionParser
    {
        /// <summary>
        /// The expression parser for caching.
        /// </summary>
        private readonly IExpressionParser _parser;

        /// <summary>
        ///  The dictionary of parse results.
        /// </summary>
        private Dictionary<string, ExpressionParseResult> _parseResults;

        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
                string key = context.Name + "_" + expression;

                if (!_parseResults.ContainsKey(key))
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        var parseResult = _parser.Parse(expression, context);
                        _parseResults[key] = parseResult;

                        return parseResult;
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return _parseResults[key];
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}
