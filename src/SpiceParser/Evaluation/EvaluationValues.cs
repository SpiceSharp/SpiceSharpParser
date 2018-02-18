using System.Collections.Generic;
using SpiceLexer;
using SpiceNetlist;

namespace SpiceParser.Evaluation
{
    public class EvaluationValues : List<EvaluationValue>
    {
        /// <summary>
        /// Gets lexem from specific EvaluationValue item
        /// </summary>
        public string GetLexem(int index)
        {
            if (this[index] is TerminalEvaluationValue t)
            {
                return t.Token.Lexem;
            }

            throw new EvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Gets lexem line numeber from specific EvaluationValue item
        /// </summary>
        public int GetLexemLineNumber(int index)
        {
            if (this[index] is TerminalEvaluationValue t)
            {
                return t.Token.LineNumber;
            }

            throw new EvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Gets token from specific EvaluationValue item
        /// </summary>
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
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="result"></param>
        /// <returns></returns>
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
        /// Gets SpiceObject from specific EvaluationValue item
        /// </summary>
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
