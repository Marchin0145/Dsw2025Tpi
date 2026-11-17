using FluentAssertions;

namespace ECommerce.UnitTests;

public class DummyTests
{
    [Fact]
    public void UnitTestProject_ShouldDiscoverAndRun()
    {
        true.Should().BeTrue();
    }
}