using System;
using System.Collections.Generic;
using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceParser.Evaluation;

namespace SpiceParser
{
    /// <summary>
    /// Evaluates a spice parse tree to SOM (Spice Object Model - SpiceNetlist library)
    /// </summary>
    public class ParseTreeEvaluator
    {
        /// <summary>
        /// The dictionary with tree node values
        /// </summary>
        private Dictionary<ParseTreeNode, EvaluationValue> treeNodesValues = new Dictionary<ParseTreeNode, EvaluationValue>();

        /// <summary>
        /// The dictionary with non-terminal nodes evaluators
        /// </summary>
        private Dictionary<string, Func<List<EvaluationValue>, SpiceObject>> evaluators = new Dictionary<string, Func<List<EvaluationValue>, SpiceObject>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeEvaluator"/> class.
        /// </summary>
        public ParseTreeEvaluator()
        {
            evaluators.Add(SpiceGrammarSymbol.START, (List<EvaluationValue> nt) => CreateNetList(nt));
            evaluators.Add(SpiceGrammarSymbol.STATEMENTS, (List<EvaluationValue> nt) => CreateStatements(nt));
            evaluators.Add(SpiceGrammarSymbol.STATEMENT, (List<EvaluationValue> nt) => CreateStatement(nt));
            evaluators.Add(SpiceGrammarSymbol.MODEL, (List<EvaluationValue> nt) => CreateModel(nt));
            evaluators.Add(SpiceGrammarSymbol.CONTROL, (List<EvaluationValue> nt) => CreateControl(nt));
            evaluators.Add(SpiceGrammarSymbol.COMPONENT, (List<EvaluationValue> nt) => CreateComponent(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETERS, (List<EvaluationValue> nt) => CreateParameters(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER, (List<EvaluationValue> nt) => CreateParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE, (List<EvaluationValue> nt) => CreateParameterSingle(nt));
            evaluators.Add(SpiceGrammarSymbol.SUBCKT, (List<EvaluationValue> nt) => CreateSubCircuit(nt));
            evaluators.Add(SpiceGrammarSymbol.COMMENT_LINE, (List<EvaluationValue> nt) => CreateComment(nt));
            evaluators.Add(SpiceGrammarSymbol.NEW_LINE_OR_EOF, (List<EvaluationValue> nt) => null);
            evaluators.Add(SpiceGrammarSymbol.SUBCKT_ENDING, (List<EvaluationValue> nt) => null);
        }

        /// <summary>
        /// Translates a spice parse tree to a context (SpiceNetlist library)
        /// </summary>
        /// <param name="root">A parse tree root</param>
        /// <returns>A net list</returns>
        public NetList Evaluate(ParseTreeNode root)
        {
            var travelsal = new ParseTreeTravelsal();

            // Get tree nodes in post order
            var treeNodes = travelsal.GetIterativePostOrder(root);

            // Iterate over tree nodes
            foreach (var treeNode in treeNodes)
            {
                if (treeNode is ParseTreeNonTerminalNode nt)
                {
                    var items = new List<EvaluationValue>();

                    foreach (var child in nt.Children)
                    {
                        items.Add(treeNodesValues[child]);
                    }

                    var treeNodeResult = evaluators[nt.Name](items);
                    treeNodesValues[treeNode] = new NonTerminalEvaluationValue
                    {
                        SpiceObject = treeNodeResult,
                        Node = treeNode
                    };
                }
                else
                {
                    treeNodesValues[treeNode] = new TerminalEvaluationValue()
                    {
                        Node = treeNode,
                        Token = ((ParseTreeTerminalNode)treeNode).Token
                    };
                }
            }

            if (treeNodesValues[root] is NonTerminalEvaluationValue rootNt)
            {
                return rootNt.SpiceObject as NetList;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets lexem from specific EvaluationValue item
        /// </summary>
        private string GetLexem(List<EvaluationValue> childrenValues, int index)
        {
            if (childrenValues[index] is TerminalEvaluationValue t)
            {
                return t.Token.Lexem;
            }

            throw new ParseException("Wrong evaluation type");
        }

        /// <summary>
        /// Gets SpiceObject from specific EvaluationValue item
        /// </summary>
        private T GetSpiceObject<T>(List<EvaluationValue> childrenValues, int index)
            where T : SpiceObject
        {
            if (childrenValues[index] is NonTerminalEvaluationValue nt)
            {
                if (nt.SpiceObject is T)
                {
                    return (T)nt.SpiceObject;
                }
            }

            throw new ParseException("Wrong evaluation type");
        }

        /// <summary>
        /// Returns new instance of <see cref="NetList"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.START"/> parse tree node
        /// </summary>
        private SpiceObject CreateNetList(List<EvaluationValue> childrenValues)
        {
            return new NetList()
            {
                Title = GetLexem(childrenValues, 0),
                Statements = GetSpiceObject<Statements>(childrenValues, 1)
            };
        }

        /// <summary>
        /// Returns new instance of <see cref="SingleParameter"/> or <see cref="ComplexParameter"/> or <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER"/> parse tree node
        /// </summary>
        private SpiceObject CreateParameter(List<EvaluationValue> childrenValues)
        {
            Parameter parameter = null;

            if (childrenValues.Count > 0)
            {
                if (childrenValues[0] is NonTerminalEvaluationValue nt && nt.SpiceObject is SingleParameter sp)
                {
                    parameter = sp;
                }
                else
                {
                    if (childrenValues[0] is TerminalEvaluationValue t1
                        && t1.Token.Is(SpiceToken.WORD)
                        && childrenValues[1] is TerminalEvaluationValue t2
                        && t2.Token.Is(SpiceToken.DELIMITER)
                        && t2.Token.Lexem == "("
                        && childrenValues[2] is NonTerminalEvaluationValue nt1
                        && nt1.SpiceObject is ParameterCollection
                        && childrenValues[3] is TerminalEvaluationValue t3
                        && t3.Token.Is(SpiceToken.DELIMITER)
                        && t3.Token.Lexem == ")")
                    {
                        parameter = new ComplexParameter()
                        {
                            Name = t1.Token.Lexem,
                            Parameters = nt1.SpiceObject as ParameterCollection
                        };
                    }

                    if (childrenValues[0] is TerminalEvaluationValue nameEval && nameEval.Token.Is(SpiceToken.WORD)
                        && childrenValues[1] is TerminalEvaluationValue t5 && t5.Token.Lexem == "="
                        && childrenValues[2] is TerminalEvaluationValue valueEval && valueEval.Token.Is(SpiceToken.VALUE))
                    {
                        parameter = new AssignmentParameter()
                        {
                            Name = nameEval.Token.Lexem,
                            Value = valueEval.Token.Lexem
                        };
                    }
                }
            }

            return parameter;
        }

        /// <summary>
        /// Returns new instance of <see cref="ReferenceParameter"/>
        /// or <see cref="ValueParameter"/> or <see cref="WordParameter"/>
        /// or <see cref="ExpressionParameter"/> or <see cref="IdentifierParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_SINGLE"/> parse tree node
        /// </summary>
        private SpiceObject CreateParameterSingle(List<EvaluationValue> childrenValues)
        {
            if (childrenValues[0] is TerminalEvaluationValue t)
            {
                var rawValue = t.Token.Lexem;
                switch (t.Token.TokenType)
                {
                    case (int)SpiceToken.REFERENCE:
                        return new ReferenceParameter() { RawValue = rawValue };
                    case (int)SpiceToken.VALUE:
                        return new ValueParameter() { RawValue = rawValue };
                    case (int)SpiceToken.WORD:
                        return new WordParameter() { RawValue = rawValue };
                    case (int)SpiceToken.IDENTIFIER:
                        return new IdentifierParameter() { RawValue = rawValue };
                    case (int)SpiceToken.EXPRESSION:
                        return new ExpressionParameter() { RawValue = rawValue };
                }
            }

            throw new ParseException();
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETERS"/> parse tree node
        /// </summary>
        private SpiceObject CreateParameters(List<EvaluationValue> childrenValues)
        {
            var parameters = new ParameterCollection();

            if (childrenValues.Count == 2)
            {
                if (childrenValues[0] is NonTerminalEvaluationValue nt && nt.SpiceObject is Parameter p)
                {
                    parameters.Add(p);
                }

                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is ParameterCollection ps2)
                {
                    parameters.Merge(ps2);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Creates an instance of <see cref="Component"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.COMPONENT"/> parse tree node
        /// </summary>
        private SpiceObject CreateComponent(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count != 2 && childrenValues.Count != 3)
            {
                throw new ParseException();
            }

            var component = new Component();
            component.Name = GetLexem(childrenValues, 0);
            component.PinsAndParameters = GetSpiceObject<ParameterCollection>(childrenValues, 1);
            return component;
        }

        /// <summary>
        /// Returns new instance of <see cref="Control"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.CONTROL"/> parse tree node
        /// </summary>
        private SpiceObject CreateControl(List<EvaluationValue> childrenValues)
        {
            var control = new Control();
            control.Name = GetLexem(childrenValues, 1);
            control.Parameters = GetSpiceObject<ParameterCollection>(childrenValues, 2);
            return control;
        }

        /// <summary>
        /// Returns new instance of <see cref="SubCircuit"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.SUBCKT"/> parse tree node
        /// </summary>
        private SpiceObject CreateSubCircuit(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count < 3)
            {
                throw new ParseException();
            }

            var subCkt = new SubCircuit();
            subCkt.Name = GetLexem(childrenValues, 2);

            var allParameters = GetSpiceObject<ParameterCollection>(childrenValues, 3);

            // Parse nodes and parameters
            bool mode = true; // true = nodes, false = parameters
            foreach (var parameter in allParameters)
            {
                if (mode)
                {
                    // After this, only parameters will follow
                    if (parameter is SingleParameter s && s.RawValue.ToLower() == "params:")
                    {
                        mode = false;
                    }

                    // Parameters have started, so we will keep reading parameters
                    else if (parameter is AssignmentParameter a)
                    {
                        mode = false;
                        subCkt.DefaultParameters.Add(a);
                    }

                    // Still reading nodes
                    else if (parameter is SingleParameter s2)
                    {
                        if (s2 is WordParameter
                            || s2 is IdentifierParameter
                            || int.TryParse(s2.RawValue, out _))
                        {
                            subCkt.Pins.Add(s2.RawValue);
                        }
                    }
                }
                else if (parameter is AssignmentParameter a2)
                {
                    subCkt.DefaultParameters.Add(a2);
                }
            }

            subCkt.Statements = GetSpiceObject<Statements>(childrenValues, 5);
            return subCkt;
        }

        /// <summary>
        /// Returns new instance of <see cref="CommentLine"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.COMMENT_LINE"/> parse tree node
        /// </summary>
        private SpiceObject CreateComment(List<EvaluationValue> childrenValues)
        {
            var comment = new CommentLine();
            comment.Text = GetLexem(childrenValues, 1);
            return comment;
        }

        /// <summary>
        /// Returns new instance of <see cref="Statement"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.STATEMENT"/> parse tree node
        /// </summary>
        private SpiceObject CreateStatement(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 1 && childrenValues[0] is NonTerminalEvaluationValue)
            {
                return GetSpiceObject<Statement>(childrenValues, 0);
            }

            throw new ParseException();
        }

        /// <summary>
        /// Returns new instance of <see cref="Model"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.MODEL"/> parse tree node
        /// </summary>
        private SpiceObject CreateModel(List<EvaluationValue> childrenValues)
        {
            var model = new Model();
            model.Name = GetLexem(childrenValues, 2);
            model.Parameters = GetSpiceObject<ParameterCollection>(childrenValues, 3);
            return model;
        }

        /// <summary>
        /// Returns new instance of <see cref="Statements"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.STATEMENTS"/> parse tree node
        /// </summary>
        private SpiceObject CreateStatements(List<EvaluationValue> childrenValues)
        {
            var statements = new Statements();

            if (childrenValues.Count == 2)
            {
                if (childrenValues[0] is NonTerminalEvaluationValue nt1 && nt1.SpiceObject is Statement st)
                {
                    statements.Add(st);
                }

                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is Statements sts)
                {
                    statements.Merge(sts);
                }
            }
            else
            {
                if (childrenValues.Count == 1)
                {
                    if (childrenValues[0] is NonTerminalEvaluationValue nt3 && nt3.SpiceObject is Statements sts)
                    {
                        statements.Merge(sts);
                    }
                    else
                    {
                        if (childrenValues[0] is TerminalEvaluationValue t1 && t1.Token.Is(SpiceToken.END))
                        {
                            // skip
                        }
                        else
                        {
                            throw new ParseException();
                        }
                    }
                }
            }

            return statements;
        }
    }
}
