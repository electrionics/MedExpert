using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MedExpert.Core.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<TQuery> In<TKey, TQuery>(
            this IQueryable<TQuery> queryable,
            IEnumerable<TKey> values,
            Expression<Func<TQuery, TKey>> keySelector)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (!values.Any())
            {
                return queryable.Take(0);
            }
            
            var distinctValues = Bucketize(values);

            if (distinctValues.Length > 2048)
            {
                throw new ArgumentException("Too many parameters for SQL Server, reduce the number of parameters", nameof(keySelector));
            }
            
            var predicates = distinctValues
                .Select(v =>
                {
                    // Create an expression that captures the variable so EF can turn this into a parameterized SQL query
                    Expression<Func<TKey>> valueAsExpression = () => v;
                    return Expression.Equal(keySelector.Body, valueAsExpression.Body);
                })
                .ToList();

            while (predicates.Count > 1)
            {
                predicates = PairWise(predicates).Select(p => Expression.OrElse(p.Item1, p.Item2)).ToList();
            }

            var body = predicates.Single();

            var clause = Expression.Lambda<Func<TQuery, bool>>(body, keySelector.Parameters);

            return queryable.Where(clause);
        }

        /// <summary>
        /// Break a list of items tuples of pairs.
        /// </summary>
        private static IEnumerable<(T, T)> PairWise<T>(this IEnumerable<T> source)
        {
            var sourceEnumerator = source.GetEnumerator();
            while (sourceEnumerator.MoveNext())
            {
                var a = sourceEnumerator.Current;
                sourceEnumerator.MoveNext();
                var b = sourceEnumerator.Current;

                yield return (a, b);
            }
        }

        private static TKey[] Bucketize<TKey>(IEnumerable<TKey> values)
        {
            var distinctValueList = values.Distinct().ToList();

            // Calculate bucket size as 1,2,4,8,16,32,64,...
            var bucket = 1;
            while (distinctValueList.Count > bucket)
            {
                bucket *= 2;
            }

            // Fill all slots.
            var lastValue = distinctValueList.Last();
            for (var index = distinctValueList.Count; index < bucket; index++)
            {
                distinctValueList.Add(lastValue);
            }

            var distinctValues = distinctValueList.ToArray();
            return distinctValues;
        }
    }
}