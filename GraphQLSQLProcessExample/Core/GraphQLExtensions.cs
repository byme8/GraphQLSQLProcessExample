using System.Runtime.CompilerServices;
using HotChocolate.Execution.Processing;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace GraphQLSQLProcessExample.Core;

public class SelectedFields(string[] selectedFields)
{
    public HashSet<string> Fields { get; } = new(selectedFields);

    public bool Contains(string value, [CallerArgumentExpression(nameof(value))] string selector = null!)
    {
        var dotIndex = selector.IndexOf('.');
        if (dotIndex > 0)
        {
            var text = selector.Substring(dotIndex).ToLower();
            return Fields.Contains(text);
        }
        
        return Fields.Contains(selector);
    }
    
    public bool ContainsOnly<T>(T value, [CallerArgumentExpression(nameof(value))] string selector = null!)
    {
        if (Fields.Count != 1)
        {
            return false;
        }
        
        return Contains(value, selector);
    }
}

public static class GraphQLExtensions
{
    public static SelectedFields SelectedFields(this IResolverContext context)
    {
        if (context.Selection is not Selection selection)
        {
            return new SelectedFields(Array.Empty<string>());
        }

        var fields = GetFields(selection.SelectionSet)
            .Select(o => o.ToLowerInvariant())
            .ToArray();

        return new SelectedFields(fields);
    }
    
    private static IEnumerable<string> GetFields(SelectionSetNode? selectionSelectionSet, string? parentField = null)
    {
        var fieldNodes = selectionSelectionSet?.Selections.OfType<FieldNode>() ?? Array.Empty<FieldNode>();
        foreach (var selection in fieldNodes)
        {
            var newParentField = string.IsNullOrEmpty(parentField)
                ? selection.Name.Value
                : $"{parentField}.{selection.Name.Value}";
            yield return newParentField;

            if (selection.SelectionSet is not null)
            {
                foreach (var field in GetFields(selection.SelectionSet, newParentField))
                {
                    yield return field;
                }
            }
        }
    }
}