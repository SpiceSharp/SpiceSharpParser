using System;
using System.Collections.Generic;
using SpiceGrammar;
using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceParser.Evaluation;

namespace SpiceParser
{
    /// <summary>
    /// Evaluates a spice parse tree to Spice Object Model - SpiceNetlist library
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
            evaluators.Add(SpiceGrammarSymbol.VECTOR, (List<EvaluationValue> nt) => CreateVector(nt));
            evaluators.Add(SpiceGrammarSymbol.VECTOR_CONTINUE, (List<EvaluationValue> nt) => CreateVectorContinue(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_BRACKET, (List<EvaluationValue> nt) => CreateBracketParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT, (List<EvaluationValue> nt) => CreateBracketParameterContent(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL, (List<EvaluationValue> nt) => CreateAssigmentParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, (List<EvaluationValue> nt) => CreateAssigmentSimpleParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE, (List<EvaluationValue> nt) => CreateAssigmentParameters(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE_CONTINUE, (List<EvaluationValue> nt) => CreateAssigmentParametersContinue(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE, (List<EvaluationValue> nt) => CreateSingleParameters(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE_CONTINUE, (List<EvaluationValue> nt) => CreateSingleParametersContinue(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE, (List<EvaluationValue> nt) => CreateParameterSingle(nt));
            evaluators.Add(SpiceGrammarSymbol.SUBCKT, (List<EvaluationValue> nt) => CreateSubCircuit(nt));
            evaluators.Add(SpiceGrammarSymbol.SUBCKT_ENDING, (List<EvaluationValue> nt) => null);
            evaluators.Add(SpiceGrammarSymbol.COMMENT_LINE, (List<EvaluationValue> nt) => CreateComment(nt));
            evaluators.Add(SpiceGrammarSymbol.NEW_LINE_OR_EOF, (List<EvaluationValue> nt) => null);
        }

        /// <summary>
        /// Translates a spice parse tree to a context (SpiceNetlist library)
        /// </summary>
        /// <param name="root">A parse tree root</param>
        /// <returns>A net list</returns>
        public SpiceObject Evaluate(ParseTreeNode root)
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

                    if (!evaluators.ContainsKey(nt.Name))
                    {
                        throw new EvaluationException("Unsupported evaluation of parse tree node");
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
                return rootNt.SpiceObject;
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

            throw new EvaluationException("Wrong evaluation type");
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

            throw new EvaluationException("Wrong evaluation type");
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
        /// Returns new instance of <see cref="SingleParameter"/>
        /// or <see cref="BracketParameter"/> 
        /// or <see cref="AssignmentParameter"/>
        /// or <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER"/> parse tree node
        /// </summary>
        private SpiceObject CreateParameter(List<EvaluationValue> childrenValues)
        {
            Parameter parameter = null;

            if (childrenValues.Count == 1)
            {
                if (childrenValues[0] is NonTerminalEvaluationValue nt && nt.SpiceObject is VectorParameter v)
                {
                    parameter = v;
                }
                else if (childrenValues[0] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is SingleParameter sp)
                {
                    parameter = sp;
                }
                else if (childrenValues[0] is NonTerminalEvaluationValue nt3 && nt3.SpiceObject is BracketParameter bp)
                {
                    parameter = bp;
                } 
                else if (childrenValues[0] is NonTerminalEvaluationValue nt4 && nt4.SpiceObject is AssignmentParameter ap)
                {
                    parameter = ap;
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
                var lexemValue = t.Token.Lexem;
                switch (t.Token.TokenType)
                {
                    case (int)SpiceTokenType.REFERENCE:
                        return new ReferenceParameter(lexemValue);
                    case (int)SpiceTokenType.VALUE:
                        return new ValueParameter(lexemValue);
                    case (int)SpiceTokenType.WORD:
                        return new WordParameter(lexemValue);
                    case (int)SpiceTokenType.IDENTIFIER:
                        return new IdentifierParameter(lexemValue);
                    case (int)SpiceTokenType.EXPRESSION:
                        return new ExpressionParameter(lexemValue);
                }
            }

            throw new EvaluationException("Error during translating parse tree to Spice Object Model");
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
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
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
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
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
                    if (parameter is SingleParameter s && s.Image.ToLower() == "params:")
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
                            || int.TryParse(s2.Image, out _))
                        {
                            subCkt.Pins.Add(s2.Image);
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

            throw new EvaluationException("Error during translating parse tree to Spice Object Model");
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
        /// Returns new instance of <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.VECTOR"/> parse tree node
        /// </summary>
        private SpiceObject CreateVector(List<EvaluationValue> childrenValues)
        {
            var vector = new VectorParameter();

            if (childrenValues.Count == 4)
            {
                if (childrenValues[0] is NonTerminalEvaluationValue nt && nt.SpiceObject is SingleParameter p)
                {
                    vector.Elements.Add(p);
                }

                if (childrenValues[2] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is SingleParameter p2)
                {
                    vector.Elements.Add(p2);
                }

                if (childrenValues[2] is NonTerminalEvaluationValue nt3 && nt3.SpiceObject is VectorParameter v)
                {
                    vector.Elements.AddRange(v.Elements);
                }
            }

            return vector;
        }

        /// <summary>
        /// Returns new instance of <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.VECTOR_CONTINUE"/> parse tree node
        /// </summary>
        private SpiceObject CreateVectorContinue(List<EvaluationValue> childrenValues)
        {
            var vector = new VectorParameter();

            if (childrenValues.Count == 3)
            {
                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is SingleParameter ps2)
                {
                    vector.Elements.Add(ps2);
                }
            }
            else
            {
                if (childrenValues.Count == 1 && childrenValues[0] is NonTerminalEvaluationValue nt && nt.SpiceObject is VectorParameter p)
                {
                    vector.Elements.AddRange(p.Elements);
                }
            }

            return vector;
        }

        /// <summary>
        /// Returns new instance of <see cref="BracketParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_BRACKET"/> parse tree node
        /// </summary>
        private SpiceObject CreateBracketParameter(List<EvaluationValue> childrenValues)
        {
            var parameter = new BracketParameter();
            if (childrenValues.Count == 4)
            {
                parameter.Name = (childrenValues[0] as TerminalEvaluationValue).Token.Lexem;
                if (childrenValues[2] is NonTerminalEvaluationValue nt)
                {
                    if (nt.SpiceObject is ParameterCollection p)
                    {
                        parameter.Parameters = p;
                    }
                }
            }
            else
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }

            return parameter;
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.BRACKET_CONTENT"/> parse tree node
        /// </summary>
        private SpiceObject CreateBracketParameterContent(List<EvaluationValue> childrenValues)
        {
            var parameters = new ParameterCollection();

            if (childrenValues.Count == 0)
            {
                return parameters;
            }

            if (childrenValues.Count == 1 && childrenValues[0] is NonTerminalEvaluationValue nt)
            {
                if (nt.SpiceObject is ParameterCollection c)
                {
                    parameters.Merge(c);
                }
            }
            else
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }

            return parameters;
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE_CONTINUE"/> parse tree node
        /// </summary>
        private SpiceObject CreateAssigmentParametersContinue(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (childrenValues[0] is NonTerminalEvaluationValue nt1 && nt1.SpiceObject is AssignmentParameter ap)
                {
                    parameters.Add(ap);
                }

                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is ParameterCollection p)
                {
                    parameters.Merge(p);
                }

                return parameters;
            }
            else
            {
                if (childrenValues.Count != 0)
                {
                    throw new EvaluationException("Error during translating parse tree to Spice Object Model");
                }
                return new ParameterCollection();
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE"/> parse tree node
        /// </summary>
        private SpiceObject CreateAssigmentParameters(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (childrenValues[0] is NonTerminalEvaluationValue nt1 && nt1.SpiceObject is AssignmentParameter ap)
                {
                    parameters.Add(ap);
                }

                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is ParameterCollection p)
                {
                    parameters.Merge(p);
                }

                return parameters;
            }
            else
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE"/> parse tree node
        /// </summary>
        private SpiceObject CreateSingleParametersContinue(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (childrenValues[0] is NonTerminalEvaluationValue nt1 && nt1.SpiceObject is SingleParameter sp)
                {
                    parameters.Add(sp);
                }

                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is ParameterCollection p)
                {
                    parameters.Merge(p);
                }

                return parameters;
            }
            else
            {
                if (childrenValues.Count != 0)
                {
                    throw new EvaluationException("Error during translating parse tree to Spice Object Model");
                }
                return new ParameterCollection();
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE"/> parse tree node
        /// </summary>
        private SpiceObject CreateSingleParameters(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (childrenValues[0] is NonTerminalEvaluationValue nt1 && nt1.SpiceObject is SingleParameter sp)
                {
                    parameters.Add(sp);
                }

                if (childrenValues[1] is NonTerminalEvaluationValue nt2 && nt2.SpiceObject is ParameterCollection p)
                {
                    parameters.Merge(p);
                }

                return parameters;
            }
            else
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE"/> parse tree node
        /// </summary>
        private SpiceObject CreateAssigmentSimpleParameter(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 3)
            {
                var assigmentParameter = new AssignmentParameter();
                assigmentParameter.Name = (childrenValues[0] as TerminalEvaluationValue).Token.Lexem;
                assigmentParameter.Value = ((childrenValues[2] as NonTerminalEvaluationValue).SpiceObject as SingleParameter).Image;

                return assigmentParameter;
            }
            else
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL"/> parse tree node
        /// </summary>
        private SpiceObject CreateAssigmentParameter(List<EvaluationValue> childrenValues)
        {
            if (childrenValues.Count == 1)
            {
                if (childrenValues[0] is NonTerminalEvaluationValue nt1 && nt1.SpiceObject is AssignmentParameter ap)
                {
                    return ap;
                }
                else
                {
                    throw new EvaluationException("Error during translating parse tree to Spice Object Model");
                }
            }
            else
            {
                if (childrenValues.Count == 6)
                {
                    // v(2) = 3
                    var assigmentParameter = new AssignmentParameter();
                    assigmentParameter.Name = (childrenValues[0] as TerminalEvaluationValue).Token.Lexem;
                    assigmentParameter.Arguments.Add(((childrenValues[2] as NonTerminalEvaluationValue).SpiceObject as SingleParameter).Image);
                    assigmentParameter.Value = ((childrenValues[5] as NonTerminalEvaluationValue).SpiceObject as SingleParameter).Image;

                    return assigmentParameter;
                }

                if (childrenValues.Count == 8)
                {
                    // v(2,3) = 4
                    var assigmentParameter = new AssignmentParameter();
                    assigmentParameter.Name = (childrenValues[0] as TerminalEvaluationValue).Token.Lexem;
                    assigmentParameter.Arguments.Add(((childrenValues[2] as NonTerminalEvaluationValue).SpiceObject as SingleParameter).Image);
                    assigmentParameter.Arguments.Add(((childrenValues[4] as NonTerminalEvaluationValue).SpiceObject as SingleParameter).Image);
                    assigmentParameter.Value = ((childrenValues[7] as NonTerminalEvaluationValue).SpiceObject as SingleParameter).Image;

                    return assigmentParameter;
                }
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }
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
                        if (childrenValues[0] is TerminalEvaluationValue t1 && t1.Token.Is(SpiceTokenType.END))
                        {
                            // skip
                        }
                        else
                        {
                            throw new EvaluationException("Error during translating parse tree to Spice Object Model");
                        }
                    }
                }
            }

            return statements;
        }
    }
}
