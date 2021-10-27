// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CameraTouch
{
    /// <summary>
    /// Handles parsing of properties.
    /// </summary>
    public class PropertyManager
    {
        /// <summary>
        /// Codes to exclude from statistics.
        /// </summary>
        private readonly string[] excludes = new[] { "dt", "fn", "sz" };

        /// <summary>
        /// A dictionary of properties mapped to name.
        /// </summary>
        private readonly IDictionary<string, CameraPropertyBase> nameMapping = new Dictionary<string, CameraPropertyBase>();

        /// <summary>
        /// A dictionary of properties mapped to code.
        /// </summary>
        private readonly IDictionary<string, CameraPropertyBase> codeMapping = new Dictionary<string, CameraPropertyBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyManager"/> class.
        /// </summary>
        public PropertyManager()
        {
            var properties = new CameraPropertyBase[]
            {
                new CameraProperty<string>("cp", "Compression", str => str, str => str),
                new CameraProperty<string>("mk", "Make", str => str, str => str),
                new CameraProperty<string>("md", "Model", str => str, str => str),
                new CameraProperty<string>("or", "Orientation", str => str, str => str),
                new CameraProperty<int>("xr", "X Resolution", ExtractFirst<int>, i => i.ToString()),
                new CameraProperty<int>("yr", "Y Resolution", ExtractFirst<int>, i => i.ToString()),
                new CameraProperty<string>("ru", "Resolution Unit", str => str, str => str),
                new CameraProperty<string>("sf", "Software", str => str, str => str),
                new CameraProperty<DateTime>("dt", "Date/Time", ParseDateTime, dt => $"{dt.ToShortDateString()} {dt.ToShortTimeString()}"),
                new CameraProperty<int>("wd", "Image Width", ExtractFirst<int>, i => i.ToString()),
                new CameraProperty<int>("ht", "Image Height", ExtractFirst<int>, i => i.ToString()),
                new CameraProperty<string>("it", "Photometric Interpretation", str => str, str => str),
                new CameraProperty<string>("cf", "CFA Pattern", ExtractCFA, str => str),
                new CameraProperty<TimeSpan>("et", "Exposure Time", ParseDuration, ShowDuration),
                new CameraProperty<double>("fs", "F-Number", str => double.Parse(str[(str.IndexOf('/') + 1) ..]), fs => $"f{fs}"),
                new CameraProperty<int>("is", "ISO Speed Ratings", str => int.Parse(str), iso => $"iso{iso}"),
                new CameraProperty<int>("fl", "Focal Length", str => int.Parse(str.Split(' ')[0]), fl => $"{fl}mm"),
                new CameraProperty<string>("ls", "Lens Specification", str => str, str => str),
                new CameraProperty<string>("lm", "Lens Model", str => str, str => str),
                new CameraProperty<string>("ft", "Detected File Type Name", str => str, str => str),
                new CameraProperty<string>("fd", "Detected File Type Long Name", str => str, str => str),
                new CameraProperty<string>("ex", "Expected File Name Extension", str => str, str => str),
                new CameraProperty<string>("fn", "File Name", str => str, str => str),
                new CameraProperty<long>("sz", "File Size", str => long.Parse(str.Split(' ')[0]), fs => fs.ToString()),
            };

            var sb = new StringBuilder();
            sb.AppendLine("The following codes are valid for the file and directory specs. Sequence numbers are automatically added.");
            sb.AppendLine();

            foreach (var property in properties.OrderBy(p => p.Code))
            {
                if (property.Code == "dt")
                {
                    sb.AppendLine("$dt[format]\tDate/Time (format follows the C# data format specification)");
                }
                else
                {
                    sb.AppendLine($"${property.Code}\t\t{property.Name}");
                }

                nameMapping.Add(property.Name, property);
                codeMapping.Add(property.Code, property);
            }

            OptionsText = sb.ToString();
        }

        /// <summary>
        /// Gets the help text for options.
        /// </summary>
        public string OptionsText { get; private set; }

        /// <summary>
        /// Returns the property referenced by name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>The <see cref="CameraPropertyBase"/> implementation.</returns>
        public CameraPropertyBase this[string name]
        {
            get => nameMapping.ContainsKey(name) ? nameMapping[name] : null;
        }

        /// <summary>
        /// Gets a value indicating whether the code is excluded from statistics.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The exclusion status.</returns>
        public bool IsExcluded(string code) => excludes.Contains(code);

        /// <summary>
        /// Gets the property lookup by code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The property object.</returns>
        public CameraPropertyBase PropertyByCode(string code) => codeMapping.ContainsKey(code) ?
            codeMapping[code] : null;

        /// <summary>
        /// String representation.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var val = new StringBuilder();
            var list =
                string.Join(";", nameMapping.Keys.Select(key => nameMapping[key].ToString()));
            return list;
        }

        /// <summary>
        /// Splits and extracts first item from string.
        /// </summary>
        /// <typeparam name="T">The type of convert to.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        private static T ExtractFirst<T>(string value)
            where T : IConvertible
        {
            var parts = value.Split(' ');
            return (T)Convert.ChangeType(parts[0], typeof(T));
        }

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to hh:mm:ss format.
        /// </summary>
        /// <param name="duration">The duration to parse.</param>
        /// <returns>The string equivalent.</returns>
        private string ShowDuration(TimeSpan duration)
        {
            var result = new StringBuilder();
            if (duration.TotalHours >= 1.0)
            {
                var h = Math.Floor(duration.TotalHours);
                result.Append($"{h}h");
                duration -= TimeSpan.FromHours(h);
            }

            if (duration.TotalMinutes >= 1.0)
            {
                var m = Math.Floor(duration.TotalMinutes);
                result.Append($"{m}m");
                duration -= TimeSpan.FromMinutes(m);
            }

            var s = duration.TotalSeconds;
            result.Append($"{s}s");
            return result.ToString();
        }

        /// <summary>
        /// Parses the camera property format to  a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="duration">The duration text.</param>
        /// <returns>The parsed <see cref="TimeSpan"/>.</returns>
        private TimeSpan ParseDuration(string duration)
        {
            var tokens = duration.Split(' ');
            var idx = 0;
            var result = TimeSpan.Zero;
            while (idx < tokens.Length)
            {
                var val = tokens[idx++];
                float value;
                if (val.IndexOf('/') > 0)
                {
                    var parts = val.Split('/');
                    value = float.Parse(parts[0]) / float.Parse(parts[1]);
                }
                else
                {
                    value = float.Parse(val);
                }

                var unit = tokens[idx++].ToLowerInvariant();
                switch (unit[0])
                {
                    case 'h':
                        result += TimeSpan.FromHours(value);
                        break;
                    case 'm':
                        result += TimeSpan.FromMinutes(value);
                        break;
                    case 's':
                        result += TimeSpan.FromSeconds(value);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the camera format date time.
        /// </summary>
        /// <param name="dateTime">The date time source.</param>
        /// <returns>The parsed <see cref="DateTime"/>.</returns>
        private DateTime ParseDateTime(string dateTime)
        {
            var dateAndTime = dateTime.Split(' ');
            var (date, time) = (dateAndTime[0], dateAndTime[1]);
            var separator = date.Contains(':') ? ':' : '-';
            var dateParts = date.Split(separator);
            var (year, month, day) = (int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]));
            var timeParts = time.Split(':');
            var (hour, min, sec) = (int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));
            return new DateTime(year, month, day, hour, min, sec);
        }

        /// <summary>
        /// Converts CFA from camera to standard.
        /// </summary>
        /// <param name="arg">The camera CFA string.</param>
        /// <returns>The converted string.</returns>
        private string ExtractCFA(string arg)
        {
            var result = arg.ToLower().Replace("red", "r")
                .Replace("green", "g")
                .Replace("blue", "b").ToUpper();
            return Regex.Replace(result, "[^RGB]+", string.Empty);
        }
    }
}
