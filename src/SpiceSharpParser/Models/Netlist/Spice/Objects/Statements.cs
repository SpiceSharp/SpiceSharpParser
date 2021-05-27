using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// An ordered collection of statements.
    /// </summary>
    public class Statements : SpiceObject, IEnumerable<Statement>
    {
        private readonly List<Statement> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="Statements"/> class.
        /// </summary>
        public Statements()
        {
            _list = new List<Statement>();
        }

        /// <summary>
        /// Gets the statements count.
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Gets the line info.
        /// </summary>
        public override SpiceLineInfo LineInfo => _list.FirstOrDefault()?.LineInfo;

        /// <summary>
        /// Indexer.
        /// </summary>
        public Statement this[int index]
        {
            get => _list[index];

            set => _list[index] = value;
        }

        /// <summary>
        /// Gets an index of statement in statements.
        /// </summary>
        /// <param name="statement">A statement.</param>
        /// <returns>
        /// An index of statement.
        /// </returns>
        public int IndexOf(Statement statement)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            return _list.IndexOf(statement);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        public void Replace(int start, int end, IEnumerable<Statement> statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            _list.RemoveRange(start, end - start + 1);
            _list.InsertRange(start, statements);
        }

        /// <summary>
        /// Adds a statement to the end of collection.
        /// </summary>
        /// <param name="statement">A statement to add.</param>
        public void Add(Statement statement)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            _list.Add(statement);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// A new enumerator.
        /// </returns>
        public IEnumerator<Statement> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>
        /// A new enumerator.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Merges the given statements collection this the current collection.
        /// </summary>
        /// <param name="sts">A collection to merge.</param>
        public void Merge(Statements sts)
        {
            if (sts == null)
            {
                throw new ArgumentNullException(nameof(sts));
            }

            _list.AddRange(sts._list);
        }

        /// <summary>
        /// Replace the given statement by statements.
        /// </summary>
        /// <param name="statement">A statement to replace.</param>
        /// <param name="statements">Statements to replace with.</param>
        public void Replace(Statement statement, IEnumerable<Statement> statements)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            if (_list.Contains(statement))
            {
                var index = _list.IndexOf(statement);
                _list.InsertRange(index, statements);
                _list.Remove(statement);
            }
            else
            {
                throw new SpiceSharpParserException("Unknown statement to replace", statement.LineInfo);
            }
        }

        /// <summary>
        /// Gets the enumerator of statements in the given order.
        /// </summary>
        /// <param name="orderByFunc">Specifies the order.</param>
        /// <returns>
        /// A new enumerator.
        /// </returns>
        public IEnumerable<Statement> OrderBy(Func<Statement, int> orderByFunc)
        {
            if (orderByFunc == null)
            {
                throw new ArgumentNullException(nameof(orderByFunc));
            }

            return _list.OrderBy(orderByFunc);
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public override SpiceObject Clone()
        {
            var clone = new Statements();

            foreach (Statement statement in _list)
            {
                clone.Add((Statement)statement.Clone());
            }

            return clone;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Statement previousStatement = null;
            foreach (Statement statement in _list)
            {
                if (previousStatement != null)
                {
                    if (previousStatement is SubCircuit s)
                    {
                        for (var i = 0; i < statement.StartLineNumber - s.Statements.Last().EndLineNumber - 3; i++)
                        {
                            builder.AppendLine();
                        }
                    }
                    else
                    {
                        for (var i = 0; i < statement.StartLineNumber - previousStatement.EndLineNumber - 1; i++)
                        {
                            builder.AppendLine();
                        }
                    }
                }

                if (_list.IndexOf(statement) == _list.Count - 1)
                {
                    builder.Append(statement);
                }
                else
                {
                    builder.AppendLine(statement.ToString());
                }

                previousStatement = statement;
            }

            return builder.ToString();
        }
    }
}