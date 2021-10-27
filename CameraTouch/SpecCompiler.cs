// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace CameraTouch
{
    /// <summary>
    /// Compiles a spec to generator code.
    /// </summary>
    public class SpecCompiler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecCompiler"/> class.
        /// </summary>
        /// <param name="propertyManager">The <see cref="PropertyManager"/>.</param>
        /// <param name="spec">The spec to parse.</param>
        public SpecCompiler(PropertyManager propertyManager, string spec)
        {
            var concat = typeof(string).GetMethods()
                    .Where(m => m.Name == nameof(string.Concat)
                    && m.IsGenericMethod == false
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(string)
                    && m.GetParameters()[1].ParameterType == typeof(string))
                    .FirstOrDefault();

            Expression<Func<FileSpec, string>> Combine(
                Expression<Func<FileSpec, string>> oldExpr,
                Expression<Func<FileSpec, string>> newExpr)
            {
                var invokeOld = Expression.Invoke(oldExpr, oldExpr.Parameters);
                var invokeNew = Expression.Invoke(newExpr, oldExpr.Parameters);
                var call = Expression.Call(concat, new Expression[] { invokeOld, invokeNew });
                var lambda = Expression.Lambda<Func<FileSpec, string>>(call, oldExpr.Parameters);
                return lambda;
            }

            var idx = 0;
            string word = string.Empty;
            string code = string.Empty;
            bool parseCode = false;
            string datePattern = "yyyy-MM-dd";
            bool parseDate = false;
            Expression<Func<FileSpec, string>> expr = f => string.Empty;
            while (idx < spec.Length)
            {
                var chr = spec[idx++];
                if (parseDate)
                {
                    if (chr == '[')
                    {
                        continue;
                    }

                    if (chr != ']')
                    {
                        datePattern += chr;
                        continue;
                    }
                }

                if (parseCode)
                {
                    if (!parseDate)
                    {
                        code += chr;
                    }

                    if (code.Length == 2)
                    {
                        if (code == "dt")
                        {
                            if (parseDate)
                            {
                                parseDate = false;
                                Func<string, Expression<Func<FileSpec, string>>> getDate = (string dp) => (FileSpec f) =>
                                    ((DateTime)f.Properties.Where(p => p.Code == "dt").First().ParsedValue).ToString(dp);
                                var dateExtract = getDate(datePattern);
                                dateExtract = dateExtract.Update(dateExtract.Body, expr.Parameters);
                                expr = Combine(expr, dateExtract);
                                code = datePattern = string.Empty;
                                parseCode = false;
                                continue;
                            }

                            var check = spec[idx];
                            if (check == '[')
                            {
                                parseDate = true;
                                datePattern = string.Empty;
                                continue;
                            }
                        }

                        Func<string, Expression<Func<FileSpec, string>>> getExtract = (string cd) => (FileSpec f) =>
                            (f.Properties.Where(p => p.Code == cd).Select(p => new { p.FileValue }).FirstOrDefault() ??
                            new { FileValue = string.Empty }).FileValue;
                        var extract = getExtract(code);
                        extract = extract.Update(extract.Body, expr.Parameters);
                        expr = Combine(expr, extract);
                        code = string.Empty;
                        parseCode = false;
                    }

                    continue;
                }

                if (chr == '$')
                {
                    if (!string.IsNullOrEmpty(word))
                    {
                        var wordExpr = Expression.Constant(word);
                        var wordExprWrapper = Expression.Lambda<Func<FileSpec, string>>(wordExpr, expr.Parameters);
                        expr = Combine(expr, wordExprWrapper);
                        word = string.Empty;
                    }

                    parseCode = true;
                }
                else
                {
                    word += chr;
                }
            }

            if (!string.IsNullOrEmpty(word))
            {
                var wordExpr = Expression.Constant(word);
                var wordExprWrapper = Expression.Lambda<Func<FileSpec, string>>(wordExpr, expr.Parameters);
                expr = Combine(expr, wordExprWrapper);
            }

            var normalizer = new ParameterNormalizer();
            GetNameForSpec = ((Expression<Func<FileSpec, string>>)normalizer.Visit(expr)).Compile();
        }

        /// <summary>
        /// Gets the function that generates the name.
        /// </summary>
        public Func<FileSpec, string> GetNameForSpec { get; private set; }
    }
}
