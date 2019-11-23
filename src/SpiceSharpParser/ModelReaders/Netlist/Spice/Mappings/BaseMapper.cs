using SpiceSharpParser.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets the mapping key to element.
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
        /// <param name="keys">Keys.</param>
        /// <param name="element">Element.</param>
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
        public bool ContainsKey(string key, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return Elements.ContainsKey(key);
            }

            return Elements.Keys.Contains(key, StringComparerProvider.Get(false));
        }

        /// <summary>
        /// Gets the element for given key.
        /// </summary>
        /// <param name="key">A key of element.</param>
        /// <param name="caseSensitive">Is key name case-sensitive.</param>
        /// <returns>
        /// A reference to the element.
        /// </returns>
        public TElement GetValue(string key, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return Elements[key];
            }

            return
                Elements
                    .First(e => e.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Value;
        }

        /// <summary>
        /// Gets the element for given key.
        /// </summary>
        /// <param name="key">A key of element.</param>
        /// <param name="caseSensitive">Is key name case-sensitive.</param>
        /// <param name="value">A value of element.</param>
        /// <returns>
        /// A reference to the element.
        /// </returns>
        public bool TryGetValue(string key, bool caseSensitive, out TElement value)
        {
            if (caseSensitive)
            {
                return Elements.TryGetValue(key, out value);
            }

            value = Elements.FirstOrDefault(e => e.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Value;
            return value != null;
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<string, TElement>> GetEnumerator()
        {
            foreach (KeyValuePair<string, TElement> element in Elements)
            {
                yield return element;
            }
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<string, TElement> element in Elements)
            {
                yield return element;
            }
        }
    }
}