using HotChocolate.Execution.Processing;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace GraphQLSQLProcessExample.Core;

public static class GraphQLExtensions
{
    public static SelectedFields<T> SelectedFields<T>(this IResolverContext context)
    {
        if (context.Selection is not Selection selection)
        {
            return new SelectedFields<T>(Array.Empty<string>());
        }

        var fields = GetFields(selection.SelectionSet)
            .Select(o => o.ToLowerInvariant())
            .ToArray();

        return new SelectedFields<T>(fields);
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