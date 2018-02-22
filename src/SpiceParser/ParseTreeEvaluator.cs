using System;
using System.Collections.Generic;
using SpiceGrammar;
using SpiceLexer;
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
        private Dictionary<string, Func<EvaluationValues, SpiceObject>> evaluators = new Dictionary<string, Func<EvaluationValues, SpiceObject>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeEvaluator"/> class.
        /// </summary>
        public ParseTreeEvaluator()
        {
            evaluators.Add(SpiceGrammarSymbol.START, (EvaluationValues nt) => CreateNetlist(nt));
            evaluators.Add(SpiceGrammarSymbol.STATEMENTS, (EvaluationValues nt) => CreateStatements(nt));
            evaluators.Add(SpiceGrammarSymbol.STATEMENT, (EvaluationValues nt) => CreateStatement(nt));
            evaluators.Add(SpiceGrammarSymbol.MODEL, (EvaluationValues nt) => CreateModel(nt));
            evaluators.Add(SpiceGrammarSymbol.CONTROL, (EvaluationValues nt) => CreateControl(nt));
            evaluators.Add(SpiceGrammarSymbol.COMPONENT, (EvaluationValues nt) => CreateComponent(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETERS, (EvaluationValues nt) => CreateParameters(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER, (EvaluationValues nt) => CreateParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.VECTOR, (EvaluationValues nt) => CreateVector(nt));
            evaluators.Add(SpiceGrammarSymbol.VECTOR_CONTINUE, (EvaluationValues nt) => CreateVectorContinue(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_BRACKET, (EvaluationValues nt) => CreateBracketParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT, (EvaluationValues nt) => CreateBracketParameterContent(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL, (EvaluationValues nt) => CreateAssigmentParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, (EvaluationValues nt) => CreateAssigmentSimpleParameter(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE, (EvaluationValues nt) => CreateAssigmentParameters(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SEQUANCE_CONTINUE, (EvaluationValues nt) => CreateAssigmentParametersContinue(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE, (EvaluationValues nt) => CreateSingleParameters(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE_CONTINUE, (EvaluationValues nt) => CreateSingleParametersContinue(nt));
            evaluators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE, (EvaluationValues nt) => CreateParameterSingle(nt));
            evaluators.Add(SpiceGrammarSymbol.SUBCKT, (EvaluationValues nt) => CreateSubCircuit(nt));
            evaluators.Add(SpiceGrammarSymbol.SUBCKT_ENDING, (EvaluationValues nt) => null);
            evaluators.Add(SpiceGrammarSymbol.COMMENT_LINE, (EvaluationValues nt) => CreateComment(nt));
            evaluators.Add(SpiceGrammarSymbol.NEW_LINE_OR_EOF, (EvaluationValues nt) => null);
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
                    var items = new EvaluationValues();

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
        /// Returns new instance of <see cref="Netlist"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.START"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Netlist"/>
        /// </returns>
        private SpiceObject CreateNetlist(EvaluationValues values)
        {
            return new Netlist()
            {
                Title = values.GetLexem(0),
                Statements = values.GetSpiceObject<Statements>(1)
            };
        }

        /// <summary>
        /// Returns new instance of <see cref="SingleParameter"/>
        /// or <see cref="BracketParameter"/>
        /// or <see cref="AssignmentParameter"/>
        /// or <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SingleParameter"/>
        /// </returns>
        private SpiceObject CreateParameter(EvaluationValues values)
        {
            if (values.Count == 1)
            {
                if (values.TryToGetSpiceObject(0, out VectorParameter vp))
                {
                    return vp;
                }

                if (values.TryToGetSpiceObject(0, out SingleParameter sp))
                {
                    return sp;
                }

                if (values.TryToGetSpiceObject(0, out BracketParameter bp))
                {
                    return bp;
                }

                if (values.TryToGetSpiceObject(0, out AssignmentParameter ap))
                {
                    return ap;
                }
            }

            throw new EvaluationException("Error during translating parse tree to Spice Object Model");
        }

        /// <summary>
        /// Returns new instance of <see cref="ReferenceParameter"/>
        /// or <see cref="ValueParameter"/> or <see cref="WordParameter"/>
        /// or <see cref="ExpressionParameter"/> or <see cref="IdentifierParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_SINGLE"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SingleParameter"/>
        /// </returns>
        private SpiceObject CreateParameterSingle(EvaluationValues values)
        {
            if (values[0] is TerminalEvaluationValue t)
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
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateParameters(EvaluationValues values)
        {
            var parameters = new ParameterCollection();

            if (values.Count == 2)
            {
                parameters.Add(values.GetSpiceObject<Parameter>(0));
                parameters.Merge(values.GetSpiceObject<ParameterCollection>(1));
            }

            return parameters;
        }

        /// <summary>
        /// Creates an instance of <see cref="Component"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.COMPONENT"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Component"/>
        /// </returns>
        private SpiceObject CreateComponent(EvaluationValues values)
        {
            if (values.Count != 2 && values.Count != 3)
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }

            var component = new Component();
            component.Name = values.GetLexem(0);
            component.PinsAndParameters = values.GetSpiceObject<ParameterCollection>(1);
            component.LineNumber = values.GetLexemLineNumber(0);
            return component;
        }

        /// <summary>
        /// Returns new instance of <see cref="Control"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.CONTROL"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Control"/>
        /// </returns>
        private SpiceObject CreateControl(EvaluationValues values)
        {
            var control = new Control();
            control.Name = values.GetLexem(1);
            control.Parameters = values.GetSpiceObject<ParameterCollection>(2);
            control.LineNumber = values.GetLexemLineNumber(1);
            return control;
        }

        /// <summary>
        /// Returns new instance of <see cref="SubCircuit"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.SUBCKT"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SubCircuit"/>
        /// </returns>
        private SpiceObject CreateSubCircuit(EvaluationValues values)
        {
            if (values.Count < 3)
            {
                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }

            var subCkt = new SubCircuit();
            subCkt.Name = values.GetLexem(2);
            subCkt.LineNumber = values.GetLexemLineNumber(2);

            var allParameters = values.GetSpiceObject<ParameterCollection>(3);

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

            subCkt.Statements = values.GetSpiceObject<Statements>(5);
            return subCkt;
        }

        /// <summary>
        /// Returns new instance of <see cref="CommentLine"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.COMMENT_LINE"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="CommentLine"/>
        /// </returns>
        private SpiceObject CreateComment(EvaluationValues values)
        {
            var comment = new CommentLine();
            comment.Text = values.GetLexem(1);
            comment.LineNumber = values.GetLexemLineNumber(1);
            return comment;
        }

        /// <summary>
        /// Returns new instance of <see cref="Statement"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.STATEMENT"/> parse tree node
        /// </summary>
        /// <returns>
        /// A instance of <see cref="Statement"/>
        /// </returns>
        private SpiceObject CreateStatement(EvaluationValues values)
        {
            if (values.Count == 1)
            {
                return values.GetSpiceObject<Statement>(0);
            }

            throw new EvaluationException("Error during translating parse tree to Spice Object Model");
        }

        /// <summary>
        /// Returns new instance of <see cref="Model"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.MODEL"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Model"/>
        /// </returns>
        private SpiceObject CreateModel(EvaluationValues values)
        {
            var model = new Model();
            model.Name = values.GetLexem(2);
            model.Parameters = values.GetSpiceObject<ParameterCollection>(3);
            model.LineNumber = values.GetLexemLineNumber(2);
            return model;
        }

        /// <summary>
        /// Returns new instance of <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.VECTOR"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="VectorParameter"/>
        /// </returns>
        private SpiceObject CreateVector(EvaluationValues values)
        {
            var vector = new VectorParameter();

            if (values.Count == 4)
            {
                vector.Elements.Add(values.GetSpiceObject<SingleParameter>(0));
                vector.Elements.Add(values.GetSpiceObject<SingleParameter>(2));
                vector.Elements.AddRange(values.GetSpiceObject<VectorParameter>(3).Elements);
            }

            return vector;
        }

        /// <summary>
        /// Returns new instance of <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.VECTOR_CONTINUE"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="VectorParameter"/>
        /// </returns>
        private SpiceObject CreateVectorContinue(EvaluationValues values)
        {
            var vector = new VectorParameter();

            if (values.Count == 3)
            {
                vector.Elements.Add(values.GetSpiceObject<SingleParameter>(1));
                vector.Elements.AddRange(values.GetSpiceObject<VectorParameter>(2).Elements);
            }

            return vector;
        }

        /// <summary>
        /// Returns new instance of <see cref="BracketParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_BRACKET"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="BracketParameter"/>
        /// </returns>
        private SpiceObject CreateBracketParameter(EvaluationValues values)
        {
            var parameter = new BracketParameter();
            if (values.Count == 4)
            {
                parameter.Name = values.GetLexem(0);
                parameter.Parameters = values.GetSpiceObject<ParameterCollection>(2);
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
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateBracketParameterContent(EvaluationValues values)
        {
            var parameters = new ParameterCollection();

            if (values.Count == 0)
            {
                return parameters;
            }

            if (values.Count == 1 && values.TryToGetSpiceObject(0, out ParameterCollection pc))
            {
                parameters.Merge(pc);
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
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateAssigmentParametersContinue(EvaluationValues values)
        {
            if (values.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (values.TryToGetSpiceObject(0, out AssignmentParameter ap))
                {
                    parameters.Add(ap);
                }

                if (values.TryToGetSpiceObject(1, out ParameterCollection p))
                {
                    parameters.Merge(p);
                }

                return parameters;
            }
            else
            {
                if (values.Count != 0)
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
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateAssigmentParameters(EvaluationValues values)
        {
            if (values.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (values.TryToGetSpiceObject(0, out AssignmentParameter ap))
                {
                    parameters.Add(ap);
                }

                if (values.TryToGetSpiceObject(1, out ParameterCollection p))
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
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateSingleParametersContinue(EvaluationValues values)
        {
            if (values.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (values.TryToGetSpiceObject(0, out SingleParameter sp))
                {
                    parameters.Add(sp);
                }

                if (values.TryToGetSpiceObject(1, out ParameterCollection p))
                {
                    parameters.Merge(p);
                }

                return parameters;
            }
            else
            {
                if (values.Count != 0)
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
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateSingleParameters(EvaluationValues values)
        {
            if (values.Count == 2)
            {
                var parameters = new ParameterCollection();

                if (values.TryToGetSpiceObject(0, out SingleParameter sp))
                {
                    parameters.Add(sp);
                }

                if (values.TryToGetSpiceObject(1, out ParameterCollection p))
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
        /// <returns>
        /// A new instance of <see cref="AssignmentParameter"/>
        /// </returns>
        private SpiceObject CreateAssigmentSimpleParameter(EvaluationValues values)
        {
            if (values.Count == 3)
            {
                var assigmentParameter = new AssignmentParameter();
                assigmentParameter.Name = values.GetLexem(0);
                assigmentParameter.Value = values.GetSpiceObject<SingleParameter>(2).Image;

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
        /// <returns>
        /// A instance of <see cref="AssignmentParameter"/>
        /// </returns>
        private SpiceObject CreateAssigmentParameter(EvaluationValues values)
        {
            if (values.Count == 1)
            {
                return values.GetSpiceObject<AssignmentParameter>(0);
            }
            else
            {
                if (values.Count == 6)
                {
                    // v(2) = 3
                    var assigmentParameter = new AssignmentParameter();
                    assigmentParameter.Name = values.GetLexem(0);
                    assigmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(2).Image);
                    assigmentParameter.Value = values.GetSpiceObject<SingleParameter>(5).Image;

                    return assigmentParameter;
                }

                if (values.Count == 8)
                {
                    // v(2,3) = 4
                    var assigmentParameter = new AssignmentParameter();
                    assigmentParameter.Name = (values[0] as TerminalEvaluationValue).Token.Lexem;
                    assigmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(2).Image);
                    assigmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(4).Image);
                    assigmentParameter.Value = values.GetSpiceObject<SingleParameter>(7).Image;

                    return assigmentParameter;
                }

                throw new EvaluationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="Statements"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.STATEMENTS"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Statements"/>
        /// </returns>
        private SpiceObject CreateStatements(EvaluationValues values)
        {
            var statements = new Statements();

            if (values.Count == 2)
            {
                if (values.TryToGetSpiceObject<Statement>(0, out Statement st))
                {
                    statements.Add(st);
                }

                statements.Merge(values.GetSpiceObject<Statements>(1));
            }
            else
            {
                if (values.Count == 1)
                {
                    if (values.TryToGetSpiceObject(0, out Statements sts))
                    {
                        statements.Merge(sts);
                    }
                    else
                    {
                        if (values.TryToGetToken(0, out SpiceToken token) && token.Is(SpiceTokenType.END))
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
