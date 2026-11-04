using System.Text;
using System.Text.Json;

namespace YourPetHealth.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        public async Task<string> ConsultarIaAsync(string mensagemTutor, string dadosTelemetria)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "Erro: Chave da API do Gemini não configurada no appsettings.json.";
            }

            var promptSystem = $@"Você é um assistente de triagem veterinária inteligente e empático da plataforma YourPetHealth.
Analise a dúvida do tutor do pet considerando os últimos dados de telemetria coletados.

Dados de Telemetria Recentes:
{dadosTelemetria}

Pergunta do Tutor:
{mensagemTutor}

Instruções:
- Seja conciso, claro e reconfortante.
- Correlacione a queixa do tutor com os batimentos e temperatura caso façam sentido.
- Dê orientações práticas e sempre lembre que a consulta presencial com veterinário é fundamental se houver agravamento.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptSystem }
                        }
                    }
                }
            };

            // Usando exatamente o modelo recomendado pela mensagem de erro da API
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={_apiKey}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                request.Headers.Add("User-Agent", "YourPetHealth-App");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var erroDetalhes = await response.Content.ReadAsStringAsync();
                    return $"Erro da API Gemini ({response.StatusCode}): {erroDetalhes}";
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                var respostaTexto = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return respostaTexto ?? "Sem resposta disponível.";
            }
            catch (TaskCanceledException)
            {
                return "Erro de Timeout: A ligação demorou demasiado tempo a responder.";
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "";
                return $"Erro detalhado na comunicação: {ex.Message}{innerMsg}";
            }
        }
    }
}