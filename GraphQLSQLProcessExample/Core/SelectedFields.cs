using System.Runtime.CompilerServices;

namespace GraphQLSQLProcessExample.Core;

public class SelectedFields<T>(string[] selectedFields)
{
    public HashSet<string> Fields { get; } = new(selectedFields);

    public bool Contains(Func<T,object> selector, [CallerArgumentExpression(nameof(selector))] string selectorText = null!)
    {
        var dotIndex = selectorText.IndexOf('.');
        if (dotIndex > 0)
        {
            var text = selectorText.Substring(dotIndex + 1).ToLower();
            return Fields.Contains(text);
        }
        
        return Fields.Contains(selectorText);
    }
    
    public bool ContainsOnly(Func<T,object> selector, [CallerArgumentExpression(nameof(selector))] string selectorText = null!)
    {
        if (Fields.Count != 1)
        {
            return false;
        }
        
        return Contains(selector, selectorText);
    }
}