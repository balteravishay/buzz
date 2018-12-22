using Buzz.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Buzz.Extensions
{
    /// <summary>
    /// Extensions methods used for manipulation of partitions and enumerators.
    /// </summary>
    internal static class ScaleExtensions
    {
        /// <summary>
        /// Partitions sum into groups with max elements per group.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="partitionSize"></param>
        /// <returns></returns>
        internal static IEnumerable<Tuple<int, int>> PartitionSum(this int @this, NonZeroInt partitionSize) =>
            Enumerable.Range(0,@this)
            .GroupBy(index => index/partitionSize.IntValue)
            .Select(index => Tuple.Create(index.Key, index.Count()));

        /// <summary>
        /// Gets a source IEnumerable and returns a sequence of interleaved calls to two functions per each element.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="this"></param>
        /// <param name="firstFunction"></param>
        /// <param name="secondFunction"></param>
        /// <returns></returns>
        internal static IEnumerable<Func<TResponse>> MakeInterleavedCalls<TInput, TResponse>(this IEnumerable<TInput> @this, Func<TInput, TResponse> firstFunction, Func<TInput, TResponse> secondFunction)
        {
            using (var e = @this.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    TInput value;
                    for (value = e.Current; e.MoveNext(); value = e.Current)
                    {
                        var iterValue = value;
                        yield return () => firstFunction(iterValue);
                        yield return () => secondFunction(iterValue);
                    }
                    yield return () => firstFunction(value);
                }
            }
        }
    }
}
