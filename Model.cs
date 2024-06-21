namespace Models;

public record User
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}