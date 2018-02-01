using System;
using System.Collections.Generic;
using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;

namespace SpiceParser
{
    public class ParseTreeTranslator : ParseTreeVisitor
    {
        Dictionary<string, Func<List<ParseTreeTranslatorItem>, SpiceObject>> Translators = new Dictionary<string, Func<List<ParseTreeTranslatorItem>, SpiceObject>>();
        Dictionary<ParseTreeNode, ParseTreeTranslatorItem> TranslatorItems = new Dictionary<ParseTreeNode, ParseTreeTranslatorItem>();

        public ParseTreeTranslator()
        {
            Translators.Add(SpiceGrammarSymbol.START, (List<ParseTreeTranslatorItem> nt) => CreateNetList(nt));
            Translators.Add(SpiceGrammarSymbol.STATEMENTS, (List<ParseTreeTranslatorItem> nt) => CreateStatements(nt));
            Translators.Add(SpiceGrammarSymbol.STATEMENT, (List<ParseTreeTranslatorItem> nt) => CreateStatement(nt));
            Translators.Add(SpiceGrammarSymbol.MODEL, (List<ParseTreeTranslatorItem> nt) => CreateModel(nt));
            Translators.Add(SpiceGrammarSymbol.CONTROL, (List<ParseTreeTranslatorItem> nt) => CreateControl(nt));
            Translators.Add(SpiceGrammarSymbol.COMPONENT, (List<ParseTreeTranslatorItem> nt) => CreateComponent(nt));
            Translators.Add(SpiceGrammarSymbol.PARAMETERS, (List<ParseTreeTranslatorItem> nt) => CreateParameters(nt));
            Translators.Add(SpiceGrammarSymbol.PARAMETER, (List<ParseTreeTranslatorItem> nt) => CreateParameter(nt));
            Translators.Add(SpiceGrammarSymbol.PARAMETERSINGLE, (List<ParseTreeTranslatorItem> nt) => CreateParameterSingle(nt));
            Translators.Add(SpiceGrammarSymbol.SUBCKT, (List<ParseTreeTranslatorItem> nt) => CreateSubCircuit(nt));
            Translators.Add(SpiceGrammarSymbol.COMMENTLINE, (List<ParseTreeTranslatorItem> nt) => CreateComment(nt));
        }

        public SpiceObject GetSpiceObject(ParseTreeNode parseTreeNode)
        {
            return TranslatorItems[parseTreeNode].SpiceObject;
        }

        public override void VisitParseTreeTerminal(ParseTreeTerminalNode node)
        {
            TranslatorItems[node] = new ParseTreeTranslatorItem() { Node = node, Token = node.Token };
        }

        public override void VisitParseTreeNonTerminal(ParseTreeNonTerminalNode node)
        {
            List<ParseTreeTranslatorItem> items = new List<ParseTreeTranslatorItem>();
            foreach (var childNode in node.Children)
            {
                items.Add(TranslatorItems[childNode]);
            }

            var item = new ParseTreeTranslatorItem() { Node = node, SpiceObject = Translators[node.Name](items) };

            TranslatorItems[node] = item;
        }

        private SpiceObject CreateParameter(List<ParseTreeTranslatorItem> CurrentItems)
        {
            Parameter parameter = null;

            if (CurrentItems.Count > 0)
            {
                if (CurrentItems[0].SpiceObject is SingleParameter sp)
                {
                    parameter = sp;
                }
                else
                {
                    if (CurrentItems[0].IsToken
                        && CurrentItems[0].Token.TokenType == (int)SpiceTokenType.WORD
                        && CurrentItems[1].IsToken
                        && CurrentItems[1].Token.TokenType == (int)SpiceTokenType.DELIMITER
                        && CurrentItems[1].Token.Value == "("
                        && CurrentItems[2].IsSpiceObject
                        && CurrentItems[2].SpiceObject is ParameterCollection
                        && CurrentItems[3].Token.TokenType == (int)SpiceTokenType.DELIMITER
                        && CurrentItems[3].Token.Value == ")")
                    {
                        parameter = new ComplexParameter()
                        {
                            Name = CurrentItems[0].Token.Value,
                            Parameters = CurrentItems[2].SpiceObject as ParameterCollection
                        };
                    }

                    if (CurrentItems[0].IsToken
                        && CurrentItems[0].Token.TokenType == (int)SpiceTokenType.WORD
                        && CurrentItems[1].Token.Value == "="
                        && CurrentItems[2].Token.TokenType == (int)SpiceTokenType.VALUE)
                    {
                        parameter = new AssignmentParameter()
                        {
                            Name = CurrentItems[0].Token.Value,
                            Value = CurrentItems[2].Token.Value,
                        };
                    }
                }
            }

            return parameter;
        }

