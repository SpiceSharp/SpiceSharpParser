using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings
{
    /// <summary>
    /// Base mapper.
    /// </summary>
    /// <typeparam name="TElement">
    /// Type of the element.
    /// </typeparam>
    public class BaseMapper<TElement> : IMapper<TElement>
        where TElement : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMapper{TElement}"/> class.
        /// </summary>
        public BaseMapper()
        {
            Elements = new Dictionary<string, TElement>();
        }

        /// <summary>
        /// Gets the count of elements in mapper.
        /// </summary>
        public int Count => Elements.Keys.Count;

        /// <summary>
        /// Gets the mapping type to element.
        /// </summary>
        protected Dictionary<string, TElement> Elements { get; }

        /// <summary>
        /// Binds key with element.
        /// </summary>
        /// <param name="key">Key of the element.</param>
        /// <param name="element">Element to add.</param>
        public virtual void Map(string key, TElement element)
        {
            Elements[key] = element;
        }

        /// <summary>
        /// Binds the element to the mapper.
        /// </summary>
        /// <param name="element">Element to add</param>
        public virtual void Map(string[] keys, TElement element)
        {
            foreach (var key in keys)
            {
                Elements[key] = element;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the mapper has a element for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// A key.
        /// </param>
        /// <param name="caseSensitive">Is key case-sensitive.</param>
        /// <returns>
        /// A value indicating whether the mapper has a element with given <paramref name="key"/>.
        /// </returns>
        public bool Contains(string key, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return Elements.ContainsKey(key);
            }

            return Elements.Keys.Contains(key, StringComparerProvider.Get(false));
        }

        /// <summary>
        /// Gets the element for given type.
        /// </summary>
        /// <param name="type">A type of element.</param>
        /// <param name="caseSensitive">Is type name case-sensitive.</param>
        /// <returns>
        /// A reference to the element.
        /// </returns>
        public TElement Get(string type, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return Elements[type];
            }

            return
                Elements
                    .First(e => e.Key.Equals(type, StringComparison.CurrentCultureIgnoreCase)).Value;
        }

        public IEnumerator<KeyValuePair<string, TElement>> GetEnumerator()
        {
            foreach (KeyValuePair<string, TElement> element in Elements)
            {
                yield return element;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<string, TElement> element in Elements)
            {
                yield return element;
            }
        }
    }
}
