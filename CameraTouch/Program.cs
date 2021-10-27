// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace CameraTouch
{
    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var propertyManager = new PropertyManager();
                var parser = new Parser(with => with.HelpWriter = null);
                var result = parser.ParseArguments<CameraConfiguration>(args);
                result.WithParsed(config => Process(config, propertyManager))
                    .WithNotParsed(_ => ShowHelp(result, propertyManager));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The program terminated unexpectedly: {ex.Message}");
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// Shows the help text.
        /// </summary>
        /// <param name="result">The parsed configuration.</param>
        /// <param name="propertyManager">The <see cref="PropertyManager"/>.</param>
        private static void ShowHelp(ParserResult<CameraConfiguration> result, PropertyManager propertyManager)
        {
            var helpText = HelpText.AutoBuild(
               result,
               h =>
               {
                   h.AddEnumValuesToHelpText = true;
                   return HelpText.DefaultParsingErrorsHandler(result, h);
               }, e => e);
            helpText.AddPostOptionsText(propertyManager.OptionsText);
            Console.WriteLine(helpText);
        }

        /// <summary>
        /// Processes with current configuration.
        /// </summary>
        /// <param name="config">The parsed configuration.</param>
        /// <param name="propMan">The <see cref="PropertyManager"/>.</param>
        private static void Process(CameraConfiguration config, PropertyManager propMan)
        {
            config.Compile(propMan);
            Console.WriteLine(config.ToString());

            var extractor = new FileSpecExtractor(propMan);
            var statistics = new Statistics(propMan);
            var sourceResolver = new SourceResolver();
            var spinner = new[] { "/", "-", @"\", "|" };
            var spinnerIdx = 0;
            var fileCount = 0;
            var totalFiles = 0;
            var lastSpin = DateTime.Now;

            FileSpec StatAndResolve(string file)
            {
                var spec = extractor.ParseFile(file);
                statistics.Aggregate(spec);
                if (config.ShowProperties)
                {
                    Console.WriteLine(spec.ToString());
                }
                else
                {
                    fileCount++;
                    if (DateTime.Now - lastSpin > TimeSpan.FromSeconds(1))
                    {
                        var pct = fileCount * 100 / totalFiles;
                        Console.Write($"{spinner[spinnerIdx]} {pct}%");
                        Console.SetCursorPosition(0, Console.CursorTop);
                        spinnerIdx = (spinnerIdx + 1) % spinner.Length;
                        lastSpin = DateTime.Now;
                    }
                }

                return spec;
            }

            Console.WriteLine("Engine initialized. Building file list...");

            var files = sourceResolver.CollectFiles(config);

            totalFiles = files.Length;
            Console.WriteLine($"Found {files.Length} files to process. Parsing properties...");

            var specs = files.Select(StatAndResolve).ToList();
            Console.WriteLine($"{Environment.NewLine}Done parsing.");

            Console.WriteLine("Building specs...");

            var directoriesToCreate = new List<string>();
            var badChars = Path.GetInvalidFileNameChars();

            foreach (var file in specs)
            {
                file.NewFile = string.Join("_", config.FileNameGenerator(file).Split(badChars));
                var ext = Path.GetExtension(file.NewFile);
                if (!string.IsNullOrWhiteSpace(ext))
                {
                    if (!sourceResolver.IsValidExtension(ext))
                    {
                        file.NewFile = file.NewFile.Replace('.', '_');
                        ext = string.Empty;
                    }
                }

                if (string.IsNullOrWhiteSpace(ext))
                {
                    file.NewFile = $"{file.NewFile}{Path.GetExtension(file.File)}";
                }

                file.Directories =
                    string.IsNullOrWhiteSpace(config.DirectoryNamingSpec) ?
                    Array.Empty<string>() :
                    config.DirectoryNameGenerators.Select(g => string.Join("_", g(file).Split(badChars)))
                    .ToArray();

                var dir = string.Empty;
                for (var idx = 0; idx < file.Directories.Length; idx++)
                {
                    dir += $"{file.Directories[idx]}/";
                    file.Directories[idx] = dir;
                    var fullDir = Path.Combine(config.TargetDirectory, dir);
                    if (!directoriesToCreate.Contains(fullDir) && !Directory.Exists(fullDir))
                    {
                        directoriesToCreate.Add(fullDir);
                    }
                }

                file.Key = $"{dir}{file.NewFile}";
            }

            Console.WriteLine("Specs complete.");
            if (directoriesToCreate.Count > 0)
            {
                Console.WriteLine($"{directoriesToCreate.Count} new directories identified.");
            }

            var key = string.Empty;
            var count = 1;

            var first = true;

            var (left, top) = Console.GetCursorPosition();
            var progress = 0;
            foreach (var file in specs.OrderBy(s => s.Key))
            {
                progress++;
                var pct = progress * 100 / specs.Count;
                var remaining = 100 - pct;
                if (!config.ReadOnly)
                {
                    Console.SetCursorPosition(left, top);
                    Console.Write("[");
                    if (pct > 0)
                    {
                        Console.Write(new string('#', pct));
                    }

                    if (remaining > 0)
                    {
                        Console.Write(new string('-', remaining));
                    }

                    Console.WriteLine($"] ({pct}% complete)");
                }

                if (key != file.Key)
                {
                    key = file.Key;
                    count = 1;
                }

                var dir = file.Directories.Length > 0 ?
                    Path.Combine(config.TargetDirectory, file.Directories[file.Directories.Length - 1]) :
                    config.TargetDirectory;

                var fileName = $"{Path.GetFileNameWithoutExtension(file.NewFile)}_{count++}{Path.GetExtension(file.NewFile)}";
                file.NewFile = Path.Combine(dir, fileName);

                if (!config.ReadOnly)
                {
                    if (first)
                    {
                        first = false;
                        if (!config.ReadOnly)
                        {
                            foreach (var dirToCreate in directoriesToCreate.OrderBy(d => d))
                            {
                                if (!Directory.Exists(dirToCreate))
                                {
                                    Directory.CreateDirectory(dirToCreate);
                                    var (left1, top1) = Console.GetCursorPosition();
                                    Console.Write($"Created directory {dirToCreate}");
                                    Console.SetCursorPosition(left1, top1);
                                }
                            }
                        }
                    }

                    var seq = 0;
                    var target = file.NewFile;
                    do
                    {
                        if (File.Exists(target))
                        {
                            seq++;
                            var seqStr = string.Format("{0:00}", seq);
                            var targetDir = Path.GetDirectoryName(file.NewFile);
                            target = Path.Combine(
                                targetDir,
                                $"{Path.GetFileNameWithoutExtension(file.NewFile)}({seqStr}){Path.GetExtension(file.NewFile)}");
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (seq < 100);

                    if (File.Exists(target))
                    {
                        throw new InvalidOperationException($"Unable to create target {file.NewFile} for {file.File}.");
                    }

                    if (config.MoveFile)
                    {
                        File.Move(file.File, target);
                    }
                    else
                    {
                        File.Copy(file.File, target);
                    }

                    file.NewFile = target;
                }

                Console.WriteLine($"{file.File} => {file.NewFile}");
            }

            if (config.ShowStatistics)
            {
                Console.WriteLine(statistics.ToString());
            }
        }
    }
}
