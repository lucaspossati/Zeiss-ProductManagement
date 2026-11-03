namespace Zeiss.ProductManagement.Infrastructure.Persistence;

public sealed class ProductIdentifierSequence
{
    public int Id { get; set; }
    public int CurrentValue { get; set; } = 100000;
}
