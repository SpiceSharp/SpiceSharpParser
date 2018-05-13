using System;
using System.Collections.Generic;
using SpiceSharpParser.Grammar;
using SpiceSharpParser.Lexer.Spice3f5;
using SpiceSharpParser.Model;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using SpiceSharpParser.Parser.Exceptions;
using SpiceSharpParser.Parser.TreeGeneration;

namespace SpiceSharpParser.Parser.TreeTranslator
{
    /// <summary>
    /// Translates a parse tree (<see cref="ParseTreeNode"/> to Spice Object Model - SpiceNetlist library
    /// </summary>
    public class ParseTreeTranslator
    {
        /// <summary>
        /// The dictionary with tree node values
        /// </summary>
        private Dictionary<ParseTreeNode, ParseTreeNodeTranslationValue> treeNodesValues = new Dictionary<ParseTreeNode, ParseTreeNodeTranslationValue>();

        /// <summary>
        /// The dictionary with non-terminal nodes evaluators
        /// </summary>
        private Dictionary<string, Func<ParseTreeNodeTranslationValues, SpiceObject>> translators = new Dictionary<string, Func<ParseTreeNodeTranslationValues, SpiceObject>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeTranslator"/> class.
        /// </summary>
        public ParseTreeTranslator()
        {
            translators.Add(SpiceGrammarSymbol.NETLIST, (ParseTreeNodeTranslationValues nt) => CreateNetlist(nt));
            translators.Add(SpiceGrammarSymbol.NETLIST_WITHOUT_TITLE, (ParseTreeNodeTranslationValues nt) => CreateNetlistWithoutTitle(nt));
            translators.Add(SpiceGrammarSymbol.NETLIST_ENDING, (ParseTreeNodeTranslationValues nt) => null);
            translators.Add(SpiceGrammarSymbol.STATEMENTS, (ParseTreeNodeTranslationValues nt) => CreateStatements(nt));
            translators.Add(SpiceGrammarSymbol.STATEMENT, (ParseTreeNodeTranslationValues nt) => CreateStatement(nt));
            translators.Add(SpiceGrammarSymbol.MODEL, (ParseTreeNodeTranslationValues nt) => CreateModel(nt));
            translators.Add(SpiceGrammarSymbol.CONTROL, (ParseTreeNodeTranslationValues nt) => CreateControl(nt));
            translators.Add(SpiceGrammarSymbol.COMPONENT, (ParseTreeNodeTranslationValues nt) => CreateComponent(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETERS, (ParseTreeNodeTranslationValues nt) => CreateParameters(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER, (ParseTreeNodeTranslationValues nt) => CreateParameter(nt));
            translators.Add(SpiceGrammarSymbol.VECTOR, (ParseTreeNodeTranslationValues nt) => CreateVector(nt));
            translators.Add(SpiceGrammarSymbol.VECTOR_CONTINUE, (ParseTreeNodeTranslationValues nt) => CreateVectorContinue(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER_BRACKET, (ParseTreeNodeTranslationValues nt) => CreateBracketParameter(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER_BRACKET_CONTENT, (ParseTreeNodeTranslationValues nt) => CreateBracketParameterContent(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL, (ParseTreeNodeTranslationValues nt) => CreateAssigmentParameter(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE, (ParseTreeNodeTranslationValues nt) => CreateAssigmentSimpleParameter(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE, (ParseTreeNodeTranslationValues nt) => CreateParameterSingle(nt));
            translators.Add(SpiceGrammarSymbol.SUBCKT, (ParseTreeNodeTranslationValues nt) => CreateSubCircuit(nt));
            translators.Add(SpiceGrammarSymbol.SUBCKT_ENDING, (ParseTreeNodeTranslationValues nt) => null);
            translators.Add(SpiceGrammarSymbol.COMMENT_LINE, (ParseTreeNodeTranslationValues nt) => CreateComment(nt));
            translators.Add(SpiceGrammarSymbol.NEW_LINE, (ParseTreeNodeTranslationValues nt) => null);
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
                    var items = new ParseTreeNodeTranslationValues();

                    foreach (var child in nt.Children)
                    {
                        items.Add(treeNodesValues[child]);
                    }

                    if (!translators.ContainsKey(nt.Name))
                    {
                        throw new TranslationException("Unsupported evaluation of parse tree node");
                    }

                    var treeNodeResult = translators[nt.Name](items);
                    treeNodesValues[treeNode] = new ParseTreeNonTerminalTranslationValue
                    {
                        SpiceObject = treeNodeResult,
                        Node = treeNode
                    };
                }
                else
                {
                    treeNodesValues[treeNode] = new ParseTreeNodeTerminalTranslationValue()
                    {
                        Node = treeNode,
                        Token = ((ParseTreeTerminalNode)treeNode).Token
                    };
                }
            }

            if (treeNodesValues[root] is ParseTreeNonTerminalTranslationValue rootNt)
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
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.NETLIST"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Netlist"/>
        /// </returns>
        private SpiceObject CreateNetlist(ParseTreeNodeTranslationValues values)
        {
            if (values.Count == 3)
            {
                return new Netlist()
                {
                    Title = string.Empty,
                    Statements = values.GetSpiceObject<Statements>(1)
                };
            }
            else
            {
                return new Netlist()
                {
                    Title = values.GetLexem(0),
                    Statements = values.GetSpiceObject<Statements>(2)
                };
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="Netlist"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.NETLIST"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Netlist"/>
        /// </returns>
        private SpiceObject CreateNetlistWithoutTitle(ParseTreeNodeTranslationValues values)
        {
            return new Netlist()
            {
                Title = null,
                Statements = values.GetSpiceObject<Statements>(0)
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
        private SpiceObject CreateParameter(ParseTreeNodeTranslationValues values)
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

            throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        private SpiceObject CreateParameterSingle(ParseTreeNodeTranslationValues values)
        {
            if (values[0] is ParseTreeNodeTerminalTranslationValue t)
            {
                var lexemValue = t.Token.Lexem;
                switch (t.Token.TokenType)
                {
                    case (int)SpiceTokenType.REFERENCE:
                        return new ReferenceParameter(lexemValue);
                    case (int)SpiceTokenType.STRING:
                        return new StringParameter(lexemValue);
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

            throw new TranslationException("Error during translating parse tree to Spice Object Model");
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETERS"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateParameters(ParseTreeNodeTranslationValues values)
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
        private SpiceObject CreateComponent(ParseTreeNodeTranslationValues values)
        {
            if (values.Count != 2 && values.Count != 3)
            {
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        private SpiceObject CreateControl(ParseTreeNodeTranslationValues values)
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
        private SpiceObject CreateSubCircuit(ParseTreeNodeTranslationValues values)
        {
            if (values.Count < 3)
            {
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.COMMENT_STATEMENT"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="CommentLine"/>
        /// </returns>
        private SpiceObject CreateStatementComment(ParseTreeNodeTranslationValues values)
        {
            if (values.Count == 1)
            {
                var comment = new CommentLine();
                comment.Text = values.GetLexem(0).Substring(1);
                return comment;
            }

            return null;
        }

        /// <summary>
        /// Returns new instance of <see cref="CommentLine"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.COMMENT_LINE"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="CommentLine"/>
        /// </returns>
        private SpiceObject CreateComment(ParseTreeNodeTranslationValues values)
        {
            var comment = new CommentLine();
            if (values.Count == 2)
            {
                comment.Text = values.GetLexem(1);
                comment.LineNumber = values.GetLexemLineNumber(1);
            }
            else
            {
                comment.Text = string.Empty;
                comment.LineNumber = values.GetLexemLineNumber(0);
            }
            return comment;
        }

        /// <summary>
        /// Returns new instance of <see cref="Statement"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.STATEMENT"/> parse tree node
        /// </summary>
        /// <returns>
        /// A instance of <see cref="Statement"/>
        /// </returns>
        private SpiceObject CreateStatement(ParseTreeNodeTranslationValues values)
        {
            if (values.Count != 3 && values.Count != 2)
            {
                throw new TranslationException("Error during translating statement - Wrong elements count for statement");
            }

            if (!(values[values.Count - 1] is ParseTreeNodeTerminalTranslationValue tv && tv.Token.Is(SpiceTokenType.NEWLINE)))
            {
                throw new TranslationException("Error during translating statement - Statement is not finished by newline");
            }

            if (values.Count == 3 && values[1] is ParseTreeNonTerminalTranslationValue nv && nv.SpiceObject != null && !(nv.SpiceObject is CommentLine c))
            {
                throw new TranslationException("Error during translating statement - Statement has second element that is not comment");
            }

            var statement = values.GetSpiceObject<Statement>(0);

            return statement;
        }

        /// <summary>
        /// Returns new instance of <see cref="Model"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.MODEL"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Model"/>
        /// </returns>
        private SpiceObject CreateModel(ParseTreeNodeTranslationValues values)
        {
            var model = new Model.SpiceObjects.Model();
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
        private SpiceObject CreateVector(ParseTreeNodeTranslationValues values)
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
        private SpiceObject CreateVectorContinue(ParseTreeNodeTranslationValues values)
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
        private SpiceObject CreateBracketParameter(ParseTreeNodeTranslationValues values)
        {
            var parameter = new BracketParameter();
            if (values.Count == 4)
            {
                parameter.Name = values.GetLexem(0);
                parameter.Parameters = values.GetSpiceObject<ParameterCollection>(2);
            }
            else
            {
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        private SpiceObject CreateBracketParameterContent(ParseTreeNodeTranslationValues values)
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
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        private SpiceObject CreateAssigmentParametersContinue(ParseTreeNodeTranslationValues values)
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
                    throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        private SpiceObject CreateAssigmentParameters(ParseTreeNodeTranslationValues values)
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
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_SINGLE_SEQUENCE"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>
        /// </returns>
        private SpiceObject CreateSingleParametersContinue(ParseTreeNodeTranslationValues values)
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
                    throw new TranslationException("Error during translating parse tree to Spice Object Model");
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
        private SpiceObject CreateSingleParameters(ParseTreeNodeTranslationValues values)
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
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL_SINGLE"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="AssignmentParameter"/>
        /// </returns>
        private SpiceObject CreateAssigmentSimpleParameter(ParseTreeNodeTranslationValues values)
        {
            if (values.Count == 3)
            {
                var assigmentParameter = new AssignmentParameter();
                assigmentParameter.Name = values.GetLexem(0);

                var singleParameter = values.GetSpiceObject<SingleParameter>(2);

                //TODO: Refactor this....
                if (singleParameter is ExpressionParameter exp)
                {
                    assigmentParameter.Value = exp.Image.Substring(1, exp.Image.Length - 2);
                }
                else
                {
                    assigmentParameter.Value = singleParameter.Image;
                }

                return assigmentParameter;
            }
            else
            {
                throw new TranslationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.PARAMETER_EQUAL"/> parse tree node
        /// </summary>
        /// <returns>
        /// A instance of <see cref="AssignmentParameter"/>
        /// </returns>
        private SpiceObject CreateAssigmentParameter(ParseTreeNodeTranslationValues values)
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

                    //TODO: refactor
                    var valueParameter = values.GetSpiceObject<SingleParameter>(5);
                    if (valueParameter is ExpressionParameter exp)
                    {
                        assigmentParameter.Value = exp.Image.Substring(1, exp.Image.Length - 2);
                    }
                    else
                    {
                        assigmentParameter.Value = valueParameter.Image;
                    }

                    return assigmentParameter;
                }

                if (values.Count == 8)
                {
                    // v(2,3) = 4
                    var assigmentParameter = new AssignmentParameter();
                    assigmentParameter.Name = (values[0] as ParseTreeNodeTerminalTranslationValue).Token.Lexem;
                    assigmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(2).Image);
                    assigmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(4).Image);

                    //TODO: refactor
                    var valueParameter = values.GetSpiceObject<SingleParameter>(7);
                    if (valueParameter is ExpressionParameter exp)
                    {
                        assigmentParameter.Value = exp.Image.Substring(1, exp.Image.Length - 2);
                    }
                    else
                    {
                        assigmentParameter.Value = valueParameter.Image;
                    }

                    return assigmentParameter;
                }

                throw new TranslationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="Statements"/>
        /// from the values of children nodes of <see cref="SpiceGrammarSymbol.STATEMENTS"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Statements"/>
        /// </returns>
        private SpiceObject CreateStatements(ParseTreeNodeTranslationValues values)
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
                            throw new TranslationException("Error during translating parse tree to Spice Object Model");
                        }
                    }
                }
            }

            return statements;
        }
    }
}
