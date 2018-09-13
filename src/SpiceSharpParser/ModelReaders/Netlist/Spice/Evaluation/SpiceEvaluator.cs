using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    /// <summary>
    /// Spice expressions evaluator.
    /// </summary>
    public class SpiceEvaluator : Evaluator, ISpiceEvaluator
    {
        public SpiceEvaluator(int? randomSeed = null)
        : this(string.Empty, null, SpiceEvaluatorMode.Spice3f5, randomSeed, new ExpressionRegistry())
        {
        }

        public SpiceEvaluator(string name, int? randomSeed = null)
         : this(name, null, SpiceEvaluatorMode.Spice3f5, randomSeed, new ExpressionRegistry())
        {
        }

        public SpiceEvaluator(string name, SpiceEvaluatorMode mode, int? randomSeed = null)
         : this(name, null, mode, randomSeed, new ExpressionRegistry())
        {
        }

        public SpiceEvaluator(SpiceEvaluatorMode mode, int? randomSeed = null)
        : this(string.Empty, null, mode, randomSeed, new ExpressionRegistry())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(string name, object context, SpiceEvaluatorMode mode, int? randomSeed, ExpressionRegistry registry)
            : base(name, context, new SpiceExpressionParser(mode == SpiceEvaluatorMode.LtSpice), registry, randomSeed)
        {
            Mode = mode;
            Parameters.Add("TEMP", new CachedEvaluatorExpression((e, a, ev) => (Circuit.ReferenceTemperature - Circuit.CelsiusKelvin), this));
            CreateCustomFunctions(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(string name, object context, SpiceEvaluatorMode mode, int? randomSeed, IExporterRegistry exporters, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator)
            : this(name, context, mode, randomSeed, new ExpressionRegistry())
        {
            ExportFunctions.Add(CustomFunctions, exporters, nodeNameGenerator, objectNameGenerator);
        }

        /// <summary>
        /// Gets the mode of evaluator.
        /// </summary>
        public SpiceEvaluatorMode Mode { get; }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A child evaluator.
        /// </returns>
        public override IEvaluator CreateChildEvaluator(string name, object context)
        {
            var newEvaluator = new SpiceEvaluator(name, context, Mode, Seed, new ExpressionRegistry());

            foreach (var parameterName in this.GetParameterNames())
            {
                newEvaluator.Parameters[parameterName] = this.ExpressionParser.Parameters[parameterName];
            }

            foreach (var customFunction in CustomFunctions)
            {
                newEvaluator.CustomFunctions[customFunction.Key] = customFunction.Value;
            }

            Children.Add(newEvaluator);

            return newEvaluator;
        }

        /// <summary>
        /// Creates a cloned evaluator.
        /// </summary>
        /// <returns>
        /// A cloned evaluator.
        /// </returns>
        public override IEvaluator CreateClonedEvaluator(string name, object context, int? randomSeed = null)
        {
            var registry = Registry.Clone();
            registry.Invalidate();

            var newEvaluator = new SpiceEvaluator(name, context, Mode, randomSeed, registry);
            registry.UpdateEvaluator(newEvaluator);

            foreach (var parameterName in this.GetParameterNames())
            {
                newEvaluator.Parameters[parameterName] = this.ExpressionParser.Parameters[parameterName].Clone();
                newEvaluator.Parameters[parameterName].Evaluator = newEvaluator;
                newEvaluator.Parameters[parameterName].Invalidate();
            }

            foreach (var customFunction in CustomFunctions)
            {
                if (!newEvaluator.CustomFunctions.ContainsKey(customFunction.Key))
                {
                    newEvaluator.CustomFunctions[customFunction.Key] = customFunction.Value;
                }
            }

            foreach (var child in Children)
            {
                newEvaluator.Children.Add(child.CreateClonedEvaluator(child.Name, context, randomSeed));
            }

            return newEvaluator;
        }

        private void CreateCustomFunctions(SpiceEvaluator evaluator)
        {
            evaluator.CustomFunctions.Clear();
            evaluator.CustomFunctions.Add("**", MathFunctions.CreatePowInfix(Mode));
            evaluator.CustomFunctions.Add("abs", MathFunctions.CreateAbs());
            evaluator.CustomFunctions.Add("buf", MathFunctions.CreateBuf());
            evaluator.CustomFunctions.Add("cbrt", MathFunctions.CreateCbrt());
            evaluator.CustomFunctions.Add("ceil", MathFunctions.CreateCeil());
            evaluator.CustomFunctions.Add("db", MathFunctions.CreateDb(Mode));
            evaluator.CustomFunctions.Add("def", ControlFunctions.CreateDef());
            evaluator.CustomFunctions.Add("exp", MathFunctions.CreateExp());
            evaluator.CustomFunctions.Add("fabs", MathFunctions.CreateAbs());
            evaluator.CustomFunctions.Add("flat", RandomFunctions.CreateFlat());
            evaluator.CustomFunctions.Add("floor", MathFunctions.CreateFloor());
            evaluator.CustomFunctions.Add("gauss", RandomFunctions.CreateGauss());
            evaluator.CustomFunctions.Add("hypot", MathFunctions.CreateHypot());
            evaluator.CustomFunctions.Add("if", ControlFunctions.CreateIf());
            evaluator.CustomFunctions.Add("lazy", ControlFunctions.CreateLazy());
            evaluator.CustomFunctions.Add("int", MathFunctions.CreateInt());
            evaluator.CustomFunctions.Add("inv", MathFunctions.CreateInv());
            evaluator.CustomFunctions.Add("ln", MathFunctions.CreateLn());
            evaluator.CustomFunctions.Add("limit", MathFunctions.CreateLimit());
            evaluator.CustomFunctions.Add("log", MathFunctions.CreateLog(Mode));
            evaluator.CustomFunctions.Add("log10", MathFunctions.CreateLog10(Mode));
            evaluator.CustomFunctions.Add("max", MathFunctions.CreateMax());
            evaluator.CustomFunctions.Add("min", MathFunctions.CreateMin());
            evaluator.CustomFunctions.Add("nint", MathFunctions.CreateRound());
            evaluator.CustomFunctions.Add("pow", MathFunctions.CreatePow(Mode));
            evaluator.CustomFunctions.Add("pwr", MathFunctions.CreatePwr(Mode));
            evaluator.CustomFunctions.Add("pwrs", MathFunctions.CreatePwrs());
            evaluator.CustomFunctions.Add("random", RandomFunctions.CreateRandom());
            evaluator.CustomFunctions.Add("round", MathFunctions.CreateRound());
            evaluator.CustomFunctions.Add("sqrt", MathFunctions.CreateSqrt(Mode));
            evaluator.CustomFunctions.Add("sgn", MathFunctions.CreateSgn());
            evaluator.CustomFunctions.Add("table", TableFunction.Create());
            evaluator.CustomFunctions.Add("u", MathFunctions.CreateU());
            evaluator.CustomFunctions.Add("uramp", MathFunctions.CreateURamp());
        }
    }
}
