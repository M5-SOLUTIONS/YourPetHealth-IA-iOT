using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace YourPetHealth.Diagnostics
{
    public static class AppDiagnostics
    {
        public const string ServiceName = "YourPetHealth";

        public static readonly ActivitySource ActivitySource = new(ServiceName);

        private static readonly Meter Meter = new(ServiceName);

        public static readonly Counter<long> LoginsRealizados =
            Meter.CreateCounter<long>("yourpethealth.logins.total", description: "Total de logins bem-sucedidos");

        public static readonly Counter<long> LoginsFalhos =
            Meter.CreateCounter<long>("yourpethealth.logins.falhos.total", description: "Total de tentativas de login com falha");

        public static readonly Counter<long> CadastrosResponsavel =
            Meter.CreateCounter<long>("yourpethealth.responsaveis.cadastrados.total", description: "Total de responsáveis cadastrados");
    }
}
