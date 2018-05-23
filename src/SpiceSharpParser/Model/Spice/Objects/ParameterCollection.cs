using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Model.Spice.Objects.Parameters;

namespace SpiceSharpParser.Model.Spice.Objects
{
    /// <summary>
    /// A ordered collection of parameters
    /// </summary>
    public class ParameterCollection : SpiceObject, IEnumerable, IEnumerable<Parameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection()
        {
            Values = new List<Parameter>();
        }

        /// <summary>
        /// Gets the count of paramters in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return Values.Count;
            }
        }

        /// <summary>
        /// Gets or sets the values of paramters
        /// </summary>
        protected List<Parameter> Values { get; set; }

        /// <summary>
        /// Gets the parameter at specified index
        /// </summary>
        /// <param name="index">The index of parameter</param>
        /// <returns>
        /// A reference to parameter
        /// </returns>
        public Parameter this[int index] => Values[index];

        /// <summary>
        /// Clears the collction
        /// </summary>
        public void Clear()
        {
            Values.Clear();
        }

        /// <summary>
        /// Adds parameter to the collction
        /// </summary>
        /// <param name="parameter">A parameter to add</param>
        public void Add(Parameter parameter)
        {
            Values.Add(parameter);
        }

        /// <summary>
        /// Gets an enumerator of parameters in collection
        /// </summary>
        /// <returns>
        /// A new enumerator
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Gets typed enumerator of parameters in collection
        /// </summary>
        /// <returns>
        /// A new enumerator
        /// </returns>
        IEnumerator<Parameter> IEnumerable<Parameter>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Inserts a parameter at the specifed index in the collection
        /// </summary>
        /// <param name="index">An index</param>
        /// <param name="parameter">A parameter to insert</param>
        public void Insert(int index, Parameter parameter)
        {
            Values.Insert(index, parameter);
        }

        /// <summary>
        /// Merges a collection to the current collection
        /// </summary>
        /// <param name="collection">A collection to merge</param>
        public void Merge(ParameterCollection collection)
        {
            Values.AddRange(collection.Values);
        }

        /// <summary>
        /// Set paramters from a collection to the current collection
        /// </summary>
        /// <param name="collection">A collection to merge</param>
        public void Set(ParameterCollection collection)
        {
            foreach (var value in collection.Values)
            {
                if (value is SingleParameter)
                {
                    Values.Add(value);
                }

                if (value is AssignmentParameter a)
                {
                    bool found = false;
                    foreach (var val in Values)
                    {
                        if (val is AssignmentParameter a2 && a2.Name == a.Name)
                        {
                            a2.Value = a.Value;
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        Values.Add(value);
                    }
                }

                if (value is BracketParameter bp)
                {
                    foreach (var val in Values)
                    {
                        if (val is BracketParameter bp2 && bp2.Name == bp.Name)
                        {
                            bp2.Parameters.Set(bp.Parameters);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a clone of the current collection without first 'count' elements
        /// </summary>
        /// <param name="count">Number of paramaters to skip</param>
        /// <returns>
        /// A new collection of parameters
        /// </returns>
        public ParameterCollection Skip(int count)
        {
            var result = new ParameterCollection();
            result.Values.AddRange(this.Values.Skip(count));

            return result;
        }

        /// <summary>
        /// Creates a clone of the current collection with first 'count' elements
        /// </summary>
        /// <param name="count">Number of paramaters to take</param>
        /// <returns>
        /// A new collection of parameters
        /// </returns>
        public ParameterCollection Take(int count)
        {
            var result = new ParameterCollection();
            result.Values.AddRange(this.Values.Take(count));
            return result;
        }

        /// <summary>
        /// Gets the string from parameter in the collection.
        /// Throws an exception if parameter is not <see cref="SingleParameter"/>
        /// </summary>
        /// <param name="parameterIndex">An index of parameter</param>
        /// <returns>
        /// A string from parameter
        /// </returns>
        public string GetString(int parameterIndex)
        {
            var singleParameter = this[parameterIndex] as SingleParameter;
            if (singleParameter == null)
            {
                throw new Exception("Parameter [" + parameterIndex + "] is not string parameter");
            }

            return singleParameter.Image;
        }

        /// <summary>
        /// Creats a new clone of the collection
        /// </summary>
        /// <returns>
        /// A new collection of parameters
        /// </returns>
        public override SpiceObject Clone()
        {
            return new ParameterCollection() { Values = new List<Parameter>(this.Values) };
        }

        /// <summary>
        /// Removes the parameter at the specified index
        /// </summary>
        /// <param name="index">An index of parameter to remove</param>
        public void Remove(int index)
        {
            this.Values.RemoveAt(index);
        }
    }
}