        private SpiceObject CreateParameterSingle(List<ParseTreeTranslatorItem> CurrentItems)
        {
            if (!CurrentItems[0].IsToken)
            {
                throw new ParseException();
            }

            switch(CurrentItems[0].Token.TokenType)
            {
                case (int)SpiceTokenType.REFERENCE:
                    return new ReferenceParameter() { RawValue = CurrentItems[0].Token.Value };
                case (int)SpiceTokenType.VALUE:
                    return new ValueParameter() { RawValue = CurrentItems[0].Token.Value };
                case (int)SpiceTokenType.WORD:
                    return new WordParameter() { RawValue = CurrentItems[0].Token.Value };
                case (int)SpiceTokenType.IDENTIFIER:
                    return new IdentifierParameter() { RawValue = CurrentItems[0].Token.Value };
            }
            throw new ParseException();
        }

        private SpiceObject CreateParameters(List<ParseTreeTranslatorItem> CurrentItems)
        {
            var parameters = new ParameterCollection();

            if (CurrentItems.Count == 2)
            {
                if (CurrentItems[0].SpiceObject is Parameter p)
                {
                    parameters.Values.Add(p);
                }

                if (CurrentItems[1].SpiceObject is ParameterCollection ps2)
                {
                    parameters.Values.AddRange(ps2.Values);
                }
            }

            return parameters;
        }

        private SpiceObject CreateComponent(List<ParseTreeTranslatorItem> CurrentItems)
        {
            if (CurrentItems.Count != 2 && CurrentItems.Count != 3)
            {
                throw new ParseException();
            }
            var component = new Component();
            component.Name = CurrentItems[0].Token.Value;
            component.Parameters = CurrentItems[1].SpiceObject as ParameterCollection;
            return component;
        }

        private SpiceObject CreateControl(List<ParseTreeTranslatorItem> CurrentItems)
        {
            var control = new Control();
            control.Name = CurrentItems[0].Token.Value;
            control.Parameters = CurrentItems[1].SpiceObject as ParameterCollection;
            return control;
        }

        private SpiceObject CreateSubCircuit(List<ParseTreeTranslatorItem> CurrentItems)
        {
            var control = new SubCircuit();
            control.Name = CurrentItems[0].Token.Value;
            control.Parameters = CurrentItems[1].SpiceObject as ParameterCollection;
            return control;
        }

        private SpiceObject CreateComment(List<ParseTreeTranslatorItem> CurrentItems)
        {
            var comment = new CommentLine();
            comment.Text = CurrentItems[1].Token.Value;
            return comment;
        }

        private SpiceObject CreateStatement(List<ParseTreeTranslatorItem> CurrentItems)
        {
            if (CurrentItems.Count == 1 && CurrentItems[0].IsSpiceObject)
            {
                return CurrentItems[0].SpiceObject as Statement;
            }
            throw new ParseException();
        }

        private SpiceObject CreateModel(List<ParseTreeTranslatorItem> CurrentItems)
        {
            var model = new Model();
            model.Name = CurrentItems[2].Token.Value;
            model.Parameters = CurrentItems[3].SpiceObject as ParameterCollection;
            return model;
        }

        private SpiceObject CreateStatements(List<ParseTreeTranslatorItem> CurrentItems)
        {
            var statements = new Statements();

            if (CurrentItems.Count == 2)
            {
                if (CurrentItems[0].SpiceObject is Statement st)
                {
                    statements.Add(st);
                }
                if (CurrentItems[1].SpiceObject is Statements sts)
                {
                    statements.Merge(sts);
                }
            }
            else
            {
                if (CurrentItems.Count == 1)
                {
                    if (CurrentItems[0].SpiceObject is Statements sts)
                    {
                        statements.Merge(sts);
                    }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }
            return statements;
        }

        private SpiceObject CreateNetList(List<ParseTreeTranslatorItem> CurrentItems)
        {
            return new NetList() {
                Title = CurrentItems[0].Token.Value,
                Statements = CurrentItems[1].SpiceObject as Statements
            };
        }
    }
}

