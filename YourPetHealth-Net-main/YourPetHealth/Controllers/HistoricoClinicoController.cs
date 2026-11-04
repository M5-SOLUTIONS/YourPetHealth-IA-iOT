using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Models;

namespace YourPetHealth.Controllers
{
    [Authorize]
    public class HistoricoClinicoController : Controller
    {
        private readonly AppDbContext _context;
        public HistoricoClinicoController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index(int PetId = 0)
        {
            List<Consulta> historico = [];
            ViewBag.Petid = new SelectList(await _context.Pets.ToListAsync(), "Id", "Nome", PetId);

            if (PetId == 0)
            {
                historico = _context.Consultas.Include(c => c.Pet).OrderByDescending(h => h.Data).Where(h => h.Status == "REALIZADA").ToList();
            }
            else
            {
                historico = _context.Consultas.Include(c => c.Pet).OrderByDescending(h => h.Data).Where(h => h.Status == "REALIZADA").Where(h => h.PetId == PetId).ToList();
            }
            return View(historico);
        }
    }
}