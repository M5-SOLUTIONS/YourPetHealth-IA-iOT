using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace YourPetHealth.Tests.Integration.Fixtures
{
    public class WebAppFactoryFixture : WebApplicationFactory<Program>
    {
    }

    [CollectionDefinition("Integration")]
    public class IntegrationTestsCollection : ICollectionFixture<WebAppFactoryFixture>
    {
    }
}
