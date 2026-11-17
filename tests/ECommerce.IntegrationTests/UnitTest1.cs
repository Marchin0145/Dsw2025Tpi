using FluentAssertions;

namespace ECommerce.IntegrationTests;

[Collection("IntegrationTests")]
public class DummyIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DummyIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task IntegrationTestProject_ShouldDiscoverAndRun()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthcheck");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}