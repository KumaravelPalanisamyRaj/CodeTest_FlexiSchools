namespace SchoolCanteensDomain;

// Student.cs
public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = null!;
    public List<string> Allergens { get; set; } = new();
}