using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceNetlist.SpiceObjects
{
    public class Statements : SpiceObject, IEnumerable
    {
        List<Statement> List { get; set; }

        public Statements()
        {
            List = new List<Statement>();
        }

        public void Clear()
        {
            List.Clear();
        }

        public void Add(Statement statement)
        {
            List.Add(statement);
        }

        public IEnumerator GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public void Merge(Statements sts)
        {
            List.AddRange(sts.List);
        }

        public IEnumerable<Statement> OrderBy(Func<Statement, int> orderByFunc)
        {
            return List.OrderBy(orderByFunc);
        }
    }
}
