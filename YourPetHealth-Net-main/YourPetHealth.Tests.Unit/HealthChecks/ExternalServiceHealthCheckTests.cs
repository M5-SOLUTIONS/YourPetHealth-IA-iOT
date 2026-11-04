using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Moq.Protected;
using YourPetHealth.HealthChecks;
using Xunit;

namespace YourPetHealth.Tests.Unit.HealthChecks
{
    public class ExternalServiceHealthCheckTests
    {
        private static HttpClient CriarHttpClientMockado(HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode));

            return new HttpClient(handlerMock.Object);
        }

        private static IConfiguration CriarConfiguracao(string? url)
        {
            var dict = new Dictionary<string, string?> { ["HealthChecks:ExternalServiceUrl"] = url };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        [Fact]
        public async Task CheckHealthAsync_ServicoRespondeOk_RetornaHealthy()
        {
            var httpClient = CriarHttpClientMockado(HttpStatusCode.OK);
            var config = CriarConfiguracao("https://servico-exemplo.com/status");
            var healthCheck = new ExternalServiceHealthCheck(httpClient, config);

            var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Healthy, resultado.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_ServicoRespondeErro500_RetornaDegraded()
        {
            var httpClient = CriarHttpClientMockado(HttpStatusCode.InternalServerError);
            var config = CriarConfiguracao("https://servico-exemplo.com/status");
            var healthCheck = new ExternalServiceHealthCheck(httpClient, config);

            var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Degraded, resultado.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_SemUrlConfigurada_RetornaHealthySemChamarHttp()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var config = CriarConfiguracao(null);
            var healthCheck = new ExternalServiceHealthCheck(httpClient, config);

            var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Healthy, resultado.Status);
            handlerMock.Protected().Verify(
                "SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}
