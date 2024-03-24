namespace GraphQLSQLProcessExample.Core;

public class Dictionary
{
    public static Dictionary<TKey, TValue?> Fill<TKey, TValue>(IReadOnlyList<TKey> keys)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue?>();
        foreach (var key in keys)
        {
            dictionary[key] = default;
        }
        
        return dictionary;
    }
}