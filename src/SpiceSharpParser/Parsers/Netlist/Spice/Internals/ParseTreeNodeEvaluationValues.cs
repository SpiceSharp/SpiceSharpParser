using System.Collections.Generic;
using SpiceSharpParser.Common;
using SpiceSharpParser.Lexers;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Netlist.Spice.Internals
{
    /// <summary>
    /// A collection of <see cref="ParseTreeNodeEvaluationValue"/> items.
    /// </summary>
    public class ParseTreeNodeEvaluationValues : List<ParseTreeNodeEvaluationValue>, ILocationProvider
    {
        public ParseTreeNodeEvaluationValues(int lineNumber, int startColumnIndex, int endColumnIndex, string filename)
        {
            LineNumber = lineNumber;
            StartColumnIndex = startColumnIndex;
            EndColumnIndex = endColumnIndex;
            FileName = filename;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the start column index.
        /// </summary>
        public int StartColumnIndex { get; }

        /// <summary>
        /// Gets the end column index.
        /// </summary>
        public int EndColumnIndex { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets lexem from the <see cref="ParseTreeNodeEvaluationValue"/> at the specified index.
        /// </summary>
        /// <param name="index">An index of the item.</param>
        /// <returns>
        /// A lexem.
        /// </returns>
        public string GetLexem(int index)
        {
            if (this[index] is ParseTreeNodeTerminalEvaluationValue t)
            {
                return t.Token.Lexem;
            }

            throw new ParseTreeEvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Gets line number from the <see cref="ParseTreeNodeEvaluationValue"/> at the specified index.
        /// </summary>
        /// <param name="index">An index of the item.</param>
        /// <returns>
        /// A line number of the lexem.
        /// </returns>
        public int GetLexemLineNumber(int index)
        {
            if (this[index] is ParseTreeNodeTerminalEvaluationValue t)
            {
                return t.Token.LineNumber;
            }

            throw new ParseTreeEvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Gets file name from the <see cref="ParseTreeNodeEvaluationValue"/> at the specified index.
        /// </summary>
        /// <param name="index">An index of the item.</param>
        /// <returns>
        /// A file name of the lexem.
        /// </returns>
        public string GetLexemFileName(int index)
        {
            if (this[index] is ParseTreeNodeTerminalEvaluationValue t)
            {
                return t.Token.FileName;
            }

            throw new ParseTreeEvaluationException("Wrong evaluation type");
        }

        /// <summary>
        /// Tries to gets a <see cref="SpiceToken"/> from specific <see cref="ParseTreeNodeEvaluationValue"/> item.
        /// </summary>
        /// <param name="index">An index of the item.</param>
        /// <param name="result">A SPICE token.</param>
        /// <returns>
        /// True if SPICE token can be returned.
        /// </returns>
        public bool TryToGetToken(int index, out SpiceToken result)
        {
            if (this[index] is ParseTreeNodeTerminalEvaluationValue t)
            {
                result = t.Token;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Tries to gets a <see cref="SpiceToken"/> from specific <see cref="ParseTreeNodeEvaluationValue"/> item.
        /// </summary>
        /// <param name="index">An index of the item.</param>
        public Token GetToken(int index)
        {
            if (this[index] is ParseTreeNodeTerminalEvaluationValue t)
            {
                return t.Token;
            }

            return null;
        }

        /// <summary>
        /// Tries to gets a <see cref="SpiceObject"/> from specific <see cref="ParseTreeNodeEvaluationValue"/> item.
        /// </summary>
        /// <param name="index">An index of the item.</param>
        /// <param name="result">A SPICE token.</param>
        /// <returns>
        /// True if SPICE token can be returned.
        /// </returns>
        public bool TryToGetSpiceObject<T>(int index, out T result)
            where T : SpiceObject
        {
            if (this[index] is ParseTreeNonTerminalEvaluationValue nt && nt.SpiceObject is T variable)
            {
                result = variable;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Gets a <see cref="SpiceObject"/> from specific <see cref="ParseTreeNodeEvaluationValue"/> item.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SpiceObject"/> to look.</typeparam>
        /// <param name="index">An index of the item.</param>
        /// <returns>
        /// A reference to <typeparamref name="T"/> SPICE object.
        /// </returns>
        public T GetSpiceObject<T>(int index)
            where T : SpiceObject
        {
            if (this[index] is ParseTreeNonTerminalEvaluationValue nt)
            {
                if (nt.SpiceObject is T variable)
                {
                    return variable;
                }
            }

            throw new ParseTreeEvaluationException("Wrong evaluation type");
        }
    }
}