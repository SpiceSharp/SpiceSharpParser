using NLex;
using SpiceNetlist;

namespace SpiceParser
{
    public class SpiceParser
    {
        public ParseTreeNode GetParseTree(Token[] tokens)
        {
            if (tokens == null)
            {
                throw new System.ArgumentNullException(nameof(tokens));
            }

            var root = CreateNonTerminal(SpiceGrammarSymbol.START, null);
            var lastIndex = ParseNetList(root, tokens, 0);

            return root;
        }

        private int ParseNetList(ParseTreeNonTerminalNode currentTreeNode, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            if (currentToken.Is(SpiceToken.TITLE))
            {
                var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, currentTreeNode);
                currentTreeNode.Children.Add(CreateTerminal(currentToken, statements));
                currentTreeNode.Children.Add(statements);

                return ParseStatements(statements, tokens, currentTokenIndex + 1);
            }
            else
            {
                throw new ParseException("Error during parsing netlist");
            }
        }

        private int ParseStatements(ParseTreeNonTerminalNode statementsNode, Token[] tokens, int currentTokenIndex)
        {
            if (tokens.Length <= currentTokenIndex)
            {
                return currentTokenIndex;
            }

            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.DOT)
                || currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.ASTERIKS))
            {
                var statement = CreateNonTerminal(SpiceGrammarSymbol.STATEMENT, statementsNode);
                statementsNode.Children.Add(statement);

                currentTokenIndex = ParseStatement(statement, tokens, currentTokenIndex);

                var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, statementsNode);
                statementsNode.Children.Add(statements);

                currentTokenIndex = ParseStatements(statements, tokens, currentTokenIndex);
            }            
            else if (currentToken.Is(SpiceToken.NEWLINE))
            {
                statementsNode.Children.Add(CreateTerminal(currentToken, statementsNode));

                var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, statementsNode);
                currentTokenIndex = ParseStatements(statements, tokens, currentTokenIndex+1);

                statementsNode.Children.Add(statements);
            }
            else if (currentToken.Is(SpiceToken.END))
            {
                statementsNode.Children.Add(CreateTerminal(currentToken, statementsNode));
            }
            else if (currentToken.Is(SpiceToken.EOF)) 
            {
                //do nothing
            }
            else if (currentToken.Is(SpiceToken.ENDS))
            {
                // do nothing 
            }
            else
            {
                throw new ParseException("Error during parsing statements");
            }
            
            return currentTokenIndex;
        }
    
        private int ParseStatement(ParseTreeNonTerminalNode statementTreeNode, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                var component = CreateNonTerminal(SpiceGrammarSymbol.COMPONENT, statementTreeNode);
                statementTreeNode.Children.Add(component);

                currentTokenIndex = ParseComponent(component, tokens, currentTokenIndex);
            }
            else if (currentToken.Is(SpiceToken.DOT))
            {
                if (nextToken.Is(SpiceToken.WORD))
                {
                    if (nextToken.Equal("subckt", true))
                    {
                        //subckt
                        var subckt = CreateNonTerminal(SpiceGrammarSymbol.SUBCKT, statementTreeNode);
                        statementTreeNode.Children.Add(subckt);
                        currentTokenIndex = ParseSubckt(subckt, tokens, currentTokenIndex);
                    }
                    else if (nextToken.Equal("model", true))
                    {
                        // model
                        var model = CreateNonTerminal(SpiceGrammarSymbol.MODEL, statementTreeNode);
                        statementTreeNode.Children.Add(model);

                        currentTokenIndex = ParseModel(model, tokens, currentTokenIndex);
                    }
                    else
                    {
                        // control
                        var control = CreateNonTerminal(SpiceGrammarSymbol.CONTROL, statementTreeNode);
                        statementTreeNode.Children.Add(control);

                        currentTokenIndex = ParseControl(control, tokens, currentTokenIndex);
                    }
                }
                else
                {
                    throw new ParseException("Error during parsing a statement");
                }
            }
            else if (currentToken.Is(SpiceToken.ASTERIKS))
            {
                var commentLine = CreateNonTerminal(SpiceGrammarSymbol.COMMENTLINE, statementTreeNode);
                statementTreeNode.Children.Add(commentLine);
                currentTokenIndex = ParseComment(commentLine, tokens, currentTokenIndex);
            }

            return currentTokenIndex;
        }

        private int ParseComment(ParseTreeNonTerminalNode commentLine, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.ASTERIKS)
                && (nextToken.Is(SpiceToken.COMMENT)
                || nextToken.Is(SpiceToken.NEWLINE)
                || nextToken.Is(SpiceToken.EOF)))

            {
                commentLine.Children.Add(CreateTerminal(currentToken, commentLine));
                commentLine.Children.Add(CreateTerminal(nextToken, commentLine));

                return currentTokenIndex + 2;
            }
            throw new ParseException("Error during parsing a comment");
        }

        private int ParseSubckt(ParseTreeNonTerminalNode subckt, Token[] tokens, int currentTokenIndex)
        {
            Token currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.DOT) && nextToken.Is(SpiceToken.WORD) && nextToken.Equal("subckt", true))
            {
                subckt.Children.Add(CreateTerminal(currentToken, subckt));
                subckt.Children.Add(CreateTerminal(nextToken, subckt));

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, subckt);
                subckt.Children.Add(parameters);

                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 2);

                var token = tokens[currentTokenIndex];

                if (token.Is(SpiceToken.NEWLINE))
                {
                    subckt.Children.Add(CreateTerminal(token, subckt));

                    var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, subckt);
                    subckt.Children.Add(statements);
                    currentTokenIndex = ParseStatements(statements, tokens, currentTokenIndex);

                    token = tokens[currentTokenIndex];
                    nextToken = tokens[currentTokenIndex + 1];
                    var nextNextToken = tokens[currentTokenIndex + 2];

                    if (token.Is(SpiceToken.ENDS) 
                        && nextToken.Is(SpiceToken.WORD)
                        && (nextNextToken.Is(SpiceToken.NEWLINE) || nextNextToken.Is(SpiceToken.EOF)))
                    {
                        subckt.Children.Add(CreateTerminal(token, subckt));
                        subckt.Children.Add(CreateTerminal(nextToken, subckt));
                        subckt.Children.Add(CreateTerminal(nextNextToken, subckt));

                        currentTokenIndex += 3;
                    }
                    else
                    {
                        throw new ParseException("Error during parsing a subckt");
                    }
                }

                return currentTokenIndex;
            }

            throw new ParseException("Error during parsing a subckt");
        }

        private int ParseParameters(ParseTreeNonTerminalNode parametersNode, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.VALUE)
                || currentToken.Is(SpiceToken.STRING)
                || currentToken.Is(SpiceToken.IDENTIFIER)
                || currentToken.Is(SpiceToken.REFERENCE)
                || currentToken.Is(SpiceToken.EXPRESSION))
            {
                var parameter = CreateNonTerminal(SpiceGrammarSymbol.PARAMETER, parametersNode);
                parametersNode.Children.Add(parameter);

                currentTokenIndex = ParseParameter(parameter, tokens, currentTokenIndex);

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, parametersNode);
                parametersNode.Children.Add(parameters);
                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex);
            }
            else if (currentToken.Is(SpiceToken.EOF))
            {
                //do nothing
            }
            else if (currentToken.Is(SpiceToken.NEWLINE)) // follow
            {
                // do nothing 
            }
            else if (currentToken.Is(SpiceToken.DELIMITER) && currentToken.Value == ")") // follow
            {
                // do nothing 
            }
            else
            {
                throw new ParseException("Error during parsing parameters");
            }

            return currentTokenIndex;
        }

        private int ParseParameter(ParseTreeNonTerminalNode parameter, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                if (nextToken.Is(SpiceToken.EQUAL))
                {
                    parameter.Children.Add(CreateTerminal(currentToken, parameter));
                    parameter.Children.Add(CreateTerminal(nextToken, parameter));

                    var nextNextToken = tokens[currentTokenIndex + 2];

                    if (nextNextToken.Is(SpiceToken.VALUE))
                    {
                        parameter.Children.Add(CreateTerminal(nextNextToken, parameter));

                        return currentTokenIndex + 3;
                    }
                }
                else if (nextToken.Is(SpiceToken.DELIMITER) && nextToken.Equal("(", true))
                {
                    parameter.Children.Add(CreateTerminal(currentToken, parameter));
                    parameter.Children.Add(CreateTerminal(nextToken, parameter));

                    var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, parameter);
                    parameter.Children.Add(parameters);
                    currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 2);
                    var nextNextToken = tokens[currentTokenIndex];
                    parameter.Children.Add(CreateTerminal(nextNextToken, parameter));
                    return currentTokenIndex + 1;
                }
                else
                {
                    var parameterSingleNode = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERSINGLE, parameter);
                    parameter.Children.Add(parameterSingleNode);
                    currentTokenIndex = ParseParameterSingle(parameterSingleNode, tokens, currentTokenIndex);
                    return currentTokenIndex;
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
                    var parameterSingleNode = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERSINGLE, parameter);
                    parameter.Children.Add(parameterSingleNode);
                    currentTokenIndex = ParseParameterSingle(parameterSingleNode, tokens, currentTokenIndex);
                    return currentTokenIndex;
                }
            }

            throw new ParseException("Error during parsing a parameter");
        }

        private int ParseParameterSingle(ParseTreeNonTerminalNode parameterSingle, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.Is(SpiceToken.WORD)
                || currentToken.Is(SpiceToken.VALUE)
                || currentToken.Is(SpiceToken.STRING)
                || currentToken.Is(SpiceToken.IDENTIFIER)
                || currentToken.Is(SpiceToken.REFERENCE)
                || currentToken.Is(SpiceToken.EXPRESSION))
            {
                parameterSingle.Children.Add(CreateTerminal(currentToken, parameterSingle));

                return currentTokenIndex + 1;
            }
            throw new ParseException("Error during parsing a parameter value");
        }

        private int ParseModel(ParseTreeNonTerminalNode model, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];
            var nextNextToken = tokens[currentTokenIndex + 2];

            if (currentToken.Is(SpiceToken.DOT) && nextToken.Is(SpiceToken.WORD)
                && nextToken.Equal("model", true) && nextNextToken.Is(SpiceToken.WORD))
            {
                model.Children.Add(CreateTerminal(currentToken, model));
                model.Children.Add(CreateTerminal(nextToken, model));
                model.Children.Add(CreateTerminal(nextNextToken, model));

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, model);
                model.Children.Add(parameters);

                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 3);
                var token = tokens[currentTokenIndex];

                if (token.Is(SpiceToken.NEWLINE) || token.Is(SpiceToken.EOF))
                {
                    model.Children.Add(CreateTerminal(token, model));
                    currentTokenIndex += 1;

                    return currentTokenIndex;
                }
            }
            throw new ParseException("Error during parsing a model");
        }

        private int ParseControl(ParseTreeNonTerminalNode control, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.DOT) && nextToken.Is(SpiceToken.WORD))
            {
                control.Children.Add(CreateTerminal(currentToken, control));
                control.Children.Add(CreateTerminal(nextToken, control));

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, control);
                control.Children.Add(parameters);
                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 2);

                var token = tokens[currentTokenIndex];

                if (token.Is(SpiceToken.NEWLINE) || token.Is(SpiceToken.EOF))
                {
                    control.Children.Add(CreateTerminal(token, control));

                    currentTokenIndex += 1;

                    return currentTokenIndex;
                }
            }
            throw new ParseException("Error during parsing a control");
        }

        private int ParseComponent(ParseTreeNonTerminalNode component, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.Is(SpiceToken.WORD))
            {
                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, component);

                component.Children.Add(CreateTerminal(currentToken, component));
                component.Children.Add(parameters);

                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 1);

                var token = tokens[currentTokenIndex];

                if (token.Is(SpiceToken.NEWLINE) || token.Is(SpiceToken.EOF))
                {
                    component.Children.Add(CreateTerminal(token, component));
                    currentTokenIndex += 1;

                    return currentTokenIndex;
                }
            }
            throw new ParseException("Error during parsing a component");
        }

        private ParseTreeNonTerminalNode CreateNonTerminal(string symbolName, ParseTreeNode parent)
        {
            return new ParseTreeNonTerminalNode(parent, symbolName);
        }

        private ParseTreeTerminalNode CreateTerminal(Token token, ParseTreeNonTerminalNode parent)
        {
            return new ParseTreeTerminalNode(token, parent);
        }
    }
}
