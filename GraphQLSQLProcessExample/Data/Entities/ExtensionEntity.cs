using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQLSQLProcessExample.Data.Entities;

public class ExtensionEntity : Entity
{
    public string Name { get; set; }
    
    public Guid ProcessId { get; set; }
    
    [ForeignKey("ProcessId")]
    public ProcessEntity Process { get; set; }
}