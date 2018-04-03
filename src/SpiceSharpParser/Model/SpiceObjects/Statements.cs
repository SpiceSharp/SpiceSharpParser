using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Model.SpiceObjects
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
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            list.Clear();
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
    }
}
