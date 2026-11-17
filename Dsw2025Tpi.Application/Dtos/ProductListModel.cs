using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Dtos;

public record ProductListModel(
    IReadOnlyList<Product> ProductItems,
    int TotalItems,
    int PageNumber,
    int PageSize);
