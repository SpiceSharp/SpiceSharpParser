using SpiceSharpParser.Lexers.Netlist.Spice;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.Netlist.Spice.Internals
{
    /// <summary>
    /// A parser tree generator for Spice netlist based on grammar from SpiceGrammar.txt.
    /// It's a hand written LL(*) parser.
    /// </summary>
    public class ParseTreeGenerator
    {
        private readonly Dictionary<string, Parser> _parsers = new Dictionary<string, Parser>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeGenerator"/> class.
        /// </summary>
        /// <param name="isDotStatementNameCaseSensitive">Are dot statements case-sensitive.</param>
        public ParseTreeGenerator(bool isDotStatementNameCaseSensitive)
        {
            IsDotStatementNameCaseSensitive = isDotStatementNameCaseSensitive;
            _parsers.Add(Symbols.Netlist, ReadNetlist);
            _parsers.Add(Symbols.NetlistWithoutTitle, ReadNetlistWithoutTitle);
            _parsers.Add(Symbols.NetlistEnding, ReadNetlistEnding);
            _parsers.Add(Symbols.Statements, ReadStatements);
            _parsers.Add(Symbols.Statement, ReadStatement);
            _parsers.Add(Symbols.CommentLine, ReadCommentLine);
            _parsers.Add(Symbols.Subckt, ReadSubckt);
            _parsers.Add(Symbols.SubcktEnding, ReadSubcktEnding);
            _parsers.Add(Symbols.Component, ReadComponent);
            _parsers.Add(Symbols.Control, ReadControl);
            _parsers.Add(Symbols.Model, ReadModel);
            _parsers.Add(Symbols.Parameters, ReadParameters);
            _parsers.Add(Symbols.ParametersSeparator, ReadParametersSeparator);
            _parsers.Add(Symbols.Parameter, ReadParameter);
            _parsers.Add(Symbols.ParameterSingle, ReadParameterSingle);
            _parsers.Add(Symbols.ParameterBracket, ReadParameterBracket);
            _parsers.Add(Symbols.ParameterBracketContent, ReadParameterBracketContent);
            _parsers.Add(Symbols.ParameterEqual, ReadParameterEqual);
            _parsers.Add(Symbols.ParameterEqualSingle, ReadParameterEqualSingle);
            _parsers.Add(Symbols.Vector, ReadVector);
            _parsers.Add(Symbols.VectorContinue, ReadVectorContinue);
            _parsers.Add(Symbols.NewLine, ReadNewLine);
            _parsers.Add(Symbols.NewLines, ReadNewLines);
            _parsers.Add(Symbols.ExpressionEqual, ReadExpressionEqual);
            _parsers.Add(Symbols.Points, ReadPoints);
            _parsers.Add(Symbols.PointsContinue, ReadPointsContinue);
            _parsers.Add(Symbols.Point, ReadPoint);
            _parsers.Add(Symbols.PointValues, ReadPointValues);
            _parsers.Add(Symbols.PointValue, ReadPointValue);
            _parsers.Add(Symbols.Distribution, ReadDistribution);
        }

        protected delegate void Parser(
            Stack<ParseTreeNode> stack,
            ParseTreeNonTerminalNode currentNode,
            SpiceToken[] tokens,
            int currentTokenIndex);

        protected bool IsDotStatementNameCaseSensitive { get; }

        /// <summary>
        /// Generates a parse tree for SPICE grammar.
        /// </summary>
        /// <param name="tokens">An array of tokens.</param>
        /// <param name="rootSymbol">A root symbol of parse tree.</param>
        /// <returns>
        /// A parse tree.
        /// </returns>
        public ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens, string rootSymbol)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            var stack = new Stack<ParseTreeNode>();

            var root = CreateNonTerminalNode(rootSymbol, null);
            stack.Push(root);

            int currentTokenIndex = 0;

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                if (currentNode is ParseTreeNonTerminalNode ntn)
                {
                    if (_parsers.ContainsKey(ntn.Name))
                    {
                        var tokenIndexToUse = Math.Min(currentTokenIndex, tokens.Length - 1);

                        ntn.StartColumnIndex = tokens[tokenIndexToUse].StartColumnIndex;
                        ntn.LineNumber = tokens[tokenIndexToUse].LineNumber;
                        ntn.FileName = tokens[tokenIndexToUse].FileName;
                        _parsers[ntn.Name](stack, ntn, tokens, currentTokenIndex);
                    }
                    else
                    {
                        throw new ParseException($"Unknown non-terminal found while parsing.{ntn.Name}", tokens[currentTokenIndex].LineNumber);
                    }
                }

                if (currentNode is ParseTreeTerminalNode tn)
                {
                    if (currentTokenIndex >= tokens.Length)
                    {
                        throw new ParseException(
                            $"End of tokens. Expected token type: {tn.Token.SpiceTokenType} line={tokens[tokens.Length - 1].LineNumber}", tokens[tokens.Length - 1].LineNumber);
                    }

                    if (tn.Token.SpiceTokenType == tokens[currentTokenIndex].SpiceTokenType && (tn.Token.Lexem == null || tn.Token.Lexem == tokens[currentTokenIndex].Lexem))
                    {
                        // TODO: refactor it
                        tn.Token.Lexem = tokens[currentTokenIndex].Lexem;
                        tn.Token.LineNumber = tokens[currentTokenIndex].LineNumber;
                        tn.Token.StartColumnIndex = tokens[currentTokenIndex].StartColumnIndex;
                        tn.Token.FileName = tokens[currentTokenIndex].FileName;

                        ((ParseTreeNonTerminalNode)tn.Parent).EndColumnIndex = tokens[currentTokenIndex].EndColumnIndex;

                        currentTokenIndex++;
                    }
                    else
                    {
                        throw new ParseException(
                            $"Unexpected token '{tokens[currentTokenIndex].Lexem}' of type: {tokens[currentTokenIndex].SpiceTokenType} at line = {tokens[currentTokenIndex].LineNumber}. Expected token type: {tn.Token.SpiceTokenType}",
                            tokens[currentTokenIndex].LineNumber);
                    }
                }
            }

            if (currentTokenIndex != tokens.Length)
            {
                throw new ParseException("There are pending tokens to process", tokens[currentTokenIndex].LineNumber);
            }

            return root;
        }

        private static bool IsEqualTokens(SpiceToken[] tokens, int currentTokenIndex)
        {
            while (tokens.Length > currentTokenIndex && tokens[currentTokenIndex].Lexem != ")")
            {
                currentTokenIndex += 1;
            }

            if (currentTokenIndex + 1 >= tokens.Length - 1)
            {
                return false;
            }

            return tokens[currentTokenIndex + 1].Lexem == "=";
        }

        /// <summary>
        /// Reads <see cref="Symbols.SubcktEnding"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadSubcktEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.ENDS))
            {
                if (nextToken.Is(SpiceTokenType.WORD) || nextToken.Is(SpiceTokenType.IDENTIFIER))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(SpiceTokenType.ENDS, currentNode),
                        CreateTerminalNode(nextToken.SpiceTokenType, currentNode));
                }
                else
                {
                    PushProductionExpression(
                         stack,
                         CreateTerminalNode(SpiceTokenType.ENDS, currentNode));
                }
            }
            else
            {
                throw new ParseException("Error during parsing subcircuit. Expected .ENDS. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.NewLines"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadNewLines(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                    CreateNonTerminalNode(Symbols.NewLines, currentNode));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.EOF))
                {
                    // follow
                }
                else
                {
                    throw new ParseException("Newline was expected. Other token was found.", currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.NewLine"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadNewLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                throw new ParseException("End of tokens. New line not found", tokens[tokens.Length - 1].LineNumber);
            }

            var currentToken = tokens[currentTokenIndex];
            if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode));
            }
            else
            {
                throw new ParseException("Newline was expected. Other token was found.", currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Netlist"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadNetlist(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.EOF))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.NetlistEnding, currentNode));
                return;
            }

            if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                    CreateNonTerminalNode(Symbols.Statements, currentNode),
                    CreateNonTerminalNode(Symbols.NetlistEnding, currentNode));
            }
            else
            {
                if (tokens[currentTokenIndex + 1].Is(SpiceTokenType.EOF))
                {
                    PushProductionExpression(
                     stack,
                     CreateTerminalNode(SpiceTokenType.TITLE, currentNode),
                     CreateNonTerminalNode(Symbols.NetlistEnding, currentNode));
                }
                else
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(SpiceTokenType.TITLE, currentNode),
                        CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                        CreateNonTerminalNode(Symbols.Statements, currentNode),
                        CreateNonTerminalNode(Symbols.NetlistEnding, currentNode));
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.NetlistWithoutTitle"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadNetlistWithoutTitle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(Symbols.Statements, currentNode),
                CreateNonTerminalNode(Symbols.NetlistEnding, currentNode));
        }

        /// <summary>
        /// Reads <see cref="Symbols.NetlistEnding"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadNetlistEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.END))
            {
                var nextToken = tokens[currentTokenIndex + 1];

                if (nextToken.Is(SpiceTokenType.EOF))
                {
                    PushProductionExpression(
                                stack,
                                CreateTerminalNode(SpiceTokenType.END, current),
                                CreateTerminalNode(SpiceTokenType.EOF, current));
                }
                else
                {
                    if (nextToken.Is(SpiceTokenType.NEWLINE))
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.END, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                            CreateNonTerminalNode(Symbols.NewLines, current),
                            CreateTerminalNode(SpiceTokenType.EOF, current));
                    }
                    else
                    {
                        throw new ParseException("Netlist ending - wrong ending", currentToken.LineNumber);
                    }
                }
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.EOF))
                {
                    PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.EOF, current));
                }
                else
                {
                    if (currentToken.Is(SpiceTokenType.NEWLINE))
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                            CreateNonTerminalNode(Symbols.NewLines, current),
                            CreateTerminalNode(SpiceTokenType.EOF, current));
                    }
                    else
                    {
                        throw new ParseException("Netlist ending - wrong ending", currentToken.LineNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Statements"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadStatements(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.DOT)
                || currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.SUFFIX)
                || currentToken.Is(SpiceTokenType.MODEL)
                || currentToken.Is(SpiceTokenType.IDENTIFIER)
                || currentToken.Is(SpiceTokenType.COMMENT)
                || currentToken.Is(SpiceTokenType.ENDL)
                || currentToken.Is(SpiceTokenType.IF)
                || currentToken.Is(SpiceTokenType.ELSE)
                || currentToken.Is(SpiceTokenType.ELSE_IF)
                || currentToken.Is(SpiceTokenType.ENDIF))
            {
                PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.Statement, current),
                            CreateNonTerminalNode(Symbols.Statements, current));
            }
            else if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                         stack,
                         CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                         CreateNonTerminalNode(Symbols.Statements, current));
            }
            else if (currentToken.Is(SpiceTokenType.END))
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceTokenType.ENDS))
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceTokenType.EOF))
            {
                // follow - do nothing
            }
            else
            {
                throw new ParseException(
                    $"Error during parsing statements. Unexpected token: '{currentToken.Lexem}' of type {currentToken.SpiceTokenType}, line={currentToken.LineNumber}", currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Statement"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadStatement(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER) || currentToken.Is(SpiceTokenType.SUFFIX))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.Component, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current));
            }
            else if (currentToken.Is(SpiceTokenType.DOT))
            {
                if (nextToken.Is(SpiceTokenType.WORD) || nextToken.Is(SpiceTokenType.IDENTIFIER))
                {
                    if (nextToken.Equal("SUBCKT", IsDotStatementNameCaseSensitive))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.Subckt, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                    else if (nextToken.Equal("DISTRIBUTION", IsDotStatementNameCaseSensitive))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.Distribution, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.Control, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                }
                else
                {
                    throw new ParseException("Error during parsing a statement. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }
            else if (currentToken.Is(SpiceTokenType.COMMENT))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.CommentLine, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current));
            }
            else if (currentToken.Is(SpiceTokenType.MODEL))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.Model, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.ENDL)
                    || currentToken.Is(SpiceTokenType.IF)
                    || currentToken.Is(SpiceTokenType.ELSE_IF)
                    || currentToken.Is(SpiceTokenType.ELSE)
                    || currentToken.Is(SpiceTokenType.ENDIF))
                {
                    PushProductionExpression(
                           stack,
                           CreateNonTerminalNode(Symbols.Control, current),
                           CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                }
                else
                {
                    throw new ParseException(string.Format("Error during parsing a statement. Unexpected token: '{0}' of type:{1} line={2}", currentToken.Lexem, currentToken.SpiceTokenType, currentToken.LineNumber), currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Vector"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadVector(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(Symbols.ParameterSingle, current),
                CreateTerminalNode(SpiceTokenType.COMMA, current, ","),
                CreateNonTerminalNode(Symbols.ParameterSingle, current),
                CreateNonTerminalNode(Symbols.VectorContinue, current));
        }

        /// <summary>
        /// Reads <see cref="Symbols.PointsContinue"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadPointsContinue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == ")")
            {
                if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Lexem == "(")
                {
                    var nextNextToken = tokens[currentTokenIndex + 2];

                    if (nextNextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Lexem == "(")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.DELIMITER, current, ")"),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, current, "("),
                            CreateNonTerminalNode(Symbols.Point, current),
                            CreateNonTerminalNode(Symbols.PointsContinue, current));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.DELIMITER, current, ")"),
                            CreateNonTerminalNode(Symbols.Point, current),
                            CreateNonTerminalNode(Symbols.PointsContinue, current));
                    }
                }
                else
                {
                    if (!nextToken.Is(SpiceTokenType.NEWLINE) && !nextToken.Is(SpiceTokenType.EOF))
                    {
                        throw new ParseException("Points in broken format", nextToken.LineNumber);
                    }
                }
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == "(")
                {
                    if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Lexem == "(")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.DELIMITER, current, "("),
                            CreateNonTerminalNode(Symbols.Point, current),
                            CreateNonTerminalNode(Symbols.PointsContinue, current));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.Point, current),
                            CreateNonTerminalNode(Symbols.PointsContinue, current));
                    }
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Points"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadPoints(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == "("
                && nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Lexem == "(")
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.DELIMITER, current, "("),
                    CreateNonTerminalNode(Symbols.Point, current),
                    CreateNonTerminalNode(Symbols.PointsContinue, current),
                    CreateTerminalNode(SpiceTokenType.DELIMITER, current, ")"));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == "(")
                {
                    PushProductionExpression(
                       stack,
                       CreateNonTerminalNode(Symbols.Point, current),
                       CreateNonTerminalNode(Symbols.PointsContinue, current));
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Point"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadPoint(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.DELIMITER, current, "("),
                CreateNonTerminalNode(Symbols.PointValues, current),
                CreateTerminalNode(SpiceTokenType.DELIMITER, current, ")"));
        }

        /// <summary>
        /// Reads <see cref="Symbols.PointValues"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadPointValues(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var nextToken = tokens[currentTokenIndex + 1];

            if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Lexem == ")")
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.PointValue, current));
            }
            else
            {
                if (nextToken.Is(SpiceTokenType.COMMA))
                {
                    PushProductionExpression(
                        stack,
                        CreateNonTerminalNode(Symbols.PointValue, current),
                        CreateTerminalNode(SpiceTokenType.COMMA, current, ","),
                        CreateNonTerminalNode(Symbols.PointValues, current));
                }
                else
                {
                    PushProductionExpression(
                        stack,
                        CreateNonTerminalNode(Symbols.PointValue, current),
                        CreateNonTerminalNode(Symbols.PointValues, current));
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.PointValue"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadPointValue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                || currentToken.Is(SpiceTokenType.EXPRESSION)
                || currentToken.Is(SpiceTokenType.VALUE)
                || currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.IDENTIFIER))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.ParameterSingle, current));
            }
            else
            {
                throw new ParseException("Unsupported point value", currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.VectorContinue"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadVectorContinue(
            Stack<ParseTreeNode> stack,
            ParseTreeNonTerminalNode current,
            SpiceToken[] tokens,
            int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                return; // empty
            }

            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.COMMA))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.COMMA, current, ","),
                    CreateNonTerminalNode(Symbols.ParameterSingle, current),
                    CreateNonTerminalNode(Symbols.VectorContinue, current));
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.CommentLine"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadCommentLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.COMMENT, current));
        }

        /// <summary>
        /// Reads <see cref="Symbols.Subckt"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadSubckt(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];
            var nextNextToken = tokens[currentTokenIndex + 2];

            if (currentToken.Is(SpiceTokenType.DOT)
                && ((nextToken.Is(SpiceTokenType.WORD) && nextToken.Equal("SUBCKT", IsDotStatementNameCaseSensitive))
                    || (nextToken.Is(SpiceTokenType.IDENTIFIER) && nextToken.Equal("SUBCKT", IsDotStatementNameCaseSensitive)))
                && (nextNextToken.Is(SpiceTokenType.WORD) || nextNextToken.Is(SpiceTokenType.IDENTIFIER)))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, current, currentToken.Lexem),
                    CreateTerminalNode(nextToken.SpiceTokenType, current, nextToken.Lexem),
                    CreateTerminalNode(nextNextToken.SpiceTokenType, current),
                    CreateNonTerminalNode(Symbols.Parameters, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                    CreateNonTerminalNode(Symbols.Statements, current),
                    CreateNonTerminalNode(Symbols.SubcktEnding, current));
            }
            else
            {
                throw new ParseException("Error during parsing a subcircuit. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.ParametersSeparator"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParametersSeparator(
            Stack<ParseTreeNode> stack,
            ParseTreeNonTerminalNode current,
            SpiceToken[] tokens,
            int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                // empty
                return;
            }

            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.COMMA))
            {
                PushProductionExpression(stack, CreateTerminalNode(SpiceTokenType.COMMA, current));
            }
            else
            {
                // do nothing
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Parameters"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameters(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                // empty
                return;
            }

            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.VALUE)
                || currentToken.Is(SpiceTokenType.DOUBLE_QUOTED_STRING)
                || currentToken.Is(SpiceTokenType.SINGLE_QUOTED_STRING)
                || currentToken.Is(SpiceTokenType.IDENTIFIER)
                || currentToken.Is(SpiceTokenType.REFERENCE)
                || currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                || currentToken.Is(SpiceTokenType.EXPRESSION)
                || currentToken.Is(SpiceTokenType.SUFFIX)
                || currentToken.Is(SpiceTokenType.PREFIX_SINGLE)
                || currentToken.Is(SpiceTokenType.PREFIX_COMPLEX)
                || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                || currentToken.Is(SpiceTokenType.PERCENT)
                || (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == "("))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.Parameter, current),
                    CreateNonTerminalNode(Symbols.ParametersSeparator, current),
                    CreateNonTerminalNode(Symbols.Parameters, current));
            }
            else if (currentToken.Is(SpiceTokenType.EOF))
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == ")")
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceTokenType.COMMENT_HSPICE) || currentToken.Is(SpiceTokenType.COMMENT_PSPICE))
            {
                // follow - do nothing
            }
            else
            {
                throw new ParseException(
                    string.Format("Error during parsing parameters. Unexpected token: '{0}' of type {1} line={2}", currentToken.Lexem, currentToken.SpiceTokenType, currentToken.LineNumber), currentToken.LineNumber);
            }
        }

        private void ReadExpressionEqual(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                || currentToken.Is(SpiceTokenType.EXPRESSION)
                || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES))
            {
                if (nextToken.Is(SpiceTokenType.EQUAL))
                {
                    PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode, "="),
                            CreateNonTerminalNode(Symbols.Points, currentNode));
                }
                else
                {
                    PushProductionExpression(
                           stack,
                           CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                           CreateNonTerminalNode(Symbols.Points, currentNode));
                }
            }
            else
            {
                throw new ParseException("Expression equal should start with expression", currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.ParameterEqual"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameterEqual(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER))
            {
                if (nextToken.Is(SpiceTokenType.EQUAL))
                {
                    if (((currentTokenIndex + 3) < tokens.Length)
                        && tokens[currentTokenIndex + 3].Is(SpiceTokenType.COMMA)
                        && ((currentTokenIndex + 5) >= tokens.Length || !tokens[currentTokenIndex + 5].Is(SpiceTokenType.EQUAL)))
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(nextToken.SpiceTokenType, currentNode),
                            CreateNonTerminalNode(Symbols.Vector, currentNode));
                    }
                    else
                    {
                        stack.Push(CreateNonTerminalNode(Symbols.ParameterEqualSingle, currentNode));
                    }
                }
                else if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Equal("(", true))
                {
                    if ((tokens.Length > currentTokenIndex + 3) && tokens[currentTokenIndex + 2].Lexem == ")" && tokens[currentTokenIndex + 3].Lexem == "=")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                    }
                    else if ((tokens.Length > currentTokenIndex + 4) && tokens[currentTokenIndex + 3].Lexem == ")" && tokens[currentTokenIndex + 4].Lexem == "=")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateNonTerminalNode(Symbols.ParameterSingle, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateNonTerminalNode(Symbols.Vector, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                    }
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.ParameterEqualSingle"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameterEqualSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            if (tokens[currentTokenIndex].SpiceTokenType == SpiceTokenType.WORD)
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.WORD, current),
                    CreateTerminalNode(SpiceTokenType.EQUAL, current),
                    CreateNonTerminalNode(Symbols.ParameterSingle, current));
            }
            else
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.IDENTIFIER, current),
                    CreateTerminalNode(SpiceTokenType.EQUAL, current),
                    CreateNonTerminalNode(Symbols.ParameterSingle, current));
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.ParameterBracketContent"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameterBracketContent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(Symbols.Parameters, currentNode));
        }

        /// <summary>
        /// Reads <see cref="Symbols.Parameter"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameter(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentTokenIndex == tokens.Length - 1)
            {
                if (currentToken.Is(SpiceTokenType.VALUE)
                        || currentToken.Is(SpiceTokenType.SINGLE_QUOTED_STRING)
                        || currentToken.Is(SpiceTokenType.DOUBLE_QUOTED_STRING)
                        || currentToken.Is(SpiceTokenType.IDENTIFIER)
                        || currentToken.Is(SpiceTokenType.REFERENCE)
                        || currentToken.Is(SpiceTokenType.EXPRESSION)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                        || currentToken.Is(SpiceTokenType.SUFFIX)
                        || currentToken.Is(SpiceTokenType.PREFIX_SINGLE)
                        || currentToken.Is(SpiceTokenType.PREFIX_COMPLEX)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                        || currentToken.Is(SpiceTokenType.PERCENT))
                {
                    PushProductionExpression(
                        stack,
                        CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                    return;
                }
                else
                {
                    throw new ParseException("Error during parsing a parameter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }

            if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == "(")
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.Point, currentNode));
                return;
            }

            var nextToken = tokens[currentTokenIndex + 1];

            if (nextToken.Is(SpiceTokenType.COMMA))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.Vector, currentNode));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER))
                {
                    if (nextToken.Is(SpiceTokenType.EQUAL))
                    {
                        stack.Push(CreateNonTerminalNode(Symbols.ParameterEqual, currentNode));
                    }
                    else if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Equal("(", true))
                    {
                        if (IsEqualTokens(tokens, currentTokenIndex))
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(Symbols.ParameterEqual, currentNode));
                        }
                        else
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(Symbols.ParameterBracket, currentNode));
                        }
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                    }
                }
                else
                {
                    if (currentToken.Is(SpiceTokenType.VALUE)
                        || currentToken.Is(SpiceTokenType.SINGLE_QUOTED_STRING)
                        || currentToken.Is(SpiceTokenType.DOUBLE_QUOTED_STRING)
                        || currentToken.Is(SpiceTokenType.IDENTIFIER)
                        || currentToken.Is(SpiceTokenType.REFERENCE)
                        || currentToken.Is(SpiceTokenType.EXPRESSION)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                        || currentToken.Is(SpiceTokenType.SUFFIX)
                        || currentToken.Is(SpiceTokenType.PREFIX_SINGLE)
                        || currentToken.Is(SpiceTokenType.PREFIX_COMPLEX)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                        || currentToken.Is(SpiceTokenType.PERCENT))
                    {
                        if (currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                            || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES))
                        {
                            if (nextToken.Is(SpiceTokenType.EQUAL))
                            {
                                PushProductionExpression(
                                    stack,
                                    CreateNonTerminalNode(Symbols.ExpressionEqual, currentNode));
                            }
                            else
                            if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Lexem == "(")
                            {
                                PushProductionExpression(
                                    stack,
                                    CreateNonTerminalNode(Symbols.ExpressionEqual, currentNode));
                            }
                            else
                            {
                                PushProductionExpression(
                                    stack,
                                    CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                            }
                        }
                        else
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(Symbols.ParameterSingle, currentNode));
                        }
                    }
                    else
                    {
                        throw new ParseException("Error during parsing a parameter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.ParameterBracket"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameterBracket(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                    CreateNonTerminalNode(Symbols.ParameterBracketContent, currentNode),
                    CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"));
            }
            else
            {
                throw new ParseException("Error during parsing a bracket parameter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.ParameterSingle"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="current">A reference to the non-terminal node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadParameterSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.VALUE)
                || currentToken.Is(SpiceTokenType.SINGLE_QUOTED_STRING)
                || currentToken.Is(SpiceTokenType.DOUBLE_QUOTED_STRING)
                || currentToken.Is(SpiceTokenType.IDENTIFIER)
                || currentToken.Is(SpiceTokenType.REFERENCE)
                || currentToken.Is(SpiceTokenType.EXPRESSION)
                || currentToken.Is(SpiceTokenType.SUFFIX)
                || currentToken.Is(SpiceTokenType.PREFIX_SINGLE)
                || currentToken.Is(SpiceTokenType.PREFIX_COMPLEX)
                || currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                || currentToken.Is(SpiceTokenType.PERCENT))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, current));
            }
            else
            {
                throw new ParseException("Error during parsing a parameter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Model"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadModel(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.MODEL) && (nextToken.Is(SpiceTokenType.WORD) || nextToken.Is(SpiceTokenType.IDENTIFIER)))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(Symbols.Parameters, currentNode));
            }
            else
            {
                throw new ParseException("Error during parsing a model, line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Distribution"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadDistribution(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];
            var nextNextToken = tokens[currentTokenIndex + 2];

            if (currentToken.Is(SpiceTokenType.DOT)
                && nextToken.Is(SpiceTokenType.WORD)
                && nextToken.Equal("DISTRIBUTION", IsDotStatementNameCaseSensitive)
                && (nextNextToken.Is(SpiceTokenType.WORD) || nextNextToken.Is(SpiceTokenType.IDENTIFIER)))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextNextToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(Symbols.Parameters, currentNode));
            }
            else
            {
                throw new ParseException("Error during parsing a distribution, line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Control"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadControl(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.DOT) && nextToken.Is(SpiceTokenType.WORD))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(Symbols.Parameters, currentNode));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.ENDL))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                        CreateNonTerminalNode(Symbols.Parameters, currentNode));
                }
                else if (currentToken.Is(SpiceTokenType.IF))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                        CreateTerminalNode(SpiceTokenType.BOOLEAN_EXPRESSION, currentNode));
                }
                else if (currentToken.Is(SpiceTokenType.ELSE_IF))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                        CreateTerminalNode(SpiceTokenType.BOOLEAN_EXPRESSION, currentNode));
                }
                else if (currentToken.Is(SpiceTokenType.ELSE))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode));
                }
                else if (currentToken.Is(SpiceTokenType.ENDIF))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode));
                }
                else
                {
                    throw new ParseException("Error during parsing a control. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.Component"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed.</param>
        /// <param name="currentNode">A reference to the current node.</param>
        /// <param name="tokens">A reference to the array of tokens.</param>
        /// <param name="currentTokenIndex">A index of the current token.</param>
        private void ReadComponent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER) || currentToken.Is(SpiceTokenType.SUFFIX))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(Symbols.Parameters, currentNode));
            }
            else
            {
                throw new ParseException("Error during parsing a component. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Creates a new non-terminal node.
        /// </summary>
        /// <param name="symbolName">A name of non-terminal.</param>
        /// <param name="parent">A parent of the new non-terminal node.</param>
        /// <returns>
        /// A new instance of <see cref="ParseTreeNonTerminalNode"/>.
        /// </returns>
        private ParseTreeNonTerminalNode CreateNonTerminalNode(string symbolName, ParseTreeNonTerminalNode parent)
        {
            if (parent == null)
            {
                return new ParseTreeNonTerminalNode(symbolName);
            }

            var node = new ParseTreeNonTerminalNode(parent, symbolName);
            parent.Children.Add(node);

            return node;
        }

        /// <summary>
        /// Creates a new terminal node.
        /// </summary>
        /// <param name="tokenType">A type of the token.</param>
        /// <param name="parent">A parent of the new terminal node.</param>
        /// <param name="tokenValue">An expected lexem for the terminal node.</param>
        /// <returns>
        /// A new instance of <see cref="ParseTreeTerminalNode"/>.
        /// </returns>
        private ParseTreeTerminalNode CreateTerminalNode(SpiceTokenType tokenType, ParseTreeNonTerminalNode parent, string tokenValue = null)
        {
            var node = new ParseTreeTerminalNode(new SpiceToken(tokenType, tokenValue), parent);
            parent.Children.Add(node);
            return node;
        }

        /// <summary>
        /// Pushes grammar production expression to stack.
        /// </summary>
        /// <param name="stack">A stack.</param>
        /// <param name="expression">An expression of production.</param>
        private void PushProductionExpression(Stack<ParseTreeNode> stack, params ParseTreeNode[] expression)
        {
            for (var i = expression.Length - 1; i >= 0; i--)
            {
                stack.Push(expression[i]);
            }
        }
    }
}