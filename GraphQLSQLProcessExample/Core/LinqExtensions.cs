namespace GraphQLSQLProcessExample.Core;

public static class LinqExtensions
{
    public static Dictionary<TKey, TValue?> ToFullDictionary<TKey, TValue>(
        this IReadOnlyList<TKey> keys,
        IEnumerable<TValue> values, 
        Func<TValue, TKey> selector)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue?>();
        foreach (var key in keys)
        {
            dictionary[key] = default;
        }
        
        foreach (var value in values)
        {
            var key = selector(value);
            dictionary[key] = value;
        }
        
        return dictionary;
    }
    
    public static string Join(this IEnumerable<string> values, string separator = ", ")
    {
        return string.Join(separator, values);
    }
    
    public static string JoinLines(this IEnumerable<string> values, string separator = ", ")
    {
        var newSeparator = $"{separator}{Environment.NewLine}";
        return string.Join(newSeparator, values);
    }
}