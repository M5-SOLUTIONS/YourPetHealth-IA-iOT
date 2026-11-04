using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Extensions;
using YourPetHealth.Models;

namespace YourPetHealth.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(PerfilUsuario.Responsavel))
            {
                var meuId = User.GetEntidadeId();
                var agora = DateTime.Now;

                var meusPets = await _context.Pets
                    .Where(p => p.ResponsavelId == meuId)
                    .ToListAsync();

                var proximasConsultas = await _context.Consultas
                    .Include(c => c.Pet)
                    .Include(c => c.Veterinario)
                    .Where(c => c.Pet.ResponsavelId == meuId && c.Status == "AGENDADA" && c.Data >= agora)
                    .OrderBy(c => c.Data)
                    .Take(5)
                    .ToListAsync();

                ViewBag.MeusPets = meusPets;
                ViewBag.ProximasConsultas = proximasConsultas;
                ViewBag.ConsultasProximas48h = proximasConsultas.Count(c => c.Data <= agora.AddHours(48));

                return View("Tutor");
            }

            ViewBag.TotalResponsaveis = await _context.Responsaveis.CountAsync();
            ViewBag.TotalVeterinarios = await _context.Veterinarios.CountAsync();
            ViewBag.TotalPets = await _context.Pets.CountAsync();
            ViewBag.TotalConsultas = await _context.Consultas.CountAsync();
            ViewBag.ConsultasAgendadas = await _context.Consultas.CountAsync(c => c.Status == "AGENDADA");

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(new YourPetHealth_.Models.ErrorViewModel { RequestId = requestId });
        }
    }
}
