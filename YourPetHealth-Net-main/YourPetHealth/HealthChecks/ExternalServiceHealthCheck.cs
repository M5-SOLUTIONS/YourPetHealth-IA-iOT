using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace YourPetHealth.HealthChecks
{
    public class ExternalServiceHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ExternalServiceHealthCheck(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var url = _configuration["HealthChecks:ExternalServiceUrl"];
            if (string.IsNullOrWhiteSpace(url))
            {
                return HealthCheckResult.Healthy("Nenhum serviço externo configurado para checagem.");
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    ["url"] = url,
                    ["statusCode"] = (int)response.StatusCode,
                    ["responseTimeMs"] = stopwatch.ElapsedMilliseconds
                };

                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy($"Serviço externo respondeu {(int)response.StatusCode}.", data)
                    : HealthCheckResult.Degraded($"Serviço externo respondeu {(int)response.StatusCode}.", data: data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Não foi possível acessar {url}.", ex);
            }
        }
    }
}
