// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;

namespace CameraTouch
{
    /// <summary>
    /// Represents a property to track.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    public class CameraProperty<T> : CameraPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProperty{T}"/> class.
        /// </summary>
        /// <param name="code">Short code for specs.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="converterIn">Convert to value.</param>
        /// <param name="converterOut">Convert to display.</param>
        public CameraProperty(
            string code,
            string name,
            Func<string, T> converterIn,
            Func<T, string> converterOut)
        {
            Code = code;
            Name = name;
            ConverterIn = converterIn;
            ConverterOut = converterOut;
        }

        /// <summary>
        /// Gets or sets the function to convert the value from a string.
        /// </summary>
        public Func<string, T> ConverterIn { get; set; }

        /// <summary>
        /// Gets or sets the function to convert the value to a string.
        /// </summary>
        public Func<T, string> ConverterOut { get; set; }

        /// <summary>
        /// Provides the string value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The string value.</returns>
        public override string FileValue(object value) => ConverterOut((T)value);

        /// <summary>
        /// Transforms to typed value.
        /// </summary>
        /// <param name="value">The string representation.</param>
        /// <returns>The actual value.</returns>
        public override object ToObject(string value) => ConverterIn(value);
    }
}