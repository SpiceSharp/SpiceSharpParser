using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.Netlist.Spice.Internals;

namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// Translates a parse tree (<see cref="ParseTreeNode"/> to SPICE object model.
    /// </summary>
    public class ParseTreeEvaluator
    {
        /// <summary>
        /// The dictionary with tree node values.
        /// </summary>
        private readonly Dictionary<ParseTreeNode, ParseTreeNodeEvaluationValue> treeNodesValues = new Dictionary<ParseTreeNode, ParseTreeNodeEvaluationValue>();

        /// <summary>
        /// The dictionary with non-terminal nodes evaluators.
        /// </summary>
        private readonly Dictionary<string, Func<ParseTreeNodeEvaluationValues, SpiceObject>> evaluators = new Dictionary<string, Func<ParseTreeNodeEvaluationValues, SpiceObject>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeEvaluator"/> class.
        /// </summary>
        public ParseTreeEvaluator()
        {
            evaluators.Add(Symbols.Netlist, (ParseTreeNodeEvaluationValues nt) => CreateNetlist(nt));
            evaluators.Add(Symbols.NetlistWithoutTitle, (ParseTreeNodeEvaluationValues nt) => CreateNetlistWithoutTitle(nt));
            evaluators.Add(Symbols.NetlistEnding, (ParseTreeNodeEvaluationValues nt) => null);
            evaluators.Add(Symbols.Statements, (ParseTreeNodeEvaluationValues nt) => CreateStatements(nt));
            evaluators.Add(Symbols.Statement, (ParseTreeNodeEvaluationValues nt) => CreateStatement(nt));
            evaluators.Add(Symbols.Model, (ParseTreeNodeEvaluationValues nt) => CreateModel(nt));
            evaluators.Add(Symbols.Distribution, (ParseTreeNodeEvaluationValues nt) => CreateDistribution(nt));
            evaluators.Add(Symbols.Control, (ParseTreeNodeEvaluationValues nt) => CreateControl(nt));
            evaluators.Add(Symbols.Component, (ParseTreeNodeEvaluationValues nt) => CreateComponent(nt));
            evaluators.Add(Symbols.Parameters, (ParseTreeNodeEvaluationValues nt) => CreateParameters(nt));
            evaluators.Add(Symbols.ParametersSeparator, (ParseTreeNodeEvaluationValues nt) => CreateParametersSeparator());
            evaluators.Add(Symbols.Parameter, (ParseTreeNodeEvaluationValues nt) => CreateParameter(nt));
            evaluators.Add(Symbols.Vector, (ParseTreeNodeEvaluationValues nt) => CreateVector(nt));
            evaluators.Add(Symbols.VectorContinue, (ParseTreeNodeEvaluationValues nt) => CreateVectorContinue(nt));
            evaluators.Add(Symbols.ParameterBracket, (ParseTreeNodeEvaluationValues nt) => CreateBracketParameter(nt));
            evaluators.Add(Symbols.ParameterBracketContent, (ParseTreeNodeEvaluationValues nt) => CreateBracketParameterContent(nt));
            evaluators.Add(Symbols.ParameterEqual, (ParseTreeNodeEvaluationValues nt) => CreateAssignmentParameter(nt));
            evaluators.Add(Symbols.ExpressionEqual, (ParseTreeNodeEvaluationValues nt) => CreateExpressionEqualParameter(nt));
            evaluators.Add(Symbols.Point, (ParseTreeNodeEvaluationValues nt) => CreatePointParameter(nt));
            evaluators.Add(Symbols.PointValue, (ParseTreeNodeEvaluationValues nt) => CreatePointValue(nt));
            evaluators.Add(Symbols.PointValues, (ParseTreeNodeEvaluationValues nt) => CreatePointValues(nt));
            evaluators.Add(Symbols.Points, (ParseTreeNodeEvaluationValues nt) => CreatePoints(nt));
            evaluators.Add(Symbols.PointsContinue, (ParseTreeNodeEvaluationValues nt) => CreatPointsContinue(nt));
            evaluators.Add(Symbols.ParameterEqualSingle, (ParseTreeNodeEvaluationValues nt) => CreateAssignmentSimpleParameter(nt));
            evaluators.Add(Symbols.ParameterSingle, (ParseTreeNodeEvaluationValues nt) => CreateParameterSingle(nt));
            evaluators.Add(Symbols.Subckt, (ParseTreeNodeEvaluationValues nt) => CreateSubCircuit(nt));
            evaluators.Add(Symbols.SubcktEnding, (ParseTreeNodeEvaluationValues nt) => null);
            evaluators.Add(Symbols.CommentLine, (ParseTreeNodeEvaluationValues nt) => CreateComment(nt));
            evaluators.Add(Symbols.NewLine, (ParseTreeNodeEvaluationValues nt) => null);
            evaluators.Add(Symbols.NewLines, (ParseTreeNodeEvaluationValues nt) => null);
        }

        /// <summary>
        /// Translates a SPICE parse tree to a context.
        /// </summary>
        /// <param name="root">A parse tree root.</param>
        /// <returns>A netlist.</returns>
        public SpiceObject Evaluate(ParseTreeNode root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var traversal = new ParseTreeTraversal();

            // Get tree nodes in post order
            var treeNodes = traversal.GetIterativePostOrder(root);

            // Iterate over tree nodes
            foreach (var treeNode in treeNodes)
            {
                if (treeNode is ParseTreeNonTerminalNode nt)
                {
                    var items = new ParseTreeNodeEvaluationValues();

                    foreach (var child in nt.Children)
                    {
                        items.Add(treeNodesValues[child]);
                    }

                    if (!evaluators.ContainsKey(nt.Name))
                    {
                        throw new ParseTreeEvaluationException("Unsupported evaluation of parse tree node: " + nt.Name);
                    }

                    var treeNodeResult = evaluators[nt.Name](items);
                    treeNodesValues[treeNode] = new ParseTreeNonTerminalEvaluationValue
                                                    {
                                                        SpiceObject = treeNodeResult,
                                                        Node = treeNode,
                                                    };
                }
                else
                {
                    treeNodesValues[treeNode] = new ParseTreeNodeTerminalEvaluationValue()
                                                    {
                                                        Node = treeNode,
                                                        Token = ((ParseTreeTerminalNode)treeNode).Token,
                                                    };
                }
            }

            if (treeNodesValues[root] is ParseTreeNonTerminalEvaluationValue rootNt)
            {
                return rootNt.SpiceObject;
            }
            else
            {
                return null;
            }
        }

        private SpiceObject CreatePointValues(ParseTreeNodeEvaluationValues values)
        {
            var pointValues = new PointValues();
            if (values.Count == 3)
            {
                pointValues.Items.Add(values.GetSpiceObject<SingleParameter>(0));
                pointValues.Items.AddRange(values.GetSpiceObject<PointValues>(2).Items);
            }
            else
            {
                if (values.Count == 2)
                {
                    pointValues.Items.Add(values.GetSpiceObject<SingleParameter>(0));
                    pointValues.Items.AddRange(values.GetSpiceObject<PointValues>(1).Items);
                }
                else
                {
                    pointValues.Items.Add(values.GetSpiceObject<SingleParameter>(0));
                }
            }

            return pointValues;
        }

        private SpiceObject CreatePointValue(ParseTreeNodeEvaluationValues nt)
        {
            return (nt[0] as ParseTreeNonTerminalEvaluationValue)?.SpiceObject;
        }

        private SpiceObject CreatePointParameter(ParseTreeNodeEvaluationValues nt)
        {
            return new PointParameter() { Values = nt.GetSpiceObject<PointValues>(1), };
        }

        private SpiceObject CreatePoints(ParseTreeNodeEvaluationValues values)
        {
            var points = new Points();

            if (values.Count == 2)
            {
                points.Values.Add(values.GetSpiceObject<PointParameter>(0));
                points.Values.AddRange(values.GetSpiceObject<Points>(1).Values);
            }

            if (values.Count == 4)
            {
                points.Values.Add(values.GetSpiceObject<PointParameter>(1));
                points.Values.AddRange(values.GetSpiceObject<Points>(2).Values);
            }

            return points;
        }

        private SpiceObject CreatPointsContinue(ParseTreeNodeEvaluationValues values)
        {
            var points = new Points();

            if (values.Count == 2)
            {
                points.Values.Add(values.GetSpiceObject<PointParameter>(0));
                points.Values.AddRange(values.GetSpiceObject<Points>(1).Values);
            }

            if (values.Count == 3)
            {
                points.Values.Add(values.GetSpiceObject<PointParameter>(1));
                points.Values.AddRange(values.GetSpiceObject<Points>(2).Values);
            }

            if (values.Count == 4)
            {
                points.Values.Add(values.GetSpiceObject<PointParameter>(2));
                points.Values.AddRange(values.GetSpiceObject<Points>(3).Values);
            }

            return points;
        }

        /// <summary>
        /// Returns new instance of <see cref="SpiceNetlist"/>
        /// from the values of children nodes of <see cref="Symbols.Netlist"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SpiceNetlist"/>
        /// </returns>
        private SpiceObject CreateNetlist(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count == 3)
            {
                return new SpiceNetlist()
                {
                    Title = string.Empty,
                    Statements = values.GetSpiceObject<Statements>(1),
                };
            }
            else
            {
                if (values.Count == 1)
                {
                    return new SpiceNetlist()
                    {
                        Title = null,
                        Statements = new Statements(),
                    };
                }

                return new SpiceNetlist()
                {
                    Title = values.GetLexem(0),
                    Statements = values.Count >= 3 ? values.GetSpiceObject<Statements>(2) : new Statements(),
                };
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="SpiceNetlist"/>
        /// from the values of children nodes of <see cref="Symbols.Netlist"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SpiceNetlist"/>
        /// </returns>
        private SpiceObject CreateNetlistWithoutTitle(ParseTreeNodeEvaluationValues values)
        {
            return new SpiceNetlist()
            {
                Title = null,
                Statements = values.GetSpiceObject<Statements>(0),
            };
        }

        /// <summary>
        /// Returns new instance of <see cref="SingleParameter"/>
        /// or <see cref="BracketParameter"/>
        /// or <see cref="AssignmentParameter"/>
        /// or <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="Symbols.Parameter"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SingleParameter"/>
        /// </returns>
        private SpiceObject CreateParameter(ParseTreeNodeEvaluationValues values)
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

                if (values.TryToGetSpiceObject(0, out ExpressionEqualParameter eep))
                {
                    return eep;
                }

                if (values.TryToGetSpiceObject(0, out PointParameter pp))
                {
                    return pp;
                }
            }

            throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
        }

        /// <summary>
        /// Returns new instance of <see cref="ReferenceParameter"/>
        /// or <see cref="ValueParameter"/> or <see cref="WordParameter"/>
        /// or <see cref="ExpressionParameter"/> or <see cref="IdentifierParameter"/>
        /// from the values of children nodes of <see cref="Symbols.ParameterSingle"/> parse tree node
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SingleParameter"/>
        /// </returns>
        private SpiceObject CreateParameterSingle(ParseTreeNodeEvaluationValues values)
        {
            if (values[0] is ParseTreeNodeTerminalEvaluationValue t)
            {
                var lexemValue = t.Token.Lexem;
                switch (t.Token.SpiceTokenType)
                {
                    case SpiceTokenType.REFERENCE:
                        return new ReferenceParameter(lexemValue) {LineNumber = t.Token.LineNumber};
                    case SpiceTokenType.DOUBLE_QUOTED_STRING:
                        return new StringParameter(lexemValue.Trim('"')) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.SINGLE_QUOTED_STRING:
                        return new StringParameter(lexemValue.Trim('\'')) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.VALUE:
                        return new ValueParameter(lexemValue) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.WORD:
                        return new WordParameter(lexemValue) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.IDENTIFIER:
                        return new IdentifierParameter(lexemValue) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.EXPRESSION_BRACKET:
                        return new ExpressionParameter(lexemValue.Trim('{', '}')) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.EXPRESSION_SINGLE_QUOTES:
                        return new ExpressionParameter(lexemValue.Trim('\'')) { LineNumber = t.Token.LineNumber };
                    case SpiceTokenType.PERCENT:
                        return new PercentParameter(lexemValue.TrimEnd('%')) { LineNumber = t.Token.LineNumber };
                }
            }

            throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="Symbols.Parameters"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>.
        /// </returns>
        private SpiceObject CreateParameters(ParseTreeNodeEvaluationValues values)
        {
            var parameters = new ParameterCollection();

            if (values.Count == 3)
            {
                parameters.Add(values.GetSpiceObject<Parameter>(0));
                parameters.Merge(values.GetSpiceObject<ParameterCollection>(2));
            }

            return parameters;
        }

        private SpiceObject CreateParametersSeparator()
        {
            var parameters = new ParameterCollection();
            return parameters;
        }

        /// <summary>
        /// Creates an instance of <see cref="Component"/>
        /// from the values of children nodes of <see cref="Symbols.Component"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Component"/>.
        /// </returns>
        private SpiceObject CreateComponent(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count != 2 && values.Count != 3)
            {
                throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
            }

            var component = new Component();
            component.Name = values.GetLexem(0);
            component.PinsAndParameters = values.GetSpiceObject<ParameterCollection>(1);
            component.LineNumber = values.GetLexemLineNumber(0);
            return component;
        }

        /// <summary>
        /// Returns new instance of <see cref="Control"/>
        /// from the values of children nodes of <see cref="Symbols.Control"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Control"/>.
        /// </returns>
        private SpiceObject CreateControl(ParseTreeNodeEvaluationValues values)
        {
            var control = new Control();

            switch (values.GetLexem(0).ToLower())
            {
                case ".endl":
                    control.Name = "endl";
                    control.Parameters = new ParameterCollection(); // TODO: fix it, endl can have a parameter
                    control.LineNumber = values.GetLexemLineNumber(0);
                    break;
                case ".if":
                    control.Name = "if";
                    control.Parameters = new ParameterCollection() { new ExpressionParameter(values.GetLexem(1)) };
                    control.LineNumber = values.GetLexemLineNumber(0);
                    break;
                case ".elseif":
                    control.Name = "elseif";
                    control.Parameters = new ParameterCollection() { new ExpressionParameter(values.GetLexem(1)) };
                    control.LineNumber = values.GetLexemLineNumber(0);
                    break;
                case ".else":
                    control.Name = "else";
                    control.Parameters = new ParameterCollection();
                    control.LineNumber = values.GetLexemLineNumber(0);
                    break;
                case ".endif":
                    control.Name = "endif";
                    control.Parameters = new ParameterCollection();
                    control.LineNumber = values.GetLexemLineNumber(0);
                    break;
                default:
                    control.Name = values.GetLexem(1);
                    control.Parameters = values.GetSpiceObject<ParameterCollection>(2);
                    control.LineNumber = values.GetLexemLineNumber(1);
                    break;
            }

            return control;
        }

        /// <summary>
        /// Returns new instance of <see cref="SubCircuit"/>
        /// from the values of children nodes of <see cref="Symbols.Subckt"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SubCircuit"/>.
        /// </returns>
        private SpiceObject CreateSubCircuit(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count < 3)
            {
                throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
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

            var lastValue = (ParseTreeNonTerminalEvaluationValue)values[values.Count - 1];

            if (lastValue != null && (((ParseTreeNonTerminalNode)lastValue.Node).Children.Count == 2))
            {
                var nameAfterEnds = (ParseTreeTerminalNode)((ParseTreeNonTerminalNode)lastValue.Node).Children[1];

                if (nameAfterEnds.Token.Lexem != subCkt.Name)
                {
                    throw new ParseException("There is wrong name after .ENDS", nameAfterEnds.Token.LineNumber);
                }
            }

            return subCkt;
        }

        /// <summary>
        /// Returns new instance of <see cref="CommentLine"/>
        /// from the values of children nodes of <see cref="Symbols.CommentLine"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="CommentLine"/>.
        /// </returns>
        private SpiceObject CreateComment(ParseTreeNodeEvaluationValues values)
        {
            var comment = new CommentLine();
            comment.Text = values.GetLexem(0);
            comment.LineNumber = values.GetLexemLineNumber(0);
            return comment;
        }

        /// <summary>
        /// Returns new instance of <see cref="Statement"/>
        /// from the values of children nodes of <see cref="Symbols.Statement"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A instance of <see cref="Statement"/>.
        /// </returns>
        private SpiceObject CreateStatement(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count != 3 && values.Count != 2)
            {
                throw new ParseTreeEvaluationException("Error during translating statement - Wrong elements count for statement");
            }

            if (!(values[values.Count - 1] is ParseTreeNodeTerminalEvaluationValue tv && tv.Token.Is(SpiceTokenType.NEWLINE)))
            {
                throw new ParseTreeEvaluationException("Error during translating statement - Statement is not finished by newline");
            }

            if (values.Count == 3 && values[1] is ParseTreeNonTerminalEvaluationValue nv && nv.SpiceObject != null && !(nv.SpiceObject is CommentLine))
            {
                throw new ParseTreeEvaluationException("Error during translating statement - Statement has second element that is not comment");
            }

            var statement = values.GetSpiceObject<Statement>(0);

            return statement;
        }

        /// <summary>
        /// Returns new instance of <see cref="Model"/>
        /// from the values of children nodes of <see cref="Symbols.Model"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Model"/>.
        /// </returns>
        private SpiceObject CreateModel(ParseTreeNodeEvaluationValues values)
        {
            var model = new Model();
            model.Name = values.GetLexem(1);
            model.Parameters = values.GetSpiceObject<ParameterCollection>(2);
            model.LineNumber = values.GetLexemLineNumber(1);
            return model;
        }

        /// <summary>
        /// Returns new instance of <see cref="Model"/>
        /// from the values of children nodes of <see cref="Symbols.Model"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Model"/>.
        /// </returns>
        private SpiceObject CreateDistribution(ParseTreeNodeEvaluationValues values)
        {
            var model = new Control();
            model.Name = "DISTRIBUTION";
            model.Parameters = values.GetSpiceObject<ParameterCollection>(3);
            model.Parameters.Insert(0, new StringParameter(values.GetLexem(2)));
            model.LineNumber = values.GetLexemLineNumber(2);
            return model;
        }


        /// <summary>
        /// Returns new instance of <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="Symbols.Vector"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="VectorParameter"/>.
        /// </returns>
        private SpiceObject CreateVector(ParseTreeNodeEvaluationValues values)
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
        /// from the values of children nodes of <see cref="Symbols.VectorContinue"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="VectorParameter"/>.
        /// </returns>
        private SpiceObject CreateVectorContinue(ParseTreeNodeEvaluationValues values)
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
        /// from the values of children nodes of <see cref="Symbols.ParameterBracket"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="BracketParameter"/>.
        /// </returns>
        private SpiceObject CreateBracketParameter(ParseTreeNodeEvaluationValues values)
        {
            var parameter = new BracketParameter();
            if (values.Count == 4)
            {
                parameter.Name = values.GetLexem(0);
                parameter.Parameters = values.GetSpiceObject<ParameterCollection>(2);
            }
            else
            {
                throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
            }

            return parameter;
        }

        /// <summary>
        /// Returns new instance of <see cref="ParameterCollection"/>
        /// from the values of children nodes of <see cref="Symbols.ParameterBracketContent"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="ParameterCollection"/>.
        /// </returns>
        private SpiceObject CreateBracketParameterContent(ParseTreeNodeEvaluationValues values)
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
                throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
            }

            return parameters;
        }

        /// <summary>
        /// Returns new instance of <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="Symbols.ParameterEqualSingle"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="AssignmentParameter"/>.
        /// </returns>
        private SpiceObject CreateAssignmentSimpleParameter(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count == 3)
            {
                var singleParameter = values.GetSpiceObject<SingleParameter>(2);

                var assignmentParameter = new AssignmentParameter
                {
                    Name = values.GetLexem(0),
                    LineNumber = singleParameter.LineNumber,
                    Values = new List<string>() {singleParameter.Image}
                };
                return assignmentParameter;
            }
            else
            {
                throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="ExpressionEqualParameter"/>
        /// from the values of children nodes of <see cref="Symbols.ExpressionEqual"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A instance of <see cref="ExpressionEqualParameter"/>.
        /// </returns>
        private SpiceObject CreateExpressionEqualParameter(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count == 2)
            {
                return new ExpressionEqualParameter()
                {
                    Expression = values.GetLexem(0).Trim('{', '}'),
                    Points = values.GetSpiceObject<Points>(1),
                };
            }
            else if (values.Count == 3)
            {
                return new ExpressionEqualParameter()
                {
                    Expression = values.GetLexem(0).Trim('{', '}'),
                    Points = values.GetSpiceObject<Points>(2),
                };
            }
            else
            {
                throw new ParseTreeEvaluationException("Error during translating assigment parameter to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="AssignmentParameter"/>
        /// from the values of children nodes of <see cref="Symbols.ParameterEqual"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A instance of <see cref="AssignmentParameter"/>.
        /// </returns>
        private SpiceObject CreateAssignmentParameter(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count == 1)
            {
                return values.GetSpiceObject<AssignmentParameter>(0);
            }
            else
            {
                var assignmentParameter = new AssignmentParameter();
                assignmentParameter.Name = values.GetLexem(0);
                assignmentParameter.LineNumber = values.GetLexemLineNumber(0);

                if (values.Count == 6)
                {
                    var arguments = values.GetSpiceObject<SpiceObject>(2);
                    if (arguments is VectorParameter vp)
                    {
                        foreach (SingleParameter parameter in vp.Elements)
                        {
                            assignmentParameter.Arguments.Add(parameter.Image);
                        }

                        assignmentParameter.HasFunctionSyntax = true;
                    }
                    else
                    {
                        assignmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(2).Image);
                        assignmentParameter.HasFunctionSyntax = true;
                    }

                    var valueParameter = values.GetSpiceObject<SingleParameter>(5);
                    assignmentParameter.Values = new List<string>() { valueParameter.Image };
                    return assignmentParameter;
                }
                else if (values.Count == 5)
                {
                    assignmentParameter.HasFunctionSyntax = true;
                    var valueParameter = values.GetSpiceObject<SingleParameter>(4);
                    assignmentParameter.Values = new List<string>() { valueParameter.Image };
                    return assignmentParameter;
                }
                else if (values.Count == 3)
                {
                    var valueParameter = values.GetSpiceObject<VectorParameter>(2);
                    assignmentParameter.Values = valueParameter.Elements.Select(e => e.Image).ToList();
                    return assignmentParameter;
                }

                throw new ParseTreeEvaluationException("Error during translating assignment parameter to Spice Object Model");
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="Statements"/>
        /// from the values of children nodes of <see cref="Symbols.Statements"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Statements"/>.
        /// </returns>
        private SpiceObject CreateStatements(ParseTreeNodeEvaluationValues values)
        {
            var statements = new Statements();

            if (values.Count == 2)
            {
                if (values.TryToGetSpiceObject(0, out Statement st))
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
                            throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
                        }
                    }
                }
            }

            return statements;
        }
    }
}
