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
            if (currentToken.TokenType == (int)SpiceTokenType.TITLE)
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
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.TokenType == (int)SpiceTokenType.DOT
                || currentToken.TokenType == (int)SpiceTokenType.WORD
                || currentToken.TokenType == (int)SpiceTokenType.ASTERIKS)
            {
                var statement = CreateNonTerminal(SpiceGrammarSymbol.STATEMENT, statementsNode);
                statementsNode.Children.Add(statement);

                currentTokenIndex = ParseStatement(statement, tokens, currentTokenIndex);

                var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, statementsNode);
                statementsNode.Children.Add(statements);

                currentTokenIndex = ParseStatements(statements, tokens, currentTokenIndex);
            }            
            else if (currentToken.TokenType == (int)SpiceTokenType.NEWLINE)
            {
                statementsNode.Children.Add(CreateTerminal(currentToken, statementsNode));

                var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, statementsNode);
                currentTokenIndex = ParseStatements(statements, tokens, currentTokenIndex+1);

                statementsNode.Children.Add(statements);
            }
            else if (currentToken.TokenType == -1) // follow
            {
                //do nothing
            }
            else if (currentToken.TokenType == (int)SpiceTokenType.ENDS) // follow
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

            if (currentToken.TokenType == (int)SpiceTokenType.WORD)
            {
                var component = CreateNonTerminal(SpiceGrammarSymbol.COMPONENT, statementTreeNode);
                statementTreeNode.Children.Add(component);

                currentTokenIndex = ParseComponent(component, tokens, currentTokenIndex);
            }
            else if (currentToken.TokenType == (int)SpiceTokenType.DOT)
            {
                if (nextToken.TokenType == (int)SpiceTokenType.WORD)
                {
                    if (nextToken.Value == "subckt")
                    {
                        //subckt
                        var subckt = CreateNonTerminal(SpiceGrammarSymbol.SUBCKT, statementTreeNode);
                        statementTreeNode.Children.Add(subckt);
                        currentTokenIndex = ParseSubckt(subckt, tokens, currentTokenIndex);
                    }
                    else if (nextToken.Value == "model")
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
            else if (currentToken.TokenType == (int)SpiceTokenType.ASTERIKS)
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

            if (currentToken.TokenType == (int)SpiceTokenType.ASTERIKS
                && (nextToken.TokenType == (int)SpiceTokenType.COMMENT
                || nextToken.TokenType == (int)SpiceTokenType.NEWLINE
                || nextToken.TokenType == -1))

            {
                commentLine.Children.Add(CreateTerminal(currentToken, commentLine));
                commentLine.Children.Add(CreateTerminal(nextToken, commentLine));

                return currentTokenIndex + 2;
            }
            throw new ParseException("Error during parsing a comment");
        }

        private int ParseSubckt(ParseTreeNonTerminalNode subckt, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];
            var nextToken = tokens[currentTokenIndex + 1];

            if (currentToken.TokenType == (int)SpiceTokenType.DOT
                && nextToken.TokenType == (int)SpiceTokenType.WORD
                && nextToken.Value == "subckt")
            {
                subckt.Children.Add(CreateTerminal(currentToken, subckt));
                subckt.Children.Add(CreateTerminal(nextToken, subckt));

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, subckt);
                subckt.Children.Add(parameters);

                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 2);

                var token = tokens[currentTokenIndex];

                if (token.TokenType == (int)SpiceTokenType.NEWLINE)
                {
                    subckt.Children.Add(CreateTerminal(token, subckt));

                    var statements = CreateNonTerminal(SpiceGrammarSymbol.STATEMENTS, subckt);
                    subckt.Children.Add(statements);
                    currentTokenIndex = ParseStatements(statements, tokens, currentTokenIndex);

                    token = tokens[currentTokenIndex];
                    nextToken = tokens[currentTokenIndex + 1];
                    var nextNextToken = tokens[currentTokenIndex + 2];

                    if (token.TokenType == (int)SpiceTokenType.ENDS 
                        && nextToken.TokenType == (int)SpiceTokenType.WORD
                        && (nextNextToken.TokenType == (int)SpiceTokenType.NEWLINE || nextNextToken.TokenType == -1))
                    {
                        subckt.Children.Add(CreateTerminal(currentToken, subckt));
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

            if (currentToken.TokenType == (int)SpiceTokenType.WORD
                || currentToken.TokenType == (int)SpiceTokenType.VALUE
                || currentToken.TokenType == (int)SpiceTokenType.STRING
                || currentToken.TokenType == (int)SpiceTokenType.IDENTIFIER
                || currentToken.TokenType == (int)SpiceTokenType.REFERENCE
                || currentToken.TokenType == (int)SpiceTokenType.EXPRESSION)
            {
                var parameter = CreateNonTerminal(SpiceGrammarSymbol.PARAMETER, parametersNode);
                parametersNode.Children.Add(parameter);

                currentTokenIndex = ParseParameter(parameter, tokens, currentTokenIndex);

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, parametersNode);
                parametersNode.Children.Add(parameters);
                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex);
            }
            else if (currentToken.TokenType == -1) // follow
            {
                //do nothing
            }
            else if (currentToken.TokenType == (int)SpiceTokenType.NEWLINE) // follow
            {
                // do nothing 
            }
            else if (currentToken.TokenType == (int)SpiceTokenType.DELIMITER && currentToken.Value == ")") // follow
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
            var nextToken = tokens[currentTokenIndex+1];

            if (currentToken.TokenType == (int)SpiceTokenType.WORD)
            {
                if (nextToken.TokenType == (int)SpiceTokenType.EQUAL)
                {
                    parameter.Children.Add(CreateTerminal(currentToken, parameter));
                    parameter.Children.Add(CreateTerminal(nextToken, parameter));

                    var nextNextToken = tokens[currentTokenIndex + 2];

                    if (nextNextToken.TokenType == (int)SpiceTokenType.VALUE)
                    {
                        parameter.Children.Add(CreateTerminal(nextNextToken, parameter));

                        return currentTokenIndex + 3;
                    }
                }
                else if (nextToken.TokenType == (int)SpiceTokenType.DELIMITER && nextToken.Value == "(")
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
                    var pS = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERSINGLE, parameter);
                    parameter.Children.Add(pS);
                    currentTokenIndex = ParseParameterSingle(pS, tokens, currentTokenIndex);
                    return currentTokenIndex;
                }
            }
            else if (currentToken.TokenType == (int)SpiceTokenType.WORD
                || currentToken.TokenType == (int)SpiceTokenType.VALUE
                || currentToken.TokenType == (int)SpiceTokenType.STRING
                || currentToken.TokenType == (int)SpiceTokenType.REFERENCE
                || currentToken.TokenType == (int)SpiceTokenType.EXPRESSION)
            {
                var pS = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERSINGLE, parameter);
                parameter.Children.Add(pS);
                currentTokenIndex = ParseParameterSingle(pS, tokens, currentTokenIndex);
                return currentTokenIndex;
            }

            throw new ParseException("Error during parsing a parameter");
        }

        private int ParseParameterSingle(ParseTreeNonTerminalNode parameterSingle, Token[] tokens, int currentTokenIndex)
        {
            var currentToken = tokens[currentTokenIndex];

            if (currentToken.TokenType == (int)SpiceTokenType.WORD
                || currentToken.TokenType == (int)SpiceTokenType.VALUE
                || currentToken.TokenType == (int)SpiceTokenType.STRING
                || currentToken.TokenType == (int)SpiceTokenType.IDENTIFIER
                || currentToken.TokenType == (int)SpiceTokenType.REFERENCE
                || currentToken.TokenType == (int)SpiceTokenType.EXPRESSION)
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

            if (currentToken.TokenType == (int)SpiceTokenType.DOT
                && nextToken.TokenType == (int)SpiceTokenType.WORD
                && nextToken.Value == "model"
                && nextNextToken.TokenType == (int)SpiceTokenType.WORD)
            {
                model.Children.Add(CreateTerminal(currentToken, model));
                model.Children.Add(CreateTerminal(nextToken, model));
                model.Children.Add(CreateTerminal(nextNextToken, model));

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, model);
                model.Children.Add(parameters);

                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 3);
                var token = tokens[currentTokenIndex];

                if (token.TokenType == (int)SpiceTokenType.NEWLINE || token.TokenType == -1)
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

            if (currentToken.TokenType == (int)SpiceTokenType.DOT
                && nextToken.TokenType == (int)SpiceTokenType.WORD)
            {
                control.Children.Add(CreateTerminal(currentToken, control));
                control.Children.Add(CreateTerminal(nextToken, control));

                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, control);
                control.Children.Add(parameters);
                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 2);

                var token = tokens[currentTokenIndex];

                if (token.TokenType == (int)SpiceTokenType.NEWLINE || token.TokenType == -1)
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

            if (currentToken.TokenType == (int)SpiceTokenType.WORD)
            {
                var parameters = CreateNonTerminal(SpiceGrammarSymbol.PARAMETERS, component);

                component.Children.Add(CreateTerminal(currentToken, component));
                component.Children.Add(parameters);

                currentTokenIndex = ParseParameters(parameters, tokens, currentTokenIndex + 2);

                var token = tokens[currentTokenIndex];

                if (token.TokenType == (int)SpiceTokenType.NEWLINE || token.TokenType == -1)
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
            return new ParseTreeNonTerminalNode() { Parent = parent, Name = symbolName };
        }

        private ParseTreeTerminalNode CreateTerminal(Token token, ParseTreeNode parent)
        {
            return new ParseTreeTerminalNode() { Parent = parent, Token = token };
        }
    }
}
