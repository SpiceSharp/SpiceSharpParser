using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiceSharpParser.Models.Netlist.Spice.Objects
{
    /// <summary>
    /// A ordered collection of parameters.
    /// </summary>
    public class ParameterCollection : SpiceObject, IEnumerable<Parameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection(List<Parameter> values)
        {
            Values = values;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection()
        {
            Values = new List<Parameter>();
        }

        /// <summary>
        /// Gets the count of parameters in the collection.
        /// </summary>
        public int Count => Values.Count;

        public override SpiceLineInfo LineInfo => Values.FirstOrDefault()?.LineInfo;

        /// <summary>
        /// Gets the values of parameters.
        /// </summary>
        protected List<Parameter> Values { get; }

        /// <summary>
        /// Gets the parameter at specified index.
        /// </summary>
        /// <param name="index">The index of parameter.</param>
        /// <returns>
        /// A reference to parameter.
        /// </returns>
        public Parameter this[int index] => Values[index];

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            Values.Clear();
        }

        /// <summary>
        /// Adds parameter to the collection.
        /// </summary>
        /// <param name="parameter">A parameter to add.</param>
        public void Add(Parameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Values.Add(parameter);
        }

        /// <summary>
        /// Gets an enumerator of parameters in collection.
        /// </summary>
        /// <returns>
        /// A new enumerator.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Gets typed enumerator of parameters in collection.
        /// </summary>
        /// <returns>
        /// A new enumerator.
        /// </returns>
        IEnumerator<Parameter> IEnumerable<Parameter>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Inserts a parameter at the specified index in the collection.
        /// </summary>
        /// <param name="index">An index.</param>
        /// <param name="parameter">A parameter to insert.</param>
        public void Insert(int index, Parameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Values.Insert(index, parameter);
        }

        /// <summary>
        /// Merges a collection to the current collection.
        /// </summary>
        /// <param name="collection">A collection to merge.</param>
        public void Merge(ParameterCollection collection)
        {
            Values.AddRange(collection.Values);
        }

        /// <summary>
        /// Set parameters from a collection to the current collection.
        /// </summary>
        /// <param name="collection">A collection to merge.</param>
        public void Set(ParameterCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

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
                            a2.Values = a.Values;
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
        /// Creates a clone of the current collection without first 'count' elements.
        /// </summary>
        /// <param name="count">Number of parameters to skip.</param>
        /// <returns>
        /// A new collection of parameters.
        /// </returns>
        public ParameterCollection Skip(int count)
        {
            var result = new ParameterCollection(Values.Skip(count).ToList());
            return result;
        }

        /// <summary>
        /// Creates a clone of the current collection with first 'count' elements.
        /// </summary>
        /// <param name="count">Number of parameters to take.</param>
        /// <returns>
        /// A new collection of parameters.
        /// </returns>
        public ParameterCollection Take(int count)
        {
            var result = new ParameterCollection(Values.Take(count).ToList());
            return result;
        }

        /// <summary>
        /// Gets the string from parameter in the collection.
        /// Throws an exception if parameter is not <see cref="SingleParameter"/>.
        /// </summary>
        /// <param name="parameterIndex">An index of parameter.</param>
        /// <returns>
        /// A string from parameter.
        /// </returns>
        public Parameter Get(int parameterIndex)
        {
            if (Count <= parameterIndex)
            {
                return null;
            }

            return this[parameterIndex];
        }

        /// <summary>
        /// Gets the value string from parameter in the collection.
        /// Throws an exception if parameter is not a value type parameter.
        /// </summary>
        /// <param name="parameterIndex">An index of parameter.</param>
        /// <returns>
        /// A value from parameter.
        /// </returns>
        public bool IsValueString(int parameterIndex)
        {
            var singleParameter = this[parameterIndex] as SingleParameter;

            if (singleParameter == null
                || singleParameter is PercentParameter
                || singleParameter is ReferenceParameter
                || singleParameter is StringParameter)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a new clone of the collection.
        /// </summary>
        /// <returns>
        /// A new collection of parameters.
        /// </returns>
        public override SpiceObject Clone()
        {
            var result = new ParameterCollection(new List<Parameter>());

            foreach (var param in Values)
            {
                result.Add((Parameter)param.Clone());
            }

            return result;
        }

        /// <summary>
        /// Removes the parameter at the specified index.
        /// </summary>
        /// <param name="index">An index of parameter to remove.</param>
        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
        }

        /// <summary>
        /// Removes the parameter at the specified index.
        /// </summary>
        /// <param name="parameter">Parameter to remove.</param>
        public void Remove(Parameter parameter)
        {
            Values.Remove(parameter);
        }

        public override string ToString()
        {
            if (Values.Any(v => v.ToString().ToLower() == "params:"))
            {
                var paramsIndex = Values.IndexOf(Values.First(v => v.ToString().ToLower() == "params:"));
                var resultBuilder = new StringBuilder();
                for (var i = 0; i < Values.Count; i++)
                {
                    if (i > paramsIndex && i != Values.Count - 1)
                    {
                        resultBuilder.Append($"{Values[i]}, ");
                    }
                    else
                    {
                        if (i != Values.Count - 1)
                        {
                            resultBuilder.Append($"{Values[i]} ");
                        }
                        else
                        {
                            resultBuilder.Append($"{Values[i]}");
                        }
                    }
                }

                return resultBuilder.ToString();
            }

            return string.Join(" ", Values.Select(v => v.ToString()));
        }

        public int IndexOf(Parameter parameter)
        {
            return Values.IndexOf(parameter);
        }
    }
}