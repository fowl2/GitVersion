namespace GitVersion.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> getValue)
    {
        ArgumentNullException.ThrowIfNull(dict);
        ArgumentNullException.ThrowIfNull(getValue);

        if (!dict.TryGetValue(key, out var value))
        {
            value = getValue();
            dict.Add(key, value);
        }
        return value;
    }

#if !NET8_0_OR_GREATER
    internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        where TKey : notnull
#if NET
        => new(source);
#else
        => MoreLinq.MoreEnumerable.ToDictionary(source);
#endif
#endif
}
