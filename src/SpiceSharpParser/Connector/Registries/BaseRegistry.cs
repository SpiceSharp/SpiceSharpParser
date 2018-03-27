using SpiceSharpParser.Connector.Processors.Common;
using System.Collections.Generic;

namespace SpiceSharpParser.Connector.Registries
{
    /// <summary>
    /// Registry with base functionalities
    /// </summary>
    /// <typeparam name="TElement">
    /// Type of the registry element
    /// </typeparam>
    public class BaseRegistry<TElement>
        where TElement : IGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRegistry{TElement}"/> class.
        /// </summary>
        public BaseRegistry()
        {
        }

        /// <summary>
        /// Gets the count of elements in registry
        /// </summary>
        public int Count
        {
            get
            {
                return Elements.Count;
            }
        }

        /// <summary>
        /// Gets the elements of the registry
        /// </summary>
        protected List<TElement> Elements { get; } = new List<TElement>();

        /// <summary>
        /// Gets the types of elements in the registry
        /// </summary>
        protected List<string> ElementsTypes { get; } = new List<string>();

        /// <summary>
        /// Gets the mapping type to element
        /// </summary>
        protected Dictionary<string, TElement> ElementsByType { get; } = new Dictionary<string, TElement>();

        /// <summary>
        /// Adds the element to the registry
        /// </summary>
        /// <param name="element">Element to add</param>
        public virtual void Add(TElement element)
        {
            if (ElementsByType.ContainsKey(element.TypeName))
            {
                var currentElement = ElementsByType[element.TypeName];
                var index = Elements.IndexOf(currentElement);
                Elements.RemoveAt(index);
                ElementsTypes.RemoveAt(index);
            }

            Elements.Add(element);
            ElementsTypes.Add(element.TypeName);
            ElementsByType[element.TypeName] = element;
        }

        /// <summary>
        /// Returns a value indicating whether the registry has a element for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// A type of generator to look
        /// </param>
        /// <returns>
        /// A boolean value
        /// </returns>
        public bool Supports(string type)
        {
            return ElementsByType.ContainsKey(type);
        }

        /// <summary>
        /// Gets the element for given type
        /// </summary>
        /// <param name="type">A type of element</param>
        /// <returns>
        /// A reference to the element
        /// </returns>
        public TElement Get(string type)
        {
            return ElementsByType[type];
        }

        /// <summary>
        /// Gets the index of the element with given type
        /// </summary>
        /// <param name="type">A type of element</param>
        /// <returns>
        /// A reference to the element
        /// </returns>
        public int IndexOf(string type)
        {
            return ElementsTypes.IndexOf(type);
        }
    }
}
