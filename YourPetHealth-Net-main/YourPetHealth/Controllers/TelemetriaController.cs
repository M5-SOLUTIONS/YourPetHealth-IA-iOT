using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using YourPetHealth.Models;
using YourPetHealth.Services;

namespace YourPetHealth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetriaController : ControllerBase
    {
        private readonly IMongoCollection<TelemetriaPet> _telemetriaCollection;

        public TelemetriaController(MongoDbService mongoDbService)
        {
            _telemetriaCollection = mongoDbService.GetCollection<TelemetriaPet>("telemetria");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TelemetriaPet telemetria)
        {
            telemetria.Id = null; 
            await _telemetriaCollection.InsertOneAsync(telemetria);
            return CreatedAtAction(nameof(Post), new { id = telemetria.Id }, telemetria);
        }
    }
}