using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;

namespace SpiceSharp.Physics2D.Core
{
    internal static class MechanicalValidation
    {
        public static void RegisterGroundReference(IRuleSubject subject, IRules rules)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            ComponentRuleParameters parameters =
                rules.GetParameterSet<ComponentRuleParameters>();
            var variables = new[] { parameters.Factory.GetSharedVariable("0") };
            foreach (IConductiveRule rule in rules.GetRules<IConductiveRule>())
            {
                rule.AddPath(subject, variables);
            }
        }
    }
}
