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
        /// <param name="rootSymbol">A root symbol of parse tree</param>
        /// <returns>
        /// A parse tree
        /// </returns>
        public ParseTreeNonTerminalNode GetParseTree(Token[] tokens, string rootSymbol = SpiceGrammarSymbol.START)
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
                        case SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE:
                            ProcessParameterSingleSequence(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE_CONTINUE:
                            ProcessParameterSingleSequenceContinue(stack, ntn, tokens, currentTokenIndex);
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
                        case SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE:
                            ProcessParameterEqualSequence(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE_CONTINUE:
                            ProcessParameterEqualSequenceContinue(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.VECTOR:
                            ProcessVector(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.VECTOR_CONTINUE:
                            ProcessVectorContinue(stack, ntn, tokens, currentTokenIndex);
                            break;
                        case SpiceGrammarSymbol.NEW_LINE_OR_EOF:
                            ProcessNewLineOrEOF(stack, ntn, tokens, currentTokenIndex);
                            break;
                    }
                }

                if (currentNode is ParseTreeTerminalNode tn)
                {
                    if (tn.Token.TokenType == tokens[currentTokenIndex].TokenType
                        && (tn.Token.Lexem == null || tn.Token.Lexem == tokens[currentTokenIndex].Lexem))
                    {
                        tn.Token.UpdateLexem(tokens[currentTokenIndex].Lexem);
                        currentTokenIndex++;
                    }
                    else
                    {
                        throw new ParseException("Unexpected token: " + tokens[currentTokenIndex].Lexem + ", expected token type: " + tn.Token.TokenType);
                    }
                }
            }

            return root;
        }

        private void ProcessSubcktEnding(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.ENDS))
            {
                if (nextToken.Is(SpiceToken.WORD))
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, current));
                    stack.Push(CreateTerminalNode((int)SpiceToken.WORD, current));
                    stack.Push(CreateTerminalNode((int)SpiceToken.ENDS, current));
                }
                else
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, current));
                    stack.Push(CreateTerminalNode((int)SpiceToken.ENDS, current));
                }
            }
            else
            {
                throw new ParseException("Wrong ending for .subckt");
            }
        }

        private void ProcessNewLineOrEOF(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentToken.Is(SpiceToken.EOF))
            {
                stack.Push(CreateTerminalNode((int)SpiceToken.EOF, currentNode));
            }

            if (currentToken.Is(SpiceToken.NEWLINE))
            {
                stack.Push(CreateTerminalNode((int)SpiceToken.NEWLINE, currentNode));
            }
        }

        private void ProcessStartNode(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode currentNode, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, currentNode));
            stack.Push(CreateTerminalNode((int)SpiceToken.TITLE, currentNode));
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
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceToken.ENDS))
            {
                // follow - do nothing
            }
            else
            {
                throw new ParseException("Error during parsing statements");
            }
        }

        private void ProcessStatement(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.COMPONENT, current));
            }
            else if (currentToken.Is(SpiceToken.DOT))
            {
                if (nextToken.Is(SpiceToken.WORD))
                {
                    if (nextToken.Equal("subckt", true))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.SUBCKT, current));
                    }
                    else if (nextToken.Equal("model", true))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.MODEL, current));
                    }
                    else
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.CONTROL, current));
                    }
                }
                else
                {
                    throw new ParseException("Error during parsing a statement");
                }
            }
            else if (currentToken.Is(SpiceToken.ASTERIKS))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.COMMENT_LINE, current));
            }
        }

        private void ProcessVector(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR_CONTINUE, current));
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
            stack.Push(CreateTerminalNode((int)SpiceToken.COMMA, current, ","));
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
        }

        private void ProcessVectorContinue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Lexem == ")")
            {
                // follow
            }
            else
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR_CONTINUE, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                stack.Push(CreateTerminalNode((int)SpiceToken.COMMA, current, ","));
            }
        }

        private void ProcessCommentLine(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.ASTERIKS)
                && (nextToken.Is(SpiceToken.COMMENT)
                || nextToken.Is(SpiceToken.NEWLINE)
                || nextToken.Is(SpiceToken.EOF)))
            {
                stack.Push(CreateTerminalNode(nextToken.TokenType, current, nextToken.Lexem));
                stack.Push(CreateTerminalNode(currentToken.TokenType, current, currentToken.Lexem));
            }
            else
            {
                throw new ParseException("Error during parsing a comment");
            }
        }

        private void ProcessSubckt(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            Token currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.DOT)
                && nextToken.Is(SpiceToken.WORD)
                && nextToken.Equal("subckt", true))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.SUBCKT_ENDING, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.STATEMENTS, current));
                stack.Push(CreateTerminalNode((int)SpiceToken.NEWLINE, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current));
                stack.Push(CreateTerminalNode((int)SpiceToken.WORD, current));
                stack.Push(CreateTerminalNode(nextToken.TokenType, current, nextToken.Lexem));
                stack.Push(CreateTerminalNode(currentToken.TokenType, current, currentToken.Lexem));
            }
            else
            {
                throw new ParseException("Error during parsing a subckt");
            }
        }

        private void ProcessParameters(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.VALUE)
                || currentToken.Is(SpiceToken.STRING)
                || currentToken.Is(SpiceToken.IDENTIFIER)
                || currentToken.Is(SpiceToken.REFERENCE)
                || currentToken.Is(SpiceToken.EXPRESSION))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER, current));
            }
            else if (currentToken.Is(SpiceToken.EOF))
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceToken.NEWLINE))
            {
                // follow - do nothing
            }
            else if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Lexem == ")")
            {
                // follow - do nothing
            }
            else
            {
                throw new ParseException("Error during parsing parameters");
            }
        }

        private void ProcessParameterEqual(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                if (nextToken.Is(SpiceToken.EQUAL))
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, current));
                }
                else if (nextToken.Is(SpiceToken.DELIMITER) && nextToken.Equal("(", true))
                {
                    if ((tokens.Length > currentTokenIndex + 4) && tokens[currentTokenIndex + 3].Lexem == ")" && tokens[currentTokenIndex + 4].Lexem == "=")
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.EQUAL, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, current, ")"));
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, current, "("));
                        stack.Push(CreateTerminalNode((int)SpiceToken.WORD, current));
                    }

                    if ((tokens.Length > currentTokenIndex + 6) && tokens[currentTokenIndex + 5].Lexem == ")" && tokens[currentTokenIndex + 6].Lexem == "=")
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.EQUAL, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, current, ")"));
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.COMMA, current, ","));
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                        stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, current, "("));
                        stack.Push(CreateTerminalNode((int)SpiceToken.WORD, current));
                    }
                }
            }
        }

        private void ProcessParameterEqualSequence(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE_CONTINUE, current));
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, current));
        }

        private void ProcessParameterEqualSequenceContinue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Lexem == ")")
            {
                // follow
            }
            else
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE_CONTINUE, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, current));
            }
        }


        private void ProcessParameterSingleSequence(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE_CONTINUE, current));
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
        }

        private void ProcessParameterSingleSequenceContinue(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Lexem == ")")
            {
                // follow
            }
            else
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE_CONTINUE, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
            }
        }


        private void ProcessParameterEqualSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
            stack.Push(CreateTerminalNode((int)SpiceToken.EQUAL, current));
            stack.Push(CreateTerminalNode((int)SpiceToken.WORD, current));
        }

        private void ProcessParameterBracketContent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (nextToken.Is(SpiceToken.COMMA))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR, current));
            }
            else
            {
                if (nextToken.Is(SpiceToken.EQUAL))
                {
                    stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE, current));
                }
                else
                {
                    if (currentToken.Is(SpiceToken.VALUE)
                        || currentToken.Is(SpiceToken.WORD)
                        || currentToken.Is(SpiceToken.STRING)
                        || currentToken.Is(SpiceToken.IDENTIFIER)
                        || currentToken.Is(SpiceToken.REFERENCE)
                        || currentToken.Is(SpiceToken.EXPRESSION))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE, current));
                    }
                    else
                    {
                        if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Lexem == ")")
                        {
                            // follow
                        }
                        else
                        {
                            throw new ParseException("Error during parsing a parameter");
                        }
                    }
                }
            }
        }

        private void ProcessParameter(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (nextToken.Is(SpiceToken.COMMA))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.VECTOR, current));
            }
            else
            {
                if (currentToken.Is(SpiceToken.WORD))
                {
                    if (nextToken.Is(SpiceToken.EQUAL))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL, current));
                    }
                    else if (nextToken.Is(SpiceToken.DELIMITER) && nextToken.Equal("(", true))
                    {
                        if (((tokens.Length > currentTokenIndex + 4) && tokens[currentTokenIndex + 3].Lexem == ")" && tokens[currentTokenIndex + 4].Lexem == "=")
                            || ((tokens.Length > currentTokenIndex + 6) && tokens[currentTokenIndex + 5].Lexem == ")" && tokens[currentTokenIndex + 6].Lexem == "="))
                        {
                            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_EQUAL, current));
                        }
                        else
                        {
                            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_BRACKET, current));
                        }
                    }
                    else
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                    }
                }
                else
                {
                    if (currentToken.Is(SpiceToken.VALUE)
                        || currentToken.Is(SpiceToken.STRING)
                        || currentToken.Is(SpiceToken.IDENTIFIER)
                        || currentToken.Is(SpiceToken.REFERENCE)
                        || currentToken.Is(SpiceToken.EXPRESSION))
                    {
                        stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_SINGLE, current));
                    }
                    else
                    {
                        throw new ParseException("Error during parsing a parameter");
                    }
                }
            }
        }

        private void ProcessParameterBracket(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, current, ")"));
            stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT, current));
            stack.Push(CreateTerminalNode((int)SpiceToken.DELIMITER, current, "("));
            stack.Push(CreateTerminalNode((int)SpiceToken.WORD, current));
        }

        private void ProcessParameterSingle(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.VALUE)
                || currentToken.Is(SpiceToken.STRING)
                || currentToken.Is(SpiceToken.IDENTIFIER)
                || currentToken.Is(SpiceToken.REFERENCE)
                || currentToken.Is(SpiceToken.EXPRESSION))
            {
                stack.Push(CreateTerminalNode(currentToken.TokenType, current));
            }
            else
            {
                throw new ParseException("Error during parsing a parameter value");
            }
        }

        private void ProcessModel(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];
            var nextNextToken = tokens[currentTokenIndex + 2];

            if (currentToken.Is(SpiceToken.DOT)
                && nextToken.Is(SpiceToken.WORD)
                && nextToken.Equal("model", true)
                && (nextNextToken.Is(SpiceToken.WORD) || nextNextToken.Is(SpiceToken.IDENTIFIER)))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current));
                stack.Push(CreateTerminalNode(nextNextToken.TokenType, current));
                stack.Push(CreateTerminalNode(nextToken.TokenType, current));
                stack.Push(CreateTerminalNode(currentToken.TokenType, current));
            }
            else
            {
                throw new ParseException("Error during parsing a model");
            }
        }

        private void ProcessControl(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.DOT) && nextToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current));
                stack.Push(CreateTerminalNode(nextToken.TokenType, current));
                stack.Push(CreateTerminalNode(currentToken.TokenType, current));
            }
            else
            {
                throw new ParseException("Error during parsing a control");
            }
        }

        private void ProcessComponent(Stack<ParseTreeNode> stack, ParseTreeNonTerminalNode current, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD))
            {
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.NEW_LINE_OR_EOF, current));
                stack.Push(CreateNonTerminalNode(SpiceGrammarSymbol.PARAMETERS, current));
                stack.Push(CreateTerminalNode(currentToken.TokenType, current));
            }
            else
            {
                throw new ParseException("Error during parsing a component");
            }
        }

        private ParseTreeNonTerminalNode CreateNonTerminalNode(string symbolName, ParseTreeNonTerminalNode currentNode)
        {
            var node = new ParseTreeNonTerminalNode(currentNode, symbolName);
            if (currentNode != null)
            {
                currentNode.Children.Insert(0, node);
            }

            return node;
        }

        private ParseTreeTerminalNode CreateTerminalNode(int tokenType, ParseTreeNonTerminalNode currentNode, string tokenValue = null)
        {
            var node = new ParseTreeTerminalNode(new Token(tokenType, tokenValue), currentNode);
            currentNode.Children.Insert(0, node);
            return node;
        }
    }
}
