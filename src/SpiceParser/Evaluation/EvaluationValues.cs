using System.Collections.Generic;
using SpiceLexer;
using SpiceNetlist;

namespace SpiceParser.Evaluation
{
    /// <summary>
    /// A collection of <see cref="EvaluationValue"/> items.
    /// </summary>
    public class EvaluationValues : List<EvaluationValue>
    {
        /// <summary>
        /// Gets lexem from the <see cref="EvaluationValue"/> at the specied index
        /// </summary>
        /// <param name="index">An index of the item</param>
        /// <returns>
        /// A lexem
        /// </returns>
        public string GetLexem(int index)
        {
            if (this[index] is TerminalEvaluationValue t)
            {
                return t.Token.Lexem;
            }

            throw new EvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Gets line number from the <see cref="EvaluationValue"/> at the specied index
        /// </summary>
        /// <param name="index">An index of the item</param>
        /// <returns>
        /// A line number of the lexem
        /// </returns>
        public int GetLexemLineNumber(int index)
        {
            if (this[index] is TerminalEvaluationValue t)
            {
                return t.Token.LineNumber;
            }

            throw new EvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Tries to gets a <see cref="SpiceToken"/> from specific <see cref="Evaluation.EvaluationValue"/> item
        /// </summary>
        /// <param name="index">An index of the item</param>
        /// <param name="result">A spice token</param>
        /// <returns>
        /// True if spice token can be returned
        /// </returns>
        public bool TryToGetToken(int index, out SpiceToken result)
        {
            if (this[index] is TerminalEvaluationValue t)
            {
                result = t.Token;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Tries to gets a <see cref="SpiceObject"/> from specific <see cref="Evaluation.EvaluationValue"/> item
        /// </summary>
        /// <param name="index">An index of the item</param>
        /// <param name="result">A spice token</param>
        /// <returns>
        /// True if spice token can be returned
        /// </returns>
        public bool TryToGetSpiceObject<T>(int index, out T result)
            where T : SpiceObject
        {
            if (this[index] is NonTerminalEvaluationValue nt && nt.SpiceObject is T)
            {
                result = (T)nt.SpiceObject;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Gets a <see cref="SpiceObject"/> from specific <see cref="Evaluation.EvaluationValue"/> item
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SpiceObject"/> to look</typeparam>
        /// <param name="index">An index of the item</param>
        /// <returns>
        /// A reference to <see cref="T"/> spice object
        /// </returns>
        public T GetSpiceObject<T>(int index)
            where T : SpiceObject
        {
            if (this[index] is NonTerminalEvaluationValue nt)
            {
                if (nt.SpiceObject is T)
                {
                    return (T)nt.SpiceObject;
                }
            }

            throw new EvaluationException("Wrong evaluation type");
        }
    }
}
