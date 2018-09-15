using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry with base functionalities.
    /// </summary>
    /// <typeparam name="TElement">
    /// Type of the registry element.
    /// </typeparam>
    public class BaseRegistry<TElement> : IEnumerable<TElement>, IRegistry<TElement>
        where TElement : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRegistry{TElement}"/> class.
        /// </summary>
        public BaseRegistry()
        {
        }

        /// <summary>
        /// Gets the count of elements in registry.
        /// </summary>
        public int Count
        {
            get
            {
                return Elements.Keys.Count;
            }
        }

        /// <summary>
        /// Gets the mapping type to element.
        /// </summary>
        protected Dictionary<string, TElement> Elements { get; } = new Dictionary<string, TElement>();

        /// <summary>
        /// Adds the element to the registry.
        /// </summary>
        /// <param name="element">Element to add</param>
        public virtual void Bind(string spiceName, TElement element)
        {
            Elements[spiceName] = element;
        }

        /// <summary>
        /// Adds the element to the registry.
        /// </summary>
        /// <param name="element">Element to add</param>
        public virtual void Bind(string[] spiceNames, TElement element)
        {
            foreach (var spiceName in spiceNames)
            {
                Elements[spiceName] = element;
            }
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
            return Elements.ContainsKey(type);
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
            return Elements[type];
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Elements.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
    }
}
