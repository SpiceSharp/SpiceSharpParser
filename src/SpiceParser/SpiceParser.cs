using System.Collections.Generic;
using NLexer;
using SpiceNetlist;

namespace SpiceParser
{
    public class SpiceParser
    {
        /// <summary>
        /// Generates a parse tree for SPICE grammar
        /// </summary>
        /// <param name="tokens">An array of tokens</param>
        /// <returns>
        /// A parse tree
        /// </returns>
        public ParseTreeNode GetParseTree(Token[] tokens)
        {
            if (tokens == null)
            {
                throw new System.ArgumentNullException(nameof(tokens));
            }

            var stack = new Stack<ParseTreeNode>();

            var root = CreateNonTerminalNode(SpiceGrammarSymbol.START, null);
            stack.Push(root);

            int currentTokenIndex = 0;

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();
                if (currentNode is ParseTreeNonTerminalNode ntn)
                {
                    switch (ntn.Name)
                    {
                        case SpiceGrammarSymbol.START:
                            ProcessStartNode(stack, ntn, tokens, currentTokenIndex);
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
                        case SpiceGrammarSymbol.COMPONENT:
                            ProcessComponent(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.CONTROL:
                            ProcessControl(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.MODEL:
                            ProcessModel(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER:
                            ProcessParameter(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETERS:
                            ProcessParameters(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_SINGLE:
                            ProcessParameterSingle(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.NEW_LINE_OR_EOF:
                            ProcessNewLineOrEOF(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.SUBCKT_ENDING:
                            ProcessSubcktEnding(stack, ntn, tokens, currentTokenIndex);
                            break;
                    }
                }

                if (currentNode is ParseTreeTerminalNode tn)
                {
                    if (tn.Token.TokenType == tokens[currentTokenIndex].TokenType
                        && (tn.Token.Value == null || tn.Token.Value == tokens[currentTokenIndex].Value))
                    {
                        tn.Token.UpdateValue(tokens[currentTokenIndex].Value);
                        currentTokenIndex++;
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }

            return root;
        }

        private void ProcessSubcktEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.ENDS))
            {
                if (nextToken.Is(SpiceToken.WORD))
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, parent));
                    stack.Push(CreateTerminalNode((int)SpiceToken.WORD, parent));
                    stack.Push(CreateTerminalNode((int)SpiceToken.ENDS, parent));
                }
                else
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, parent));
                    stack.Push(CreateTerminalNode((int)SpiceToken.ENDS, parent));
                }
            }
            else
            {
                throw new ParseException("Wrong ending for .subckt");
            }
        }

        private void ProcessNewLineOrEOF(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parentNode, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentToken.Is(SpiceToken.EOF))
            {
                stack.Push(CreateTerminalNode((int)SpiceToken.EOF, parentNode));
            }

