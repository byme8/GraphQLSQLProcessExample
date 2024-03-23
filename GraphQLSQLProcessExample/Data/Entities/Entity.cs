namespace GraphQLSQLProcessExample.Data.Entities;

public class Entity
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}