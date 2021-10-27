// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Collections.Generic;
using System.Linq;

namespace CameraTouch
{
    /// <summary>
    /// A specification for a file.
    /// </summary>
    public class FileSpec
    {
        /// <summary>
        /// Gets or sets the original file path.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the new file name.
        /// </summary>
        public string NewFile { get; set; }

        /// <summary>
        /// Gets or sets the directories the file is contained in.
        /// </summary>
        public string[] Directories { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the key for assigning ordinals.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets a list of the properties.
        /// </summary>
        public IList<CameraPropertyValue> Properties { get; private set; } = new List<CameraPropertyValue>();

        /// <summary>
        /// String implementation.
        /// </summary>
        /// <returns>The string representation of the spec.</returns>
        public override string ToString()
        {
            var list = new List<string>
            {
                File,
            };
            list.AddRange(Properties.Select(prop => prop.ToString()));
            return string.Join("\r\n", list);
        }

        /// <summary>
        /// Compare files.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>A value indicating whether they are equal.</returns>
        public override bool Equals(object obj) => obj is FileSpec fs && fs.File == File;

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code of the file.</returns>
        public override int GetHashCode() => File.GetHashCode();
    }
}