            if (currentToken.Is(SpiceToken.NEWLINE))
            {
                stack.Push(CreateTerminalNode((int)SpiceToken.NEWLINE, parentNode));
            }
        }

        private void ProcessStartNode(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parentNode, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, parentNode));
            stack.Push(CreateTerminalNode((int)SpiceToken.TITLE, parentNode));
        }

        private void ProcessStatements(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode statementsNode, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.DOT)
                || currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.ASTERIKS))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, statementsNode));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENT, statementsNode));
            }
            else if (currentToken.Is(SpiceToken.NEWLINE))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, statementsNode));
                stack.Push(CreateTerminalNode((int)SpiceToken.NEWLINE, statementsNode));
            }
            else if (currentToken.Is(SpiceToken.END))
            {
                stack.Push(CreateTerminalNode((int)SpiceToken.END, statementsNode));
            }
            else if (currentToken.Is(SpiceToken.EOF))
            {
                // do nothing
            }
            else if (currentToken.Is(SpiceToken.ENDS))
            {
                // do nothing
            }
            else
            {
                throw new ParseException("Error during parsing statements");
            }
        }

        private void ProcessStatement(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.COMPONENT, parent));
            }
            else if (currentToken.Is(SpiceToken.DOT))
            {
                if (nextToken.Is(SpiceToken.WORD))
                {
                    if (nextToken.Equal("subckt", true))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.SUBCKT, parent));
                    }
                    else if (nextToken.Equal("model", true))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.MODEL, parent));
                    }
                    else
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.CONTROL, parent));
                    }
                }
                else
                {
                    throw new ParseException("Error during parsing a statement");
                }
            }
            else if (currentToken.Is(SpiceToken.ASTERIKS))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.COMMENT_LINE, parent));
            }
        }

        private void ProcessCommentLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.ASTERIKS)
                && (nextToken.Is(SpiceToken.COMMENT)
                || nextToken.Is(SpiceToken.NEWLINE)
                || nextToken.Is(SpiceToken.EOF)))
            {
                stack.Push(CreateTerminalNode(nextToken.TokenType, parent, nextToken.Value));
                stack.Push(CreateTerminalNode(currentToken.TokenType, parent, currentToken.Value));
            }
            else
            {
                throw new ParseException("Error during parsing a comment");
            }
        }

        private void ProcessSubckt(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            Token currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.DOT)
                && nextToken.Is(SpiceToken.WORD)
                && nextToken.Equal("subckt", true))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.SUBCKT_ENDING, parent));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, parent));
                stack.Push(CreateTerminalNode((int)SpiceToken.NEWLINE, parent));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, parent));
                stack.Push(CreateTerminalNode((int)SpiceToken.WORD, parent));
                stack.Push(CreateTerminalNode(nextToken.TokenType, parent, nextToken.Value));
                stack.Push(CreateTerminalNode(currentToken.TokenType, parent, currentToken.Value));
            }
            else
            {
                throw new ParseException("Error during parsing a subckt");
            }
        }

        private void ProcessParameters(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.VALUE)
                || currentToken.Is(SpiceToken.STRING)
                || currentToken.Is(SpiceToken.IDENTIFIER)
                || currentToken.Is(SpiceToken.REFERENCE)
                || currentToken.Is(SpiceToken.EXPRESSION))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, parent));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER, parent));
            }
            else if (currentToken.Is(SpiceToken.EOF))
            {
                // do nothing
            }
            else if (currentToken.Is(SpiceToken.NEWLINE))
            {
                // do nothing
            }
            else if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Value == ")")
            {
                // do nothing
            }
            else
            {
                throw new ParseException("Error during parsing parameters");
            }
        }

        private void ProcessParameter(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                if (nextToken.Is(SpiceToken.EQUAL))
                {
                    stack.Push(CreateTerminalNode((int)SpiceToken.VALUE, parent));
                    stack.Push(CreateTerminalNode(nextToken.TokenType, parent));
                    stack.Push(CreateTerminalNode(currentToken.TokenType, parent));
                }
                else if (nextToken.Is(SpiceToken.DELIMITER) && nextToken.Equal("(", true))
                {
                    stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, parent, ")"));
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, parent));
                    stack.Push(CreateTerminalNode(nextToken.TokenType, parent));
                    stack.Push(CreateTerminalNode(currentToken.TokenType, parent));
                }
                else
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, parent));
                }
            }
            else
            {
                if (currentToken.Is(SpiceToken.WORD)
                    || currentToken.Is(SpiceToken.VALUE)
                    || currentToken.Is(SpiceToken.STRING)
                    || currentToken.Is(SpiceToken.REFERENCE)
                    || currentToken.Is(SpiceToken.EXPRESSION))
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, parent));
                }
                else
                {
                    throw new ParseException("Error during parsing a parameter");
                }
            }
        }

        private void ProcessParameterSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.VALUE)
                || currentToken.Is(SpiceToken.STRING)
                || currentToken.Is(SpiceToken.IDENTIFIER)
                || currentToken.Is(SpiceToken.REFERENCE)
                || currentToken.Is(SpiceToken.EXPRESSION))
            {
                stack.Push(CreateTerminalNode(currentToken.TokenType, parent));
            }
            else
            {
                throw new ParseException("Error during parsing a parameter value");
            }
        }

        private void ProcessModel(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];
            var nextNextToken = tokens[currentTokenIndex + 2];

            if (currentToken.Is(SpiceToken.DOT)
                && nextToken.Is(SpiceToken.WORD)
                && nextToken.Equal("model", true)
                && nextNextToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, parent));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, parent));
                stack.Push(CreateTerminalNode(nextNextToken.TokenType, parent));
                stack.Push(CreateTerminalNode(nextToken.TokenType, parent));
                stack.Push(CreateTerminalNode(currentToken.TokenType, parent));
            }
            else
            {
                throw new ParseException("Error during parsing a model");
            }
        }

        private void ProcessControl(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.DOT) && nextToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, parent));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, parent));
                stack.Push(CreateTerminalNode(nextToken.TokenType, parent));
                stack.Push(CreateTerminalNode(currentToken.TokenType, parent));
            }
            else
            {
                throw new ParseException("Error during parsing a control");
            }
        }

        private void ProcessComponent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode parent, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, parent));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, parent));
                stack.Push(CreateTerminalNode(currentToken.TokenType, parent));
            }
            else
            {
                throw new ParseException("Error during parsing a component");
            }
        }

        private ParseTreeNonTerminalNode CreateNonTerminalNode(string symbolName, ParseTreeNonTerminalNode parentNode)
        {
            var node = new ParseTreeNonTerminalNode(parentNode, symbolName);
            if (parentNode != null)
            {
                parentNode.Children.Insert(0, node);
            }

            return node;
        }

        private ParseTreeTerminalNode CreateTerminalNode(int tokenType, ParseTreeNonTerminalNode parentNode, string tokenValue = null)
        {
            var node = new ParseTreeTerminalNode(new Token(tokenType, tokenValue), parentNode);
            parentNode.Children.Insert(0, node);
            return node;
        }
    }
}
