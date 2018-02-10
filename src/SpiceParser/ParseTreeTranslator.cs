using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using System;
using System.Collections.Generic;

namespace SpiceParser
{
    public class ParseTreeTranslator
    {
        Dictionary<ParseTreeNode, ParseTreeTranslatorItem> TranslatorItems = new Dictionary<ParseTreeNode, ParseTreeTranslatorItem>();
        Dictionary<string, Func<List<ParseTreeTranslatorItem>, SpiceObject>> Translators = new Dictionary<string, Func<List<ParseTreeTranslatorItem>, SpiceObject>>();

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
            Translators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE, (List<ParseTreeTranslatorItem> nt) => CreateParameterSingle(nt));
            Translators.Add(SpiceGrammarSymbol.SUBCKT, (List<ParseTreeTranslatorItem> nt) => CreateSubCircuit(nt));
            Translators.Add(SpiceGrammarSymbol.COMMENT_LINE, (List<ParseTreeTranslatorItem> nt) => CreateComment(nt));
            Translators.Add(SpiceGrammarSymbol.NEW_LINE_OR_EOF, (List<ParseTreeTranslatorItem> nt) => null);
            Translators.Add(SpiceGrammarSymbol.SUBCKT_ENDING, (List<ParseTreeTranslatorItem> nt) => null);
        }

        /// <summary>
        /// Translates a spice parse tree to a netlist (SpiceNetList library)
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public NetList GetNetList(ParseTreeNode root)
        {
            var travelsal = new ParseTreeTravelsal();
            var treeNodes = travelsal.GetIterativePostOrder(root);

            foreach (var treeNode in treeNodes)
            {
                if (treeNode is ParseTreeNonTerminalNode nt)
                {
                    var items = new List<ParseTreeTranslatorItem>();

                    foreach (var child in nt.Children)
                    {
                        items.Add(TranslatorItems[child]);
                    }

                    var treeNodeResult = Translators[nt.Name](items);
                    TranslatorItems[treeNode] = new ParseTreeTranslatorItem()
                    {
                        SpiceObject = treeNodeResult,
                        Node = treeNode
                    };
                }
                else
                {
                    TranslatorItems[treeNode] = new ParseTreeTranslatorItem()
                    {
                        Node = treeNode,
                        Token = ((ParseTreeTerminalNode)treeNode).Token
                    };
                }
            }

            return TranslatorItems[root].SpiceObject as NetList;
        }

        SpiceObject CreateParameter(List<ParseTreeTranslatorItem> childrenItems)
        {
            Parameter parameter = null;

            if (childrenItems.Count > 0)
            {
                if (childrenItems[0].SpiceObject is SingleParameter sp)
                {
                    parameter = sp;
                }
                else
                {
                    if (childrenItems[0].IsToken
                        && childrenItems[0].Token.TokenType == (int)SpiceToken.WORD
                        && childrenItems[1].IsToken
                        && childrenItems[1].Token.TokenType == (int)SpiceToken.DELIMITER
                        && childrenItems[1].Token.Value == "("
                        && childrenItems[2].IsSpiceObject
                        && childrenItems[2].SpiceObject is ParameterCollection
                        && childrenItems[3].Token.TokenType == (int)SpiceToken.DELIMITER
                        && childrenItems[3].Token.Value == ")")
                    {
                        parameter = new ComplexParameter()
                        {
                            Name = childrenItems[0].Token.Value,
                            Parameters = childrenItems[2].SpiceObject as ParameterCollection
                        };
                    }

                    if (childrenItems[0].IsToken
                        && childrenItems[0].Token.TokenType == (int)SpiceToken.WORD
                        && childrenItems[1].Token.Value == "="
                        && childrenItems[2].Token.TokenType == (int)SpiceToken.VALUE)
                    {
                        parameter = new AssignmentParameter()
                        {
                            Name = childrenItems[0].Token.Value,
                            Value = childrenItems[2].Token.Value,
                        };
                    }
                }
            }

            return parameter;
        }

        SpiceObject CreateParameterSingle(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (!childrenItems[0].IsToken)
            {
                throw new ParseException();
            }

            switch (childrenItems[0].Token.TokenType)
            {
                case (int)SpiceToken.REFERENCE:
                    return new ReferenceParameter() { RawValue = childrenItems[0].Token.Value };
                case (int)SpiceToken.VALUE:
                    return new ValueParameter() { RawValue = childrenItems[0].Token.Value };
                case (int)SpiceToken.WORD:
                    return new WordParameter() { RawValue = childrenItems[0].Token.Value };
                case (int)SpiceToken.IDENTIFIER:
                    return new IdentifierParameter() { RawValue = childrenItems[0].Token.Value };
            }
            throw new ParseException();
        }

        SpiceObject CreateParameters(List<ParseTreeTranslatorItem> childrenItems)
        {
            var parameters = new ParameterCollection();

            if (childrenItems.Count == 2)
            {
                if (childrenItems[0].SpiceObject is Parameter p)
                {
                    parameters.Values.Add(p);
                }

                if (childrenItems[1].SpiceObject is ParameterCollection ps2)
                {
                    parameters.Values.AddRange(ps2.Values);
                }
            }

            return parameters;
        }

        SpiceObject CreateComponent(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (childrenItems.Count != 2 && childrenItems.Count != 3)
            {
                throw new ParseException();
            }
            var component = new Component();
            component.Name = childrenItems[0].Token.Value;
            component.Parameters = childrenItems[1].SpiceObject as ParameterCollection;
            return component;
        }

        SpiceObject CreateControl(List<ParseTreeTranslatorItem> childrenItems)
        {
            var control = new Control();
            control.Name = childrenItems[1].Token.Value;
            control.Parameters = childrenItems[2].SpiceObject as ParameterCollection;
            return control;
        }

        SpiceObject CreateSubCircuit(List<ParseTreeTranslatorItem> childrenItems)
        {
            var subCkt = new SubCircuit();
            subCkt.Name = childrenItems[1].Token.Value;
            subCkt.Parameters = childrenItems[2].SpiceObject as ParameterCollection;
            subCkt.Statements = childrenItems[4].SpiceObject as Statements;
            return subCkt;
        }

        SpiceObject CreateComment(List<ParseTreeTranslatorItem> childrenItems)
        {
            var comment = new CommentLine();
            comment.Text = childrenItems[1].Token.Value;
            return comment;
        }

        SpiceObject CreateStatement(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (childrenItems.Count == 1 && childrenItems[0].IsSpiceObject)
            {
                return childrenItems[0].SpiceObject as Statement;
            }
            throw new ParseException();
        }

        SpiceObject CreateModel(List<ParseTreeTranslatorItem> childrenItems)
        {
            var model = new Model();
            model.Name = childrenItems[2].Token.Value;
            model.Parameters = childrenItems[3].SpiceObject as ParameterCollection;
            return model;
        }

        SpiceObject CreateStatements(List<ParseTreeTranslatorItem> childrenItems)
        {
            var statements = new Statements();

            if (childrenItems.Count == 2)
            {
                if (childrenItems[0].SpiceObject is Statement st)
                {
                    statements.Add(st);
                }
                if (childrenItems[1].SpiceObject is Statements sts)
                {
                    statements.Merge(sts);
                }
            }
            else
            {
                if (childrenItems.Count == 1)
                {
                    if (childrenItems[0].SpiceObject is Statements sts)
                    {
                        statements.Merge(sts);
                    }
                    else
                    {
                        if (childrenItems[0].IsToken && childrenItems[0].Token.TokenType == (int)SpiceToken.END)
                        {
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            return statements;
        }

        SpiceObject CreateNetList(List<ParseTreeTranslatorItem> childrenItems)
        {
            return new NetList()
            {
                Title = childrenItems[0].Token.Value,
                Statements = childrenItems[1].SpiceObject as Statements
            };
        }
    }
}
