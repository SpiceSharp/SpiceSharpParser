using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Model.Netlist.Spice.Objects
{
    /// <summary>
    /// Ordered collection of statements
    /// </summary>
    public class Statements : SpiceObject, IEnumerable<Statement>
    {
        private List<Statement> list = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Statements"/> class.
        /// </summary>
        public Statements()
        {
            list = new List<Statement>();
        }

        /// <summary>
        /// Gets the statements count
        /// </summary>
        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        /// <summary>
        /// Gets an index of statement in statemens.
        /// </summary>
        /// <param name="statement">A statement.</param>
        /// <returns>
        /// An index of statement.
        /// </returns>
        public int IndexOf(Statement statement)
        {
            return this.list.IndexOf(statement);
        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Indexer.
        /// </summary>
        public Statement this[int index]
        {
            get
            {
                return this.list[index];
            }

            set
            {
                this.list[index] = value;
            }
        }

        public void Replace(int start, int end, IEnumerable<Statement> statements)
        {
            list.RemoveRange(start, end - start + 1);
            list.InsertRange(start, statements);
        }

        /// <summary>
        /// Adds a statement to the end of collection
        /// </summary>
        /// <param name="statement">A statement to add</param>
        public void Add(Statement statement)
        {
            list.Add(statement);
        }

        /// <summary>
        /// Gets the enumerator
        /// </summary>
        /// <returns>
        /// A new enumerator
        /// </returns>
        public IEnumerator<Statement> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator
        /// </summary>
        /// <returns>
        /// A new enumerator
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Merges the given statements collection this the current collection
        /// </summary>
        /// <param name="sts">A collection to merge</param>
        public void Merge(Statements sts)
        {
            list.AddRange(sts.list);
        }

        /// <summary>
        /// Replace the given statement by statements
        /// </summary>
        /// <param name="statement">A statement to replace</param>
        /// <param name="statements">Statements to replace with</param>
        public void Replace(Statement statement, IEnumerable<Statement> statements)
        {
            if (list.Contains(statement))
            {
                var index = list.IndexOf(statement); 
                list.InsertRange(index, statements);
                list.Remove(statement);
            }
            else
            {
                throw new Exception("Unkonwn statement to replace");
            }
        }

        /// <summary>
        /// Gets the enumerator of statemets in the given order
        /// </summary>
        /// <param name="orderByFunc">Specifies the order</param>
        /// <returns>
        /// A new enumerator
        /// </returns>
        public IEnumerable<Statement> OrderBy(Func<Statement, int> orderByFunc)
        {
            return list.OrderBy(orderByFunc);
        }

        /// <summary>
        /// Closes the object.
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override SpiceObject Clone()
        {
            var clone = new Statements();

            foreach (Statement statement in this.list)
            {
                clone.Add((Statement)statement.Clone());
            }

            return clone;
        }
    }
}
