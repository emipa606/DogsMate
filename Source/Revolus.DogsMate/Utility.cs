using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Revolus.DogsMate
{
    public static class Utility
    {
        public static IReadOnlyDictionary<K, V> AsReadOnlyDict<K, V>(this IReadOnlyDictionary<K, V> me)
        {
            return me;
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyList<T> me)
        {
            return me;
        }

        public static IReadOnlyCollection<T> AsReadOnlyArray<T>(this T[] me)
        {
            return me;
        }

        public static bool TryGetRandomElement<T>(this IReadOnlyList<T> list, out T element)
        {
            if (list is null || list.Count == 0)
            {
                element = default;
                return false;
            }

            element = list[Rand.Range(0, list.Count)];
            return true;
        }

        public static IEnumerable<(T, T)> Cross<T>(this IEnumerable<T> enumable)
        {
            var list = enumable as IReadOnlyList<T> ?? enumable.ToList();
            foreach (var a in list)
            {
                foreach (var b in list)
                {
                    yield return (a, b);
                }
            }
        }

        public static bool TryGetRandomValue(this SimpleCurve curve, out float result)
        {
            result = default;
            switch (curve?.PointsCount ?? 0)
            {
                case 0:
                    return false;
                case 1:
                    if (curve != null)
                    {
                        result = curve[0].y;
                    }

                    return true;
                default:
                    if (curve != null)
                    {
                        result = curve.Evaluate(Rand.Value);
                    }

                    return true;
            }
        }
    }
}