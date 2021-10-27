// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

namespace CameraTouch
{
    /// <summary>
    /// Represents the instance of a realized value.
    /// </summary>
    public class CameraPropertyValue
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the reported value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the short code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the value that will be used for naming.
        /// </summary>
        public string FileValue { get; set; }

        /// <summary>
        /// Gets or sets the typed value for sorting.
        /// </summary>
        public object ParsedValue { get; set; }

        /// <summary>
        /// Gets the string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() =>
            $"${Code}: {Name} = {Value} ({ParsedValue} => {FileValue})";

        /// <summary>
        /// Implementation of equals.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>A value indicating whether they are equal.</returns>
        public override bool Equals(object obj) => obj is CameraPropertyValue cpv
            && cpv.ToString() == ToString();

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => $"{Name}:{Value}".GetHashCode();
    }
}
