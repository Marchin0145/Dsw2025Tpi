using Dsw2025Tpi.Domain.Entities;

namespace ECommerce.Testing.Builders;

internal sealed record CustomerBuilder
{
    public string UserName { get; init; } = "cliente@tp4.example";
    public string Name { get; init; } = "Cliente TP4";
    public string? Email { get; init; }
    public string PhoneNumber { get; init; } = "3815550101";

    public Customer Build() => new(UserName, Name, Email ?? UserName, PhoneNumber);
}
