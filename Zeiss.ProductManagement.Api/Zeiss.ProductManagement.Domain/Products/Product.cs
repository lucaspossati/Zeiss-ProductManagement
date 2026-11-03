namespace Zeiss.ProductManagement.Domain.Products;

public class Product
{
    public int Id { get; private set; }
    public int ProductIdentifier { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int AvailableStock { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Product()
    {
    }

    public Product(int productIdentifier, string name, string? description, decimal price, int availableStock)
    {
        if (productIdentifier < 100000 || productIdentifier > 999999)
        {
            throw new ArgumentException("Product identifier must be a six-digit number.", nameof(productIdentifier));
        }

        Name = ValidateName(name);
        Description = description;
        Price = ValidatePrice(price);
        AvailableStock = ValidateStock(availableStock);
        ProductIdentifier = productIdentifier;
        var now = DateTime.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void Update(string name, string? description, decimal price, int availableStock)
    {
        if (availableStock <= 0)
        {
            throw new InvalidStockOperationException("Stock quantity must be greater than zero.");
        }

        Name = ValidateName(name);
        Description = description;
        Price = ValidatePrice(price);
        AvailableStock = ValidateStock(availableStock);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidStockOperationException("Stock quantity must be greater than zero.");
        }

        AvailableStock += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void DecrementStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidStockOperationException("Stock quantity must be greater than zero.");
        }

        if (AvailableStock < quantity)
        {
            throw new InsufficientStockException($"Insufficient stock for product '{Name}'. Requested: {quantity}, available: {AvailableStock}.");
        }

        AvailableStock -= quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static string ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProductDomainException("Product name is required.", nameof(Name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 200)
        {
            throw new ProductDomainException("Product name must be between 2 and 200 characters long.", nameof(Name));
        }

        return trimmed;
    }

    public static decimal ValidatePrice(decimal price)
    {
        if (price <= 0m)
        {
            throw new ProductDomainException("Product price must be greater than zero.", nameof(Price));
        }

        return Math.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    public static int ValidateStock(int stock)
    {
        if (stock < 0)
        {
            throw new ProductDomainException("Available stock cannot be negative.", nameof(AvailableStock));
        }

        return stock;
    }
}
