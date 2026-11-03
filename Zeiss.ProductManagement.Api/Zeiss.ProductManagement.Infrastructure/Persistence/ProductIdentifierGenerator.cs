using Microsoft.EntityFrameworkCore;
using Zeiss.ProductManagement.Application.Abstractions;

namespace Zeiss.ProductManagement.Infrastructure.Persistence;

public sealed class ProductIdentifierGenerator : IProductIdentifierGenerator
{
    private readonly AppDbContext _dbContext;

    public ProductIdentifierGenerator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GenerateAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.IsRelational())
        {
            var sequence = await _dbContext.ProductIdentifierSequences.SingleOrDefaultAsync(cancellationToken);

            if (sequence is null)
            {
                var maxIdentifier = await _dbContext.Products
                    .Select(product => (int?)product.ProductIdentifier)
                    .MaxAsync(cancellationToken);
                sequence = new ProductIdentifierSequence
                {
                    CurrentValue = maxIdentifier is null or >= 999999
                        ? 100000
                        : maxIdentifier.Value + 1
                };
                _dbContext.ProductIdentifierSequences.Add(sequence);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var currentValue = sequence.CurrentValue;
            sequence.CurrentValue = sequence.CurrentValue >= 999999 ? 100000 : sequence.CurrentValue + 1;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return currentValue;
        }

        var inMemorySequence = await _dbContext.ProductIdentifierSequences.SingleOrDefaultAsync(cancellationToken);
        if (inMemorySequence is null)
        {
            var maxIdentifier = await _dbContext.Products
                .Select(product => (int?)product.ProductIdentifier)
                .MaxAsync(cancellationToken);
            inMemorySequence = new ProductIdentifierSequence
            {
                CurrentValue = maxIdentifier is null or >= 999999
                    ? 100000
                    : maxIdentifier.Value + 1
            };
            _dbContext.ProductIdentifierSequences.Add(inMemorySequence);
        }

        var value = inMemorySequence.CurrentValue;
        inMemorySequence.CurrentValue = inMemorySequence.CurrentValue >= 999999 ? 100000 : inMemorySequence.CurrentValue + 1;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return value;
    }
}
