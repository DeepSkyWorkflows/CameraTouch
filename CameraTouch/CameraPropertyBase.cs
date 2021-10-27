// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

namespace CameraTouch
{
    /// <summary>
    /// Base class for tracked property.
    /// </summary>
    public abstract class CameraPropertyBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the code used for setting the file pattern.
        /// </summary>
        public virtual string Code { get; set; }

        /// <summary>
        /// Signature of object conversion.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The strongly typed value.</returns>
        public abstract object ToObject(string value);

        /// <summary>
        /// Signature of file value conversion.
        /// </summary>
        /// <param name="value">The strongly typed value.</param>
        /// <returns>The value to use for naming.</returns>
        public abstract string FileValue(object value);

        /// <summary>
        /// String override.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString() => $"${Code}: {Name}";

        /// <summary>
        /// Implement equality.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>A value indicating whether the instances are equal.</returns>
        public override bool Equals(object obj) =>
            obj is CameraPropertyBase cb && cb.Code == Code;

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code of the code.</returns>
        public override int GetHashCode() => Code.GetHashCode();
    }
}
