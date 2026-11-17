using System.Linq.Expressions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Moq;

namespace Dsw2025Tpi.UnitTests.Mocks;

internal static class OrderRepositoryMock
{
    public static Mock<IRepository> Create(Customer? customer, IEnumerable<Product> products)
    {
        var productsById = products.ToDictionary(product => product.Id);
        var repository = new Mock<IRepository>(MockBehavior.Strict);
        repository.Setup(repo => repo.First<Customer>(
                It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync((Expression<Func<Customer, bool>> predicate, string[] _) =>
                customer is not null && predicate.Compile()(customer) ? customer : null);
        repository.Setup(repo => repo.GetById<Product>(It.IsAny<Guid>(), It.IsAny<string[]>()))
            .ReturnsAsync((Guid id, string[] _) => productsById.GetValueOrDefault(id));
        repository.Setup(repo => repo.Update(It.IsAny<Product>()))
            .ReturnsAsync((Product product) => product);
        repository.Setup(repo => repo.Add(It.IsAny<Order>()))
            .ReturnsAsync((Order order) => order);

        return repository;
    }
}
