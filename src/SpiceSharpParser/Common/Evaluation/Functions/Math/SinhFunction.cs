﻿using System;

namespace SpiceSharpParser.Common.Evaluation.Functions.Math
{
    public class SinhFunction : Function<double, double>
    {
        public SinhFunction()
        {
            Name = "sinh";
            VirtualParameters = false;
            ArgumentsCount = 1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("sinh() function expects one argument");
            }

            double d = args[0];
            return System.Math.Sinh(d);
        }
    }
}