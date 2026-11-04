using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using YourPetHealth.Data;

namespace YourPetHealth.HealthChecks
{
    
    public class OracleDatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        public OracleDatabaseHealthCheck(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var podeConectar = await _context.Database.CanConnectAsync(cancellationToken);
                if (!podeConectar)
                {
                    return HealthCheckResult.Unhealthy("Não foi possível abrir conexão com o Oracle.");
                }

                await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM DUAL", cancellationToken);

                stopwatch.Stop();
                var data = new Dictionary<string, object>
                {
                    ["responseTimeMs"] = stopwatch.ElapsedMilliseconds,
                    ["dataSource"] = _context.Database.GetDbConnection().DataSource
                };

                return stopwatch.ElapsedMilliseconds > 1000
                    ? HealthCheckResult.Degraded("Oracle respondeu, mas acima de 1s.", data: data)
                    : HealthCheckResult.Healthy("Conexão com o Oracle OK.", data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Falha ao conectar no Oracle.", ex);
            }
        }
    }
}
