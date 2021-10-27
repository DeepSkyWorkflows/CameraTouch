// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.
using System;
using System.Linq;
using MetadataExtractor;
using IO = System.IO;

namespace CameraTouch
{
    /// <summary>
    /// Extracts the file spec from the target file.
    /// </summary>
    public class FileSpecExtractor
    {
        private readonly PropertyManager propertyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSpecExtractor"/> class.
        /// </summary>
        /// <param name="propertyManager">The <see cref="PropertyManager"/> instance.</param>
        public FileSpecExtractor(PropertyManager propertyManager) => this.propertyManager = propertyManager;

        /// <summary>
        /// Parse the spec from a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The file specification.</returns>
        public FileSpec ParseFile(string filePath)
        {
            var fileSpec = new FileSpec { File = filePath };
            var info = new IO.FileInfo(filePath);
            foreach (var dir in ImageMetadataReader.ReadMetadata(filePath))
            {
                if (dir.Name.ToLowerInvariant().Contains("thumbnail"))
                {
                    continue;
                }

                foreach (var tag in dir.Tags)
                {
                    var prop = propertyManager[tag.Name];
                    if (prop != null)
                    {
                        var propertyValue = new CameraPropertyValue
                        {
                            Name = tag.Name,
                            Value = tag.Description,
                            Code = prop.Code,
                        };
                        propertyValue.ParsedValue = prop.ToObject(propertyValue.Value);
                        propertyValue.FileValue = prop.FileValue(propertyValue.ParsedValue);
                        var existing = fileSpec.Properties.FirstOrDefault(p => p.Code == propertyValue.Code);
                        if (existing != null)
                        {
                            if (existing.ParsedValue is IComparable comparable &&
                            comparable.CompareTo((IComparable)propertyValue.ParsedValue) < 0)
                            {
                                fileSpec.Properties.Remove(existing);
                                fileSpec.Properties.Add(propertyValue);
                            }
                        }
                        else
                        {
                            fileSpec.Properties.Add(propertyValue);
                        }
                    }
                }
            }

            void CheckProperty(string code, string val)
            {
                // defaults
                if (!fileSpec.Properties.Any(p => p.Code == code))
                {
                    var prop = propertyManager.PropertyByCode(code);
                    var dt = new CameraPropertyValue
                    {
                        Code = code,
                        Name = prop.Name,
                        Value = val,
                    };
                    dt.ParsedValue = prop.ToObject(dt.Value);
                    dt.FileValue = prop.FileValue(dt.ParsedValue);
                    fileSpec.Properties.Add(dt);
                }
            }

            CheckProperty("dt", info.LastAccessTime.ToString("yyyy:MM:dd HH:mm:ss"));
            CheckProperty("ft", info.Extension[1..]);
            CheckProperty("fd", info.Extension[1..]);
            CheckProperty("ex", info.Extension[1..]);
            CheckProperty("fn", IO.Path.GetFileNameWithoutExtension(info.Name));
            CheckProperty("sz", info.Length.ToString());

            return fileSpec;
        }
    }
}
