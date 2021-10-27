// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CameraTouch
{
    /// <summary>
    /// Handles parsing sources.
    /// </summary>
    public class SourceResolver
    {
        /// <summary>
        /// What we think is supported.
        /// </summary>
        private readonly string[] extensions = new[] { ".arw", ".raw", ".jpg", ".jpeg", ".tif", ".tiff" };

        /// <summary>
        /// Test for valid extension.
        /// </summary>
        /// <param name="ext">The extension to check.</param>
        /// <returns>A value indicating whether the extension is valid.</returns>
        public bool IsValidExtension(string ext) => extensions.Contains(ext);

        /// <summary>
        /// Uses the configuration to compiled the list of files to process.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>The list of files.</returns>
        public string[] CollectFiles(CameraConfiguration config)
        {
            if (File.Exists(config.Source))
            {
                return new[] { config.Source };
            }

            if (Directory.Exists(config.Source))
            {
                config.IsDirectory = true;
                return ParseDirectory(config.Source, config);
            }
            else
            {
                throw new FileNotFoundException(config.Source);
            }
        }

        /// <summary>
        /// Parses the directory.
        /// </summary>
        /// <param name="source">The directory to parse.</param>
        /// <param name="config">Configuration.</param>
        /// <returns>The list of files found.</returns>
        private string[] ParseDirectory(string source, CameraConfiguration config)
        {
            var list = new List<string>();
            if (config.Recurse)
            {
                foreach (var dir in Directory.EnumerateDirectories(source))
                {
                    var files = ParseDirectory(dir, config);
                    if (files.Length > 0)
                    {
                        list.AddRange(files);
                    }
                }
            }

            foreach (var file in Directory.EnumerateFiles(source))
            {
                if (extensions.Any(e => e == Path.GetExtension(file).ToLowerInvariant()))
                {
                    list.Add(file);
                }
            }

            return list.ToArray();
        }
    }
}
