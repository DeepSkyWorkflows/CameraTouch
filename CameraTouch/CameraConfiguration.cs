// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace CameraTouch
{
    /// <summary>
    /// Configuration elements.
    /// </summary>
    public class CameraConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraConfiguration"/> class.
        /// </summary>
        /// <param name="readOnly">Whether or not to move the files.</param>
        /// <param name="recurse">Recurse subdirectories.</param>
        /// <param name="showProperties">Show properties per file.</param>
        /// <param name="showStatistics">Show overall statistics.</param>
        /// <param name="moveFile">Move (instead of copy).</param>
        /// <param name="directoryNamingSpec">Naming convention for directories.</param>
        /// <param name="fileNamingSpec">Naming convention for files.</param>
        /// <param name="source">Source to scan.</param>
        /// <param name="target">Target directory.</param>
        public CameraConfiguration(
            bool readOnly,
            bool recurse,
            bool showProperties,
            bool showStatistics,
            bool moveFile,
            string directoryNamingSpec,
            string fileNamingSpec,
            string source,
            string target)
        {
            ReadOnly = readOnly;
            Recurse = recurse;
            ShowProperties = showProperties;
            ShowStatistics = showStatistics;
            MoveFile = moveFile;
            DirectoryNamingSpec = directoryNamingSpec;
            FileNamingSpec = fileNamingSpec;
            Source = source;
            TargetDirectory = target;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraConfiguration"/> class.
        /// </summary>
        public CameraConfiguration()
        {
        }

        /// <summary>
        /// Gets the usage examples.
        /// </summary>
        [Usage(ApplicationAlias = "cameratouch")]
        public static IEnumerable<Example> Examples
            => new List<Example>()
            {
                new Example(
                    "Grab statistics only",
                    new CameraConfiguration(true, true, false, true, false, null, null, @"C:\myfiles", null)),
                new Example(
                    "Show stats per file",
                    new CameraConfiguration(true, true, true, true, false, null, null, @"C:\myfiles", null)),
                new Example(
                    "Show stats for a single file",
                    new CameraConfiguration(true, true, true, true, false, null, null, @"C:\myfiles\myfile.arw", null)),
                new Example(
                    "Rename in place with defaults",
                    new CameraConfiguration(false, true, false, false, true, null, null, @"C:\myfiles", null)),
                new Example(
                    "Copy to a new directory and organize by date then exposure time and ISO",
                    new CameraConfiguration(false, true, false, false, false, "$dt[YYYY-MM-DD];$et_$is", null, @"C:\myfiles", @"D:\targetFiles")),
            };

        /// <summary>
        /// Gets or sets the file or directory to parse.
        /// </summary>
        [Value(0, MetaName = nameof(Source), Required = true, HelpText = "The path to the file or directory to scan.")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the root target directorty.
        /// </summary>
        [Value(1, MetaName = nameof(TargetDirectory), Required = false, HelpText = "The root of the target directory to move files to.")]
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target is a directory.
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the files should be moved.
        /// </summary>
        [Option('s', "scan-only",  Default = false, HelpText = "Scan only. Do not move or rename files.")]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to recurse the target directories.
        /// </summary>
        [Option('r', "recurse-subdirectories", Default = false, HelpText = "Recurse subdirectories.")]
        public bool Recurse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to show properties of each file.
        /// </summary>
        [Option('p', "properties-display", Default = false, HelpText = "Show properties for each file.")]
        public bool ShowProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to move the file instead of copying.
        /// </summary>
        [Option('m', "move-file", Default = false, HelpText = "Move the file instead of copying.")]
        public bool MoveFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to show statistics.
        /// </summary>
        [Option('i', "info-statistics", Default = false, HelpText = "Show statistics.")]
        public bool ShowStatistics { get; set; }

        /// <summary>
        /// Gets or sets the naming spec for directories.
        /// </summary>
        [Option('d', "directory-specs", Default = null, HelpText = "Directory naming specifications.")]
        public string DirectoryNamingSpec { get; set; }

        /// <summary>
        /// Gets or sets the list of specs for directories.
        /// </summary>
        public string[] DirectoryNamingSpecs { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the file naming spec.
        /// </summary>
        [Option('f', "file-naming-spec", Default = "$et_$is_$fl_Image", HelpText = "File naming spec.")]
        public string FileNamingSpec { get; set; }

        /// <summary>
        /// Gets the generator to extract from the file spec.
        /// </summary>
        public Func<FileSpec, string> FileNameGenerator { get; private set; }

        /// <summary>
        /// Gets the ordered list of directory generators.
        /// </summary>
        public Func<FileSpec, string>[] DirectoryNameGenerators { get; private set; }

        /// <summary>
        /// Compiles options and specs.
        /// </summary>
        /// <param name="propertyManager">The <see cref="PropertyManager"/> instance to use.</param>
        public void Compile(PropertyManager propertyManager)
        {
            if (!File.Exists(Source) && !Directory.Exists(Source))
            {
                throw new FileNotFoundException(Source);
            }

            if (string.IsNullOrWhiteSpace(TargetDirectory))
            {
                if (Directory.Exists(Source))
                {
                    TargetDirectory = Source;
                }
                else if (File.Exists(Source))
                {
                    TargetDirectory = Path.GetDirectoryName(Source);
                }
            }
            else if (!Directory.Exists(TargetDirectory))
            {
                throw new DirectoryNotFoundException(TargetDirectory);
            }

            var compiledFileSpec = new SpecCompiler(propertyManager, FileNamingSpec);
            FileNameGenerator = compiledFileSpec.GetNameForSpec;

            if (!string.IsNullOrWhiteSpace(DirectoryNamingSpec))
            {
                DirectoryNamingSpecs = DirectoryNamingSpec.Split(';');
                DirectoryNameGenerators = DirectoryNamingSpecs
                    .Select(d => new SpecCompiler(propertyManager, d).GetNameForSpec)
                    .ToArray();
            }
        }

        /// <summary>
        /// Prints the options configuration.
        /// </summary>
        /// <returns>The list of configured options.</returns>
        public override string ToString()
        {
            var location = Process.GetCurrentProcess().MainModule.FileName;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(location);
            var heading = $"{fileVersionInfo.ProductName} {fileVersionInfo.ProductVersion}";
            var copyright = fileVersionInfo.LegalCopyright;
            var sb = new StringBuilder($"{heading}{Environment.NewLine}{copyright}{Environment.NewLine}");

            sb.AppendLine("Running with options:");
            sb.AppendLine(ParseOption(opt => opt.ReadOnly));
            sb.AppendLine(ParseOption(opt => opt.Recurse));
            sb.AppendLine(ParseOption(opt => opt.ShowProperties));
            sb.AppendLine(ParseOption(opt => opt.ShowStatistics));
            sb.AppendLine(ParseOption(opt => opt.FileNamingSpec));
            sb.AppendLine(ParseOption(opt => opt.DirectoryNamingSpec));
            sb.AppendLine(ParseOption(opt => opt.Source));
            sb.AppendLine(ParseOption(opt => opt.TargetDirectory));

            return sb.ToString();
        }

        /// <summary>
        /// Parses the property name and value for an option.
        /// </summary>
        /// <typeparam name="T">The type of thet option.</typeparam>
        /// <param name="option">An expression that resolves to the option.</param>
        /// <returns>The text to display the option settings.
        /// </returns>
        public string ParseOption<T>(Expression<Func<CameraConfiguration, T>> option)
        {
            const int left = 40;
            var value = option.Compile()(this);
            var lambda = option as LambdaExpression;
            var memberExpression = lambda.Body as MemberExpression;
            var name = memberExpression.Member.Name;
            var diff = left - name.Length;
            var pad = diff > 0 ? new string(' ', diff) : string.Empty;
            return $"{name}:{pad}{value}";
        }
    }
}
