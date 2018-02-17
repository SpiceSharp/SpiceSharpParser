using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceNetlist.SpiceObjects
{
    public class Statements : Statement, IEnumerable
    {
        private List<Statement> list = null;

        public Statements()
        {
            list = new List<Statement>();
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Add(Statement statement)
        {
            list.Add(statement);
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Merge(Statements sts)
        {
            list.AddRange(sts.list);
        }

        public IEnumerable<Statement> OrderBy(Func<Statement, int> orderByFunc)
        {
            return list.OrderBy(orderByFunc);
        }
    }
}
