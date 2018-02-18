using System.Collections.Generic;
using SpiceNetlist.SpiceSharpConnector.Common;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class BaseRegistry<TElement>
        where TElement : ITyped
    {
        protected List<TElement> elements = new List<TElement>();
        protected List<string> elementsTypes = new List<string>();
        protected Dictionary<string, TElement> elementsByType = new Dictionary<string, TElement>();

        public BaseRegistry()
        {
        }

        public virtual void Add(TElement element)
        {
            elements.Add(element);
            elementsByType[element.Type] = element;
        }

        public bool Supports(string type)
        {
            return elementsByType.ContainsKey(type);
        }

        public TElement Get(string type)
        {
            return elementsByType[type];
        }

        public int IndexOf(string type)
        {
            return elementsTypes.IndexOf(type);
        }
    }
}
