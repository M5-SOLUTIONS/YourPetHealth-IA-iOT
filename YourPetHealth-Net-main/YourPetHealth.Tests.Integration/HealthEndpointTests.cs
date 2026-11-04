using System.Net;
using System.Text.Json;
using YourPetHealth.Tests.Integration.Fixtures;
using Xunit;

namespace YourPetHealth.Tests.Integration
{
    [Collection("Integration")]
    public class HealthEndpointTests
    {
        private readonly WebAppFactoryFixture _factory;

        public HealthEndpointTests(WebAppFactoryFixture factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Health_RetornaJsonComStatusEChecksDoOracleEDoServicoExterno()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/health");
            var conteudo = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable);

            using var json = JsonDocument.Parse(conteudo);
            var checks = json.RootElement.GetProperty("checks");
            var nomes = checks.EnumerateArray().Select(c => c.GetProperty("name").GetString()).ToList();

            Assert.Contains("oracle_database", nomes);
            Assert.Contains("external_service", nomes);
        }

        [Fact]
        public async Task Get_HealthReady_RetornaApenasChecksMarcadosComATagReady()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/health/ready");
            var conteudo = await response.Content.ReadAsStringAsync();

            Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable);
            Assert.Contains("oracle_database", conteudo);
        }
    }
}
