using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Parsers.Netlist.Spice.Internals
{
    /// <summary>
    /// Translates a parse tree (<see cref="ParseTreeNode"/> to SPICE object model.
    /// </summary>
    public class ParseTreeEvaluator
    {
        /// <summary>
        /// The dictionary with tree node values.
        /// </summary>
        private readonly Dictionary<ParseTreeNode, ParseTreeNodeEvaluationValue> _treeNodesValues = new Dictionary<ParseTreeNode, ParseTreeNodeEvaluationValue>();

        /// <summary>
        /// The dictionary with non-terminal nodes evaluators.
        /// </summary>
        private readonly Dictionary<string, Func<ParseTreeNodeEvaluationValues, SpiceObject>> _evaluators = new Dictionary<string, Func<ParseTreeNodeEvaluationValues, SpiceObject>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeEvaluator"/> class.
        /// </summary>
        public ParseTreeEvaluator()
        {
            _evaluators.Add(Symbols.Netlist, CreateNetlist);
            _evaluators.Add(Symbols.NetlistWithoutTitle, CreateNetlistWithoutTitle);
            _evaluators.Add(Symbols.NetlistEnding, (_) => null);
            _evaluators.Add(Symbols.Statements, CreateStatements);
            _evaluators.Add(Symbols.Statement, CreateStatement);
            _evaluators.Add(Symbols.Model, CreateModel);
            _evaluators.Add(Symbols.Distribution, CreateDistribution);
            _evaluators.Add(Symbols.Control, CreateControl);
            _evaluators.Add(Symbols.Component, CreateComponent);
            _evaluators.Add(Symbols.Parameters, CreateParameters);
            _evaluators.Add(Symbols.ParametersSeparator, (_) => CreateParametersSeparator());
            _evaluators.Add(Symbols.Parameter, CreateParameter);
            _evaluators.Add(Symbols.Vector, CreateVector);
            _evaluators.Add(Symbols.VectorContinue, CreateVectorContinue);
            _evaluators.Add(Symbols.ParameterBracket, CreateBracketParameter);
            _evaluators.Add(Symbols.ParameterBracketContent, CreateBracketParameterContent);
            _evaluators.Add(Symbols.ParameterEqual, CreateAssignmentParameter);
            _evaluators.Add(Symbols.ExpressionEqual, CreateExpressionEqualParameter);
            _evaluators.Add(Symbols.Point, CreatePointParameter);
            _evaluators.Add(Symbols.PointValue, CreatePointValue);
            _evaluators.Add(Symbols.PointValues, CreatePointValues);
            _evaluators.Add(Symbols.Points, CreatePoints);
            _evaluators.Add(Symbols.PointsContinue, CreatePointsContinue);
            _evaluators.Add(Symbols.ParameterEqualSingle, CreateAssignmentSimpleParameter);
            _evaluators.Add(Symbols.ParameterSingle, CreateParameterSingle);
            _evaluators.Add(Symbols.Subckt, CreateSubCircuit);
            _evaluators.Add(Symbols.SubcktEnding, (_) => null);
            _evaluators.Add(Symbols.ParallelEnding, (_) => null);
            _evaluators.Add(Symbols.Parallel, CreateParallel);
            _evaluators.Add(Symbols.CommentLine, CreateComment);
            _evaluators.Add(Symbols.NewLine, (_) => null);
            _evaluators.Add(Symbols.NewLines, (_) => null);
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
                    var items = new ParseTreeNodeEvaluationValues(nt.LineNumber, nt.StartColumnIndex, nt.EndColumnIndex, nt.FileName);

                    foreach (var child in nt.Children)
                    {
                        items.Add(_treeNodesValues[child]);
                    }

                    if (!_evaluators.ContainsKey(nt.Name))
                    {
                        throw new ParseTreeEvaluationException("Unsupported evaluation of parse tree node: " + nt.Name);
                    }

                    var treeNodeResult = _evaluators[nt.Name](items);
                    _treeNodesValues[treeNode] = new ParseTreeNonTerminalEvaluationValue
                    {
                        SpiceObject = treeNodeResult,
                        Node = treeNode,
                    };
                }
                else
                {
                    _treeNodesValues[treeNode] = new ParseTreeNodeTerminalEvaluationValue()
                    {
                        Node = treeNode,
                        Token = ((ParseTreeTerminalNode)treeNode).Token,
                    };
                }
            }

            if (_treeNodesValues[root] is ParseTreeNonTerminalEvaluationValue rootNt)
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
            var pointValues = new PointValues(new SpiceLineInfo(values));

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
            var values = nt.GetSpiceObject<PointValues>(1);
            var pointParameter = new PointParameter(values, new SpiceLineInfo(nt));
            return pointParameter;
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

        private SpiceObject CreatePointsContinue(ParseTreeNodeEvaluationValues values)
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
        /// A new instance of <see cref="SpiceNetlist"/>.
        /// </returns>
        private SpiceObject CreateNetlist(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count == 3)
            {
                return new SpiceNetlist(string.Empty, values.GetSpiceObject<Statements>(1));
            }
            else
            {
                if (values.Count == 1)
                {
                    return new SpiceNetlist(null, new Statements());
                }

                return new SpiceNetlist(values.GetLexem(0), values.Count >= 3 ? values.GetSpiceObject<Statements>(2) : new Statements());
            }
        }

        /// <summary>
        /// Returns new instance of <see cref="SpiceNetlist"/>
        /// from the values of children nodes of <see cref="Symbols.Netlist"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SpiceNetlist"/>.
        /// </returns>
        private SpiceObject CreateNetlistWithoutTitle(ParseTreeNodeEvaluationValues values)
        {
            return new SpiceNetlist(null, values.GetSpiceObject<Statements>(0));
        }

        /// <summary>
        /// Returns new instance of <see cref="SingleParameter"/>
        /// or <see cref="BracketParameter"/>
        /// or <see cref="AssignmentParameter"/>
        /// or <see cref="VectorParameter"/>
        /// from the values of children nodes of <see cref="Symbols.Parameter"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SingleParameter"/>.
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
        /// from the values of children nodes of <see cref="Symbols.ParameterSingle"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="SingleParameter"/>.
        /// </returns>
        private SpiceObject CreateParameterSingle(ParseTreeNodeEvaluationValues values)
        {
            if (values[0] is ParseTreeNodeTerminalEvaluationValue t)
            {
                var lexemValue = t.Token.Lexem;
                switch (t.Token.SpiceTokenType)
                {
                    case SpiceTokenType.REFERENCE:
                        return new ReferenceParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.DOUBLE_QUOTED_STRING:
                        return new StringParameter(lexemValue.Trim('"'), new SpiceLineInfo(t.Token));

                    case SpiceTokenType.SINGLE_QUOTED_STRING:
                        return new StringParameter(lexemValue.Trim('\''), new SpiceLineInfo(t.Token));

                    case SpiceTokenType.VALUE:
                        return new ValueParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.WORD:
                        return new WordParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.SUFFIX:
                        return new SuffixParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.PREFIX_SINGLE:
                    case SpiceTokenType.PREFIX_COMPLEX:
                        return new PrefixParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.IDENTIFIER:
                        return new IdentifierParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.EXPRESSION:
                        return new ExpressionParameter(lexemValue, new SpiceLineInfo(t.Token));

                    case SpiceTokenType.EXPRESSION_BRACKET:
                        return new ExpressionParameter(lexemValue.Replace("}", string.Empty).Replace("{", string.Empty), new SpiceLineInfo(t.Token));

                    case SpiceTokenType.EXPRESSION_SINGLE_QUOTES:
                        return new ExpressionParameter(lexemValue.Trim('\''), new SpiceLineInfo(t.Token));

                    case SpiceTokenType.PERCENT:
                        return new PercentParameter(lexemValue.TrimEnd('%'), new SpiceLineInfo(t.Token));
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

            var component = new Component(
                values.GetLexem(0),
                values.GetSpiceObject<ParameterCollection>(1),
                new SpiceLineInfo(values));

            switch ((SpiceTokenType)values.GetToken(0).Type)
            {
                case SpiceTokenType.SUFFIX:
                    component.NameParameter = new SuffixParameter(values.GetLexem(0));
                    break;
                default:
                    component.NameParameter = new WordParameter(values.GetLexem(0));
                    break;
            }

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
            Control control;
            SpiceLineInfo lineInfo = new SpiceLineInfo(values);

            switch (values.GetLexem(0).ToLower())
            {
                case ".endl":
                    control = new Control("endl", new ParameterCollection(), lineInfo);
                    break;

                case ".if":
                    control = new Control(
                        "if",
                        new ParameterCollection(
                            new List<Parameter>()
                            {
                                new ExpressionParameter(values.GetLexem(1), lineInfo),
                            }),
                        lineInfo);

                    break;

                case ".elseif":
                    control = new Control(
                        "elseif",
                        new ParameterCollection(
                            new List<Parameter>()
                            {
                                new ExpressionParameter(values.GetLexem(1), lineInfo),
                            }),
                        lineInfo);
                    break;

                case ".else":
                    control = new Control("else", new ParameterCollection(new List<Parameter>()), lineInfo);
                    break;

                case ".endif":
                    control = new Control("endif", new ParameterCollection(new List<Parameter>()), lineInfo);
                    break;

                default:
                    control = new Control(values.GetLexem(1), values.GetSpiceObject<ParameterCollection>(2), lineInfo);
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

            var subCkt = new SubCircuit(values.GetLexem(2), new Statements(), new ParameterCollection(), new SpiceLineInfo(values));

            var allParameters = values.GetSpiceObject<ParameterCollection>(3);

            // Parse nodes and parameters
            bool mode = true; // true = nodes, false = parameters
            foreach (var parameter in allParameters)
            {
                if (mode)
                {
                    // After this, only parameters will follow
                    if (parameter is SingleParameter s && s.Value.ToLower() == "params:")
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
                            || s2 is PrefixParameter
                            || s2 is SuffixParameter
                            || int.TryParse(s2.Value, out _))
                        {
                            subCkt.Pins.Add(s2);
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
        /// Returns new instance of <see cref="Parallel"/>
        /// from the values of children nodes of <see cref="Symbols.Parallel"/> parse tree node.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="Parallel"/>.
        /// </returns>
        private SpiceObject CreateParallel(ParseTreeNodeEvaluationValues values)
        {
            if (values.Count == 5)
            {
                var parallel = new Parallel(string.Empty, new Statements(), new SpiceLineInfo(values));
                parallel.Statements = values.GetSpiceObject<Statements>(3);
                return parallel;
            }
            else
            {
                var parallel = new Parallel(values.GetLexem(2), new Statements(), new SpiceLineInfo(values));
                parallel.Statements = values.GetSpiceObject<Statements>(4);
                return parallel;
            }
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
            var comment = new CommentLine(values.GetLexem(0), new SpiceLineInfo(values));
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
            var model = new Model(values.GetLexem(1), values.GetSpiceObject<ParameterCollection>(2), new SpiceLineInfo(values));
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
            var lineInfo = new SpiceLineInfo(values);
            var model = new Control("DISTRIBUTION", values.GetSpiceObject<ParameterCollection>(3), lineInfo);
            model.Parameters.Insert(0, new StringParameter(values.GetLexem(2), new SpiceLineInfo(values.GetToken(2))));
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
            if (values.Count == 4)
            {
                var parameter = new BracketParameter(
                    values.GetLexem(0),
                    values.GetSpiceObject<ParameterCollection>(2),
                    new SpiceLineInfo(values));

                return parameter;
            }
            else
            {
                throw new ParseTreeEvaluationException("Error during translating parse tree to Spice Object Model");
            }
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

                var assignmentParameter = new AssignmentParameter(
                    values.GetLexem(0),
                    new List<string>(),
                    new List<string>() { singleParameter.Value },
                    false,
                    singleParameter.LineInfo);

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
                return new ExpressionEqualParameter(
                    values.GetLexem(0).Trim('{', '}'),
                    values.GetSpiceObject<Points>(1),
                    new SpiceLineInfo(values));
            }
            else if (values.Count == 3)
            {
                return new ExpressionEqualParameter(
                    values.GetLexem(0).Trim('{', '}'),
                    values.GetSpiceObject<Points>(2),
                    new SpiceLineInfo(values));
            }
            else
            {
                throw new ParseTreeEvaluationException("Error during translating assignment parameter to Spice Object Model");
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
                var assignmentParameter = new AssignmentParameter(
                    values.GetLexem(0),
                    new List<string>(),
                    new List<string>(),
                    false,
                    new SpiceLineInfo(values));

                if (values.Count == 6)
                {
                    var arguments = values.GetSpiceObject<SpiceObject>(2);
                    if (arguments is VectorParameter vp)
                    {
                        foreach (SingleParameter parameter in vp.Elements)
                        {
                            assignmentParameter.Arguments.Add(parameter.Value);
                        }

                        assignmentParameter.HasFunctionSyntax = true;
                    }
                    else
                    {
                        assignmentParameter.Arguments.Add(values.GetSpiceObject<SingleParameter>(2).Value);
                        assignmentParameter.HasFunctionSyntax = true;
                    }

                    var valueParameter = values.GetSpiceObject<SingleParameter>(5);
                    assignmentParameter.Values = new List<string>() { valueParameter.Value };
                    return assignmentParameter;
                }
                else if (values.Count == 5)
                {
                    assignmentParameter.HasFunctionSyntax = true;
                    var valueParameter = values.GetSpiceObject<SingleParameter>(4);
                    assignmentParameter.Values = new List<string>() { valueParameter.Value };
                    return assignmentParameter;
                }
                else if (values.Count == 3)
                {
                    var valueParameter = values.GetSpiceObject<VectorParameter>(2);
                    assignmentParameter.Values = valueParameter.Elements.Select(e => e.Value).ToList();
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