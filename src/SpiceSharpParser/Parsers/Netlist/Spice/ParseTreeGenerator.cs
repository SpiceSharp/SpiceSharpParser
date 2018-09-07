using System;
using System.Collections.Generic;
using SpiceSharpParser.Lexers.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// A parser tree generator for Spice netlist based on grammar from SpiceGrammar.txt.
    /// It's a manualy written LL(*) parser.
    /// </summary>
    public class ParseTreeGenerator
    {
        private Dictionary<string, Action<Stack<ParseTreeNode>, ParseTreeNonTerminalNode, SpiceToken[], int>> parsers = new Dictionary<string, Action<Stack<ParseTreeNode>, ParseTreeNonTerminalNode, SpiceToken[], int>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeGenerator"/> class.
        /// </summary>
        /// <param name="isNewLineRequiredAtTheEnd">Is NEWLINE required at the end?</param>
        public ParseTreeGenerator(bool isNewLineRequiredAtTheEnd = false)
        {
            IsNewLineRequiredAtTheEnd = isNewLineRequiredAtTheEnd;

            parsers.Add(Symbols.NETLIST, ReadNetlist);
            parsers.Add(Symbols.NETLIST_WITHOUT_TITLE, ReadNetlistWithoutTitle);
            parsers.Add(Symbols.NETLIST_ENDING, ReadNetlistEnding);
            parsers.Add(Symbols.STATEMENTS, ReadStatements);
            parsers.Add(Symbols.STATEMENT, ReadStatement);
            parsers.Add(Symbols.COMMENT_LINE, ReadCommentLine);
            parsers.Add(Symbols.SUBCKT, ReadSubckt);
            parsers.Add(Symbols.SUBCKT_ENDING, ReadSubcktEnding);
            parsers.Add(Symbols.COMPONENT, ReadComponent);
            parsers.Add(Symbols.CONTROL, ReadControl);
            parsers.Add(Symbols.MODEL, ReadModel);
            parsers.Add(Symbols.PARAMETERS, ReadParameters);
            parsers.Add(Symbols.PARAMETER, ReadParameter);
            parsers.Add(Symbols.PARAMETER_SINGLE, ReadParameterSingle);
            parsers.Add(Symbols.PARAMETER_BRACKET, ReadParameterBracket);
            parsers.Add(Symbols.PARAMETER_BRACKET_CONTENT, ReadParameterBracketContent);
            parsers.Add(Symbols.PARAMETER_EQUAL, ReadParameterEqual);
            parsers.Add(Symbols.PARAMETER_EQUAL_SINGLE, ReadParameterEqualSingle);
            parsers.Add(Symbols.VECTOR, ReadVector);
            parsers.Add(Symbols.VECTOR_CONTINUE, ReadVectorContinue);
            parsers.Add(Symbols.NEW_LINE, ReadNewLine);
        }

        protected bool IsNewLineRequiredAtTheEnd { get; }

        /// <summary>
        /// Generates a parse tree for SPICE grammar.
        /// </summary>
        /// <param name="tokens">An array of tokens.</param>
        /// <param name="rootSymbol">A root symbol of parse tree.</param>
        /// <returns>
        /// A parse tree.
        /// </returns>
        public ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens, string rootSymbol = Symbols.NETLIST)
        {
            if (tokens == null)
            {
                throw new System.ArgumentNullException(nameof(tokens));
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
                    if (parsers.ContainsKey(ntn.Name))
                    {
                        parsers[ntn.Name](stack, ntn, tokens, currentTokenIndex);
                    }
                    else
                    {
                        throw new ParseException("Unknown non-terminal found while parsing." + ntn.Name, tokens[currentTokenIndex].LineNumber);
                    }
                }

                if (currentNode is ParseTreeTerminalNode tn)
                {
                    if (currentTokenIndex >= tokens.Length)
                    {
                        throw new ParseException("End of tokens. Expected token type: " + tn.Token.SpiceTokenType + " line=" + tokens[tokens.Length - 1].LineNumber, tokens[tokens.Length - 1].LineNumber);
                    }

                    if (tn.Token.SpiceTokenType == tokens[currentTokenIndex].SpiceTokenType
                        && (tn.Token.Lexem == null || tn.Token.Lexem == tokens[currentTokenIndex].Lexem))
                    {
                        tn.Token.UpdateLexem(tokens[currentTokenIndex].Lexem);
                        tn.Token.UpdateLineNumber(tokens[currentTokenIndex].LineNumber);
                        currentTokenIndex++;
                    }
                    else
                    {
                        throw new ParseException(string.Format("Unexpected token: '{0}' of type: {1}. Expected token type: {2} line={3}", tokens[currentTokenIndex].Lexem, tokens[currentTokenIndex].SpiceTokenType, tn.Token.SpiceTokenType, tokens[currentTokenIndex].LineNumber), tokens[currentTokenIndex].LineNumber);
                    }
                }
            }

            if (currentTokenIndex != tokens.Length)
            {
                throw new ParseException("There are pending tokens to process", tokens[currentTokenIndex].LineNumber);
            }

            return root;
        }

        /// <summary>
        /// Reads <see cref="Symbols.SUBCKT_ENDING"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadSubcktEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.ENDS))
            {
                if (nextToken.Is(SpiceTokenType.WORD))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(SpiceTokenType.ENDS, currentNode),
                        CreateTerminalNode(SpiceTokenType.WORD, currentNode));
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
        /// Reads <see cref="Symbols.NEW_LINE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
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
        /// Reads <see cref="Symbols.NETLIST"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadNetlist(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.EOF))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.NETLIST_ENDING, currentNode)
                );
                return;
            }

            if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                    CreateNonTerminalNode(Symbols.STATEMENTS, currentNode),
                    CreateNonTerminalNode(Symbols.NETLIST_ENDING, currentNode)
                );
            }
            else
            {
                if (tokens[currentTokenIndex + 1].Is(SpiceTokenType.EOF) && !IsNewLineRequiredAtTheEnd)
                {
                    PushProductionExpression(
                     stack,
                     CreateTerminalNode(SpiceTokenType.TITLE, currentNode),
                     CreateNonTerminalNode(Symbols.NETLIST_ENDING, currentNode)
                 );
                }
                else
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(SpiceTokenType.TITLE, currentNode),
                        CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                        CreateNonTerminalNode(Symbols.STATEMENTS, currentNode),
                        CreateNonTerminalNode(Symbols.NETLIST_ENDING, currentNode)
                    );
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.NETLIST_WITHOUT_TITLE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadNetlistWithoutTitle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(Symbols.STATEMENTS, currentNode),
                CreateNonTerminalNode(Symbols.NETLIST_ENDING, currentNode)
            );
        }

        /// <summary>
        /// Reads <see cref="Symbols.NETLIST_ENDING"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadNetlistEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.END))
            {
                if (IsNewLineRequiredAtTheEnd)
                {
                    PushProductionExpression(
                                stack,
                                CreateTerminalNode(SpiceTokenType.END, current),
                                CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                                CreateTerminalNode(SpiceTokenType.EOF, current));
                }
                else
                {
                    if (currentTokenIndex + 1 < tokens.Length)
                    {
                        if (tokens[currentTokenIndex + 1].Is(SpiceTokenType.NEWLINE))
                        {
                            PushProductionExpression(
                                stack,
                                CreateTerminalNode(SpiceTokenType.END, current),
                                CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                                CreateTerminalNode(SpiceTokenType.EOF, current));
                        }
                        else
                        {
                            if (tokens[currentTokenIndex + 1].Is(SpiceTokenType.EOF))
                            {
                                PushProductionExpression(
                                    stack,
                                    CreateTerminalNode(SpiceTokenType.END, current),
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
                        PushProductionExpression(
                           stack,
                           CreateTerminalNode(SpiceTokenType.END, current));
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
                    throw new ParseException("Netlist ending - wrong ending", currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.STATEMENTS"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadStatements(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.DOT)
                || currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.COMMENT)
                || currentToken.Is(SpiceTokenType.ENDL)
                || currentToken.Is(SpiceTokenType.IF)
                || currentToken.Is(SpiceTokenType.ELSE)
                || currentToken.Is(SpiceTokenType.ELSE_IF)
                || currentToken.Is(SpiceTokenType.ENDIF))
            {
                PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.STATEMENT, current),
                            CreateNonTerminalNode(Symbols.STATEMENTS, current));
            }
            else if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                         stack,
                         CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                         CreateNonTerminalNode(Symbols.STATEMENTS, current));
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
                throw new ParseException("Error during parsing statements. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.STATEMENT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadStatement(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.WORD))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.COMPONENT, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current));
            }
            else if (currentToken.Is(SpiceTokenType.DOT))
            {
                if (nextToken.Is(SpiceTokenType.WORD))
                {
                    if (nextToken.Equal("subckt", true))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.SUBCKT, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                    else if (nextToken.Equal("model", true))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.MODEL, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.CONTROL, current),
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
                    CreateNonTerminalNode(Symbols.COMMENT_LINE, current),
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
                           CreateNonTerminalNode(Symbols.CONTROL, current),
                           CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                }
                else
                {
                    throw new ParseException(string.Format("Error during parsing a statement. Unexpected token: '{0}' of type:{1} line={2}", currentToken.Lexem, currentToken.SpiceTokenType, currentToken.LineNumber), currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.VECTOR"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadVector(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, current),
                CreateTerminalNode(SpiceTokenType.COMMA, current, ","),
                CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, current),
                CreateNonTerminalNode(Symbols.VECTOR_CONTINUE, current));
        }

        /// <summary>
        /// Reads <see cref="Symbols.VECTOR_CONTINUE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadVectorContinue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                return; // empty
            }

            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.DELIMITER) && currentToken.Lexem == ")")
            {
                // follow
            }
            else
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.COMMA, current, ","),
                    CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, current),
                    CreateNonTerminalNode(Symbols.VECTOR_CONTINUE, current));
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.COMMENT_LINE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadCommentLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.COMMENT, current));
        }

        /// <summary>
        /// Reads <see cref="Symbols.SUBCKT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadSubckt(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.DOT)
                && nextToken.Is(SpiceTokenType.WORD)
                && nextToken.Equal("subckt", true))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, current, currentToken.Lexem),
                    CreateTerminalNode(nextToken.SpiceTokenType, current, nextToken.Lexem),
                    CreateTerminalNode(SpiceTokenType.WORD, current),
                    CreateNonTerminalNode(Symbols.PARAMETERS, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                    CreateNonTerminalNode(Symbols.STATEMENTS, current),
                    CreateNonTerminalNode(Symbols.SUBCKT_ENDING, current));
            }
            else
            {
                throw new ParseException("Error during parsing a subcircuit. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.PARAMETERS"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
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
                || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                || currentToken.Is(SpiceTokenType.PERCENT))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.PARAMETER, current),
                    CreateNonTerminalNode(Symbols.PARAMETERS, current));
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
                    string.Format("Error during parsing parameters. Unexpected token: '{0}' of type {1} line={2}", currentToken.Lexem,  currentToken.SpiceTokenType, currentToken.LineNumber), currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.PARAMETER_EQUAL"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadParameterEqual(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.IDENTIFIER))
            {
                if (nextToken.Is(SpiceTokenType.EQUAL))
                {
                    stack.Push(CreateNonTerminalNode(Symbols.PARAMETER_EQUAL_SINGLE, currentNode));
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
                            CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode));
                    }
                    else if ((tokens.Length > currentTokenIndex + 4) && tokens[currentTokenIndex + 3].Lexem == ")" && tokens[currentTokenIndex + 4].Lexem == "=")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateNonTerminalNode(Symbols.VECTOR, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode));
                    }
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.PARAMETER_EQUAL_SINGLE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadParameterEqualSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.WORD, current),
                CreateTerminalNode(SpiceTokenType.EQUAL, current),
                CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, current));
        }

        /// <summary>
        /// Reads <see cref="Symbols.PARAMETER_BRACKET_CONTENT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadParameterBracketContent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(Symbols.PARAMETERS, currentNode));
        }

        /// <summary>
        /// Reads <see cref="Symbols.PARAMETER"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
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
                        || currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                        || currentToken.Is(SpiceTokenType.PERCENT))
                {
                    PushProductionExpression(
                        stack,
                        CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode));
                    return;
                }
                else
                {
                    throw new ParseException("Error during parsing a paremeter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }

            var nextToken = tokens[currentTokenIndex + 1];

            if (nextToken.Is(SpiceTokenType.COMMA))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(Symbols.VECTOR, currentNode));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER))
                {
                    if (nextToken.Is(SpiceTokenType.EQUAL))
                    {
                        stack.Push(CreateNonTerminalNode(Symbols.PARAMETER_EQUAL, currentNode));
                    }
                    else if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Equal("(", true))
                    {
                        if (IsEqualTokens(tokens, currentTokenIndex))
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(Symbols.PARAMETER_EQUAL, currentNode));
                        }
                        else
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(Symbols.PARAMETER_BRACKET, currentNode));
                        }
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode));
                    }
                }
                else
                {
                    if (currentToken.Is(SpiceTokenType.VALUE)
                        || currentToken.Is(SpiceTokenType.SINGLE_QUOTED_STRING)
                        || currentToken.Is(SpiceTokenType.DOUBLE_QUOTED_STRING)
                        || currentToken.Is(SpiceTokenType.IDENTIFIER)
                        || currentToken.Is(SpiceTokenType.REFERENCE)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_BRACKET)
                        || currentToken.Is(SpiceTokenType.EXPRESSION_SINGLE_QUOTES)
                        || currentToken.Is(SpiceTokenType.PERCENT))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(Symbols.PARAMETER_SINGLE, currentNode));
                    }
                    else
                    {
                        throw new ParseException("Error during parsing a paremeter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                    }
                }
            }
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
        /// Reads <see cref="Symbols.PARAMETER_BRACKET"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadParameterBracket(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD) || currentToken.Is(SpiceTokenType.IDENTIFIER))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                    CreateNonTerminalNode(Symbols.PARAMETER_BRACKET_CONTENT, currentNode),
                    CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"));
            }
            else
            {
                throw new ParseException("Error during parsing a bracket parameter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.PARAMETER_SINGLE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadParameterSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.VALUE)
                || currentToken.Is(SpiceTokenType.SINGLE_QUOTED_STRING)
                || currentToken.Is(SpiceTokenType.DOUBLE_QUOTED_STRING)
                || currentToken.Is(SpiceTokenType.IDENTIFIER)
                || currentToken.Is(SpiceTokenType.REFERENCE)
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
                throw new ParseException("Error during parsing a paremeter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.MODEL"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadModel(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];
            var nextNextToken = tokens[currentTokenIndex + 2];

            if (currentToken.Is(SpiceTokenType.DOT)
                && nextToken.Is(SpiceTokenType.WORD)
                && nextToken.Equal("model", true)
                && (nextNextToken.Is(SpiceTokenType.WORD) || nextNextToken.Is(SpiceTokenType.IDENTIFIER)))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextNextToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(Symbols.PARAMETERS, currentNode));
            }
            else
            {
                throw new ParseException("Error during parsing a model, line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.CONTROL"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
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
                    CreateNonTerminalNode(Symbols.PARAMETERS, currentNode));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.ENDL))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                        CreateNonTerminalNode(Symbols.PARAMETERS, currentNode));
                }
                else if (currentToken.Is(SpiceTokenType.IF))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                        CreateTerminalNode(SpiceTokenType.BOOLEAN_EXPRESSION, currentNode)
                    );
                }
                else if (currentToken.Is(SpiceTokenType.ELSE_IF))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                        CreateTerminalNode(SpiceTokenType.BOOLEAN_EXPRESSION, currentNode)
                    );
                }
                else if (currentToken.Is(SpiceTokenType.ELSE))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode)
                    );
                }
                else if (currentToken.Is(SpiceTokenType.ENDIF))
                {
                    PushProductionExpression(
                        stack,
                        CreateTerminalNode(currentToken.SpiceTokenType, currentNode)
                    );
                }
                else
                {
                    throw new ParseException("Error during parsing a control. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Reads <see cref="Symbols.COMPONENT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ReadComponent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(Symbols.PARAMETERS, currentNode));
            }
            else
            {
                throw new ParseException("Error during parsing a component. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Creates a new non-terminal node
        /// </summary>
        /// <param name="symbolName">A name of non-terminal</param>
        /// <param name="parent">A parent of the new non-terminal node</param>
        /// <returns>
        /// A new instance of <see cref="ParseTreeNonTerminalNode"/>
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
        /// Creates a new terminal node
        /// </summary>
        /// <param name="tokenType">A type of the token</param>
        /// <param name="parent">A parent of the new terminal node</param>
        /// <param name="tokenValue">An expected lexem for the terminal node</param>
        /// <returns>
        /// A new instance of <see cref="ParseTreeTerminalNode"/>
        /// </returns>
        private ParseTreeTerminalNode CreateTerminalNode(SpiceTokenType tokenType, ParseTreeNonTerminalNode parent, string tokenValue = null)
        {
            var node = new ParseTreeTerminalNode(new SpiceToken(tokenType, tokenValue), parent);
            parent.Children.Add(node);
            return node;
        }

        /// <summary>
        /// Pushes grammar production expression to stack
        /// </summary>
        /// <param name="stack">A stack</param>
        /// <param name="expression">An expression of production</param>
        private void PushProductionExpression(Stack<ParseTreeNode> stack, params ParseTreeNode[] expression)
        {
            for (var i = expression.Length - 1; i >= 0; i--)
            {
                stack.Push(expression[i]);
            }
        }
    }
}
