using System.Collections.Generic;
using SpiceSharpParser.Grammar;
using SpiceSharpParser.Lexer.Spice3f5;
using SpiceSharpParser.Parser.Exceptions;

namespace SpiceSharpParser.Parser.TreeGeneration
{
    /// <summary>
    /// A parser tree generator for Spice netlist based on grammar from SpiceGrammar.txt.
    /// It's a manualy written LL(*) parser.
    /// </summary>
    public class ParserTreeGenerator
    {
        /// <summary>
        /// Generates a parse tree for SPICE grammar
        /// </summary>
        /// <param name="tokens">An array of tokens</param>
        /// <param name="rootSymbol">A root symbol of parse tree</param>
        /// <returns>
        /// A parse tree
        /// </returns>
        public ParseTreeNonTerminalNode GetParseTree(SpiceToken[] tokens, string rootSymbol = SpiceGrammarSymbol.NETLIST)
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
                    switch (ntn.Name)
                    {
                        case SpiceGrammarSymbol.NETLIST:
                            ProcessNetlist(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.NETLIST_WITHOUT_TITLE:
                            ProcessNetlistWithoutTitle(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.NETLIST_ENDING:
                            ProcessNetlistEnding(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.STATEMENTS:
                            ProcessStatements(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.STATEMENT:
                            ProcessStatement(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.COMMENT_LINE:
                            ProcessCommentLine(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.SUBCKT:
                            ProcessSubckt(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.SUBCKT_ENDING:
                            ProcessSubcktEnding(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.COMPONENT:
                            ProcessComponent(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.CONTROL:
                            ProcessControl(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.MODEL:
                            ProcessModel(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETERS:
                            ProcessParameters(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER:
                            ProcessParameter(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_SINGLE:
                            ProcessParameterSingle(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_BRACKET:
                            ProcessParameterBracket(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT:
                            ProcessParameterBracketContent(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_EQUAL:
                            ProcessParameterEqual(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE:
                            ProcessParameterEqualSingle(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.VECTOR:
                            ProcessVector(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.VECTOR_CONTINUE:
                            ProcessVectorContinue(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.NEW_LINE:
                            ProcessNewLine(stack, ntn, tokens, currentTokenIndex);
                            break;
                    }
                }

                if (currentNode is ParseTreeTerminalNode tn)
                {
                    if (currentTokenIndex >= tokens.Length)
                    {
                        throw new ParsingException("End of tokens. Expected token type: " + tn.Token.SpiceTokenType + " line=" + tokens[tokens.Length - 1].LineNumber, tokens[tokens.Length - 1].LineNumber);
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
                        throw new ParsingException(string.Format("Unexpected token: '{0}' of type: {1}. Expected token type: {2} line={3}", tokens[currentTokenIndex].Lexem, tokens[currentTokenIndex].SpiceTokenType, tn.Token.SpiceTokenType, tokens[currentTokenIndex].LineNumber), tokens[currentTokenIndex].LineNumber);
                    }
                }
            }

            if (currentTokenIndex != tokens.Length)
            {
                throw new ParsingException("There are pending tokens to process", tokens[currentTokenIndex].LineNumber);
            }

            return root;
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.SUBCKT_ENDING"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessSubcktEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
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
                throw new ParsingException("Error during parsing subcircuit. Expected .ENDS. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.NEW_LINE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessNewLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                throw new ParsingException("End of tokens. New line not found", tokens[tokens.Length - 1].LineNumber);
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
                throw new ParsingException("Newline was expected. Other token was found.", currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.NETLIST"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessNetlist(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.NETLIST_ENDING, currentNode)
                );
            }
            else if (currentToken.Is(SpiceTokenType.ASTERIKS))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.ASTERIKS, currentNode),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.NETLIST_ENDING, currentNode)
                );
            }
            else
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(SpiceTokenType.TITLE, currentNode),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.NETLIST_ENDING, currentNode)
                );
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.NETLIST_WITHOUT_TITLE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessNetlistWithoutTitle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, currentNode),
                CreateNonTerminalNode(SpiceGrammarSymbol.NETLIST_ENDING, currentNode)
            );
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.NETLIST_ENDING"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessNetlistEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.END))
            {
                if (currentTokenIndex + 1 < tokens.Length)
                {
                    if (tokens[currentTokenIndex + 1].Is(SpiceTokenType.EOF))
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.END, current),
                            CreateTerminalNode(SpiceTokenType.EOF, current));
                    }

                    if (tokens[currentTokenIndex + 1].Is(SpiceTokenType.NEWLINE))
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.END, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                            CreateTerminalNode(SpiceTokenType.EOF, current));
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
                    throw new ParsingException("Netlist ending - wrong ending", currentToken.LineNumber);
                }
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.STATEMENTS"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessStatements(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.DOT)
                || currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.COMMENT))
            {
                PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENT, current),
                            CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, current));
            }
            else if (currentToken.Is(SpiceTokenType.NEWLINE))
            {
                PushProductionExpression(
                         stack,
                         CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                         CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, current));
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
                throw new ParsingException("Error during parsing statements. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.STATEMENT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessStatement(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.WORD))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(SpiceGrammarSymbol.COMPONENT, current),
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
                            CreateNonTerminalNode(SpiceGrammarSymbol.SUBCKT, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                    else if (nextToken.Equal("model", true))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(SpiceGrammarSymbol.MODEL, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(SpiceGrammarSymbol.CONTROL, current),
                            CreateTerminalNode(SpiceTokenType.NEWLINE, current));
                    }
                }
                else
                {
                    throw new ParsingException("Error during parsing a statement. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }
            else if (currentToken.Is(SpiceTokenType.COMMENT))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(SpiceGrammarSymbol.COMMENT_LINE, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current));
            }
            else
            {
                throw new ParsingException(string.Format("Error during parsing a statement. Unexpected token: '{0}' of type:{1} line={2}", currentToken.Lexem, currentToken.SpiceTokenType, currentToken.LineNumber), currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.VECTOR"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessVector(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current),
                CreateTerminalNode(SpiceTokenType.COMMA, current, ","),
                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current),
                CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR_CONTINUE, current));
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.VECTOR_CONTINUE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessVectorContinue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
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
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current),
                    CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR_CONTINUE, current));
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.COMMENT_LINE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessCommentLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.COMMENT, current));
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.SUBCKT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessSubckt(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
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
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current),
                    CreateTerminalNode(SpiceTokenType.NEWLINE, current),
                    CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, current),
                    CreateNonTerminalNode(SpiceGrammarSymbol.SUBCKT_ENDING, current));
            }
            else
            {
                throw new ParsingException("Error during parsing a subcircuit. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETERS"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameters(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            if (currentTokenIndex > tokens.Length - 1)
            {
                // empty
                return;
            }

            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.VALUE)
                || currentToken.Is(SpiceTokenType.STRING)
                || currentToken.Is(SpiceTokenType.IDENTIFIER)
                || currentToken.Is(SpiceTokenType.REFERENCE)
                || currentToken.Is(SpiceTokenType.EXPRESSION))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER, current),
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current));
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
                throw new ParsingException(string.Format("Error during parsing parameters. Unexpected token: '{0}' of type {1} line={2}", currentToken.Lexem,  currentToken.SpiceTokenType, currentToken.LineNumber), currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameterEqual(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.WORD))
            {
                if (nextToken.Is(SpiceTokenType.EQUAL))
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, currentNode));
                }
                else if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Equal("(", true))
                {
                    if ((tokens.Length > currentTokenIndex + 4) && tokens[currentTokenIndex + 3].Lexem == ")" && tokens[currentTokenIndex + 4].Lexem == "=")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.WORD, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode));
                    }

                    if ((tokens.Length > currentTokenIndex + 6) && tokens[currentTokenIndex + 5].Lexem == ")" && tokens[currentTokenIndex + 6].Lexem == "=")
                    {
                        PushProductionExpression(
                            stack,
                            CreateTerminalNode(SpiceTokenType.WORD, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode),
                            CreateTerminalNode(SpiceTokenType.COMMA, currentNode, ","),
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode),
                            CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"),
                            CreateTerminalNode(SpiceTokenType.EQUAL, currentNode),
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode));
                    }
                }
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameterEqualSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.WORD, current),
                CreateTerminalNode(SpiceTokenType.EQUAL, current),
                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameterBracketContent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, currentNode));
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETER"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameter(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentTokenIndex == tokens.Length - 1)
            {
                if (currentToken.Is(SpiceTokenType.VALUE)
                        || currentToken.Is(SpiceTokenType.STRING)
                        || currentToken.Is(SpiceTokenType.IDENTIFIER)
                        || currentToken.Is(SpiceTokenType.REFERENCE)
                        || currentToken.Is(SpiceTokenType.EXPRESSION))
                {
                    PushProductionExpression(
                        stack,
                        CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode));
                    return;
                }
                else
                {
                    throw new ParsingException("Error during parsing a paremeter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                }
            }

            var nextToken = tokens[currentTokenIndex + 1];

            if (nextToken.Is(SpiceTokenType.COMMA))
            {
                PushProductionExpression(
                    stack,
                    CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR, currentNode));
            }
            else
            {
                if (currentToken.Is(SpiceTokenType.WORD))
                {
                    if (nextToken.Is(SpiceTokenType.EQUAL))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL, currentNode));
                    }
                    else if (nextToken.Is(SpiceTokenType.DELIMITER) && nextToken.Equal("(", true))
                    {
                        if (((tokens.Length > currentTokenIndex + 4) && tokens[currentTokenIndex + 3].Lexem == ")" && tokens[currentTokenIndex + 4].Lexem == "=")
                            || ((tokens.Length > currentTokenIndex + 6) && tokens[currentTokenIndex + 5].Lexem == ")" && tokens[currentTokenIndex + 6].Lexem == "="))
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL, currentNode));
                        }
                        else
                        {
                            PushProductionExpression(
                                stack,
                                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_BRACKET, currentNode));
                        }
                    }
                    else
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode));
                    }
                }
                else
                {
                    if (currentToken.Is(SpiceTokenType.VALUE)
                        || currentToken.Is(SpiceTokenType.STRING)
                        || currentToken.Is(SpiceTokenType.IDENTIFIER)
                        || currentToken.Is(SpiceTokenType.REFERENCE)
                        || currentToken.Is(SpiceTokenType.EXPRESSION))
                    {
                        PushProductionExpression(
                            stack,
                            CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, currentNode));
                    }
                    else
                    {
                        throw new ParsingException("Error during parsing a paremeter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETER_BRACKET"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameterBracket(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            PushProductionExpression(
                stack,
                CreateTerminalNode(SpiceTokenType.WORD, currentNode),
                CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, "("),
                CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT, currentNode),
                CreateTerminalNode(SpiceTokenType.DELIMITER, currentNode, ")"));
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.PARAMETER_SINGLE"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="current">A reference to the non-terminal node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessParameterSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD)
                || currentToken.Is(SpiceTokenType.VALUE)
                || currentToken.Is(SpiceTokenType.STRING)
                || currentToken.Is(SpiceTokenType.IDENTIFIER)
                || currentToken.Is(SpiceTokenType.REFERENCE)
                || currentToken.Is(SpiceTokenType.EXPRESSION))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, current));
            }
            else
            {
                throw new ParsingException("Error during parsing a paremeter. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.MODEL"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessModel(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
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
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, currentNode));
            }
            else
            {
                throw new ParsingException("Error during parsing a model, line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.CONTROL"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessControl(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceTokenType.DOT) && nextToken.Is(SpiceTokenType.WORD))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateTerminalNode(nextToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, currentNode));
            }
            else
            {
                throw new ParsingException("Error during parsing a control. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
            }
        }

        /// <summary>
        /// Processes <see cref="SpiceGrammarSymbol.COMPONENT"/> non-terminal node
        /// Pushes tree nodes to the stack based on the grammar.
        /// </summary>
        /// <param name="stack">A stack where the production is pushed</param>
        /// <param name="currentNode">A reference to the current node</param>
        /// <param name="tokens">A reference to the array of tokens</param>
        /// <param name="currentTokenIndex">A index of the current token</param>
        private void ProcessComponent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, SpiceToken[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceTokenType.WORD))
            {
                PushProductionExpression(
                    stack,
                    CreateTerminalNode(currentToken.SpiceTokenType, currentNode),
                    CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, currentNode));
            }
            else
            {
                throw new ParsingException("Error during parsing a component. Unexpected token: '" + currentToken.Lexem + "'" + " line=" + currentToken.LineNumber, currentToken.LineNumber);
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
