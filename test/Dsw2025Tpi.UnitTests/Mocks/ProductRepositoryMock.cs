using System.Linq.Expressions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Moq;

namespace Dsw2025Tpi.UnitTests.Mocks;

internal static class ProductRepositoryMock
{
    public static Mock<IRepository> Create(params Product[] products)
    {
        var repository = new Mock<IRepository>(MockBehavior.Strict);
        repository.Setup(repo => repo.First<Product>(
                It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync((Expression<Func<Product, bool>> predicate, string[] _) =>
                products.FirstOrDefault(predicate.Compile()));
        repository.Setup(repo => repo.GetAll<Product>(It.IsAny<string[]>()))
            .ReturnsAsync(products);
        repository.Setup(repo => repo.Add(It.IsAny<Product>()))
            .ReturnsAsync((Product product) => product);

        return repository;
    }
}
