// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraTouch
{
    /// <summary>
    /// Property statistics roll-up.
    /// </summary>
    public class Statistics
    {
        private readonly PropertyManager propertyManager;

        private readonly IDictionary<string, IDictionary<string, int>> stats =
            new Dictionary<string, IDictionary<string, int>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Statistics"/> class.
        /// </summary>
        /// <param name="propertyManager">The <seealso cref="PropertyManager"/> instance.</param>
        public Statistics(PropertyManager propertyManager) =>
            this.propertyManager = propertyManager;

        /// <summary>
        /// Aggregates the file statistics.
        /// </summary>
        /// <param name="file">The <see cref="FileSpec"/> to parse.</param>
        public void Aggregate(FileSpec file)
        {
            foreach (var prop in file.Properties)
            {
                if (propertyManager.IsExcluded(prop.Code))
                {
                    continue;
                }

                if (!stats.ContainsKey(prop.Code))
                {
                    stats.Add(prop.Code, new Dictionary<string, int>());
                }

                if (!stats[prop.Code].ContainsKey(prop.Value))
                {
                    stats[prop.Code].Add(prop.Value, 0);
                }

                stats[prop.Code][prop.Value]++;
            }
        }

        /// <summary>
        /// String representation of statistics.
        /// </summary>
        /// <returns>The statistics report.</returns>
        public override string ToString()
        {
            const string divider = "=========";
            const string heading = "Property\t\tValue\t\tCount";

            var sb = new StringBuilder("Top 10:");
            sb.AppendLine();
            sb.AppendLine(divider);
            sb.AppendLine(heading);

            var top10 = ((Dictionary<string, IDictionary<string, int>>)stats)
                .SelectMany(s => s.Value, (key, value) => new { code = key.Key, value = value.Key, count = value.Value })
                .OrderByDescending(s => s.count)
                .ThenBy(s => s.code)
                .Take(10);

            foreach (var stat in top10)
            {
                sb.AppendLine($"{propertyManager.PropertyByCode(stat.code).Name}\t\t{stat.value}\t\t{stat.count}");
            }

            sb.AppendLine(divider);
            sb.AppendLine("Statistics:");
            sb.AppendLine(divider);
            sb.AppendLine(heading);

            foreach (var property in stats.Keys.OrderBy(k => propertyManager.PropertyByCode(k).Name))
            {
                foreach (var value in stats[property].Keys.OrderBy(k1 => k1))
                {
                    var count = stats[property][value];
                    if (count > 1)
                    {
                        sb.AppendLine($"{propertyManager.PropertyByCode(property).Name}\t\t{value}\t\t{count}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
