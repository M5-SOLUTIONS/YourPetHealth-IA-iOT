using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using YourPetHealth.Models;
using YourPetHealth.Services;

namespace YourPetHealth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IaConsultasController : ControllerBase
    {
        private readonly IMongoCollection<TelemetriaPet> _telemetriaCollection;
        private readonly GeminiService _geminiService;

        public IaConsultasController(MongoDbService mongoDbService, GeminiService geminiService)
        {
            _telemetriaCollection = mongoDbService.GetCollection<TelemetriaPet>("telemetria");
            _geminiService = geminiService;
        }

        public class ChatRequest
        {
            public int PetId { get; set; }
            public string Mensagem { get; set; } = string.Empty;
        }

        [HttpPost("perguntar")]
        public async Task<IActionResult> Perguntar([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Mensagem))
            {
                return BadRequest("A mensagem não pode estar vazia.");
            }

           
            var filter = Builders<TelemetriaPet>.Filter.Eq(t => t.PetId, request.PetId);
            var ultimaTelemetria = await _telemetriaCollection
                .Find(filter)
                .SortByDescending(t => t.DataLeitura)
                .FirstOrDefaultAsync();

            string telemetriaContexto = ultimaTelemetria != null
                ? $"Batimentos: {ultimaTelemetria.BatimentosCardiacos} BPM, Temperatura: {ultimaTelemetria.Temperatura}°C, Status: {ultimaTelemetria.StatusGeral}, Data: {ultimaTelemetria.DataLeitura:dd/MM/yyyy HH:mm}"
                : "Nenhum dado de telemetria encontrado para este pet recentemente.";

           
            var respostaIa = await _geminiService.ConsultarIaAsync(request.Mensagem, telemetriaContexto);

            return Ok(new
            {
                petId = request.PetId,
                pergunta = request.Mensagem,
                resposta = respostaIa,
                telemetriaUsada = telemetriaContexto
            });
        }
    }
}