namespace SchoolCanteensDomain;

// Parent.cs
public class Parent
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal WalletBalance { get; set; }
    public List<Student> Students { get; set; } = new();
}