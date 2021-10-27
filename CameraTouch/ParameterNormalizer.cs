// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace CameraTouch
{
    /// <summary>
    /// Ensures only one copy of each parameter exists.
    /// </summary>
    public class ParameterNormalizer : ExpressionVisitor
    {
        private readonly List<ParameterExpression> parameters = new ();

        /// <summary>
        /// Visit a parameter.
        /// </summary>
        /// <param name="node">The <see cref="ParameterExpression"/>.</param>
        /// <returns>The normalized parameter.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            var match = parameters.Where(p => p.Type == node.Type && p.Name == node.Name)
                .FirstOrDefault();
            if (match == null)
            {
                var result = base.VisitParameter(node);
                parameters.Add((ParameterExpression)result);
                return result;
            }

            return match;
        }
    }
}
