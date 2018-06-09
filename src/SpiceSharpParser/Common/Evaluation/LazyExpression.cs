using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.Common.Evaluation
{
    public class LazyExpression
    {
        private double value;

        public LazyExpression(Func<string, object, double> logic, string expression = null)
        {
            Expression = expression;
            Logic = logic;
        }

        public Func<string, object, double> Logic { get; }

        public string Expression { get; }

        public bool IsLoaded { get; private set; }

        public double GetValue(object context)
        {
            if (!IsLoaded)
            {
                value = Logic(Expression, context);
                IsLoaded = true;
            }
            return value;
        }

        public void Invalidate()
        {
            IsLoaded = false;
        }
    }
}
