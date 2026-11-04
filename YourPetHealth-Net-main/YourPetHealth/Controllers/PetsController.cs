using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Extensions;
using YourPetHealth.Models;

namespace YourPetHealth.Controllers
{
    [Authorize]
    public class PetsController : Controller
    {
        private readonly AppDbContext _context;
        public PetsController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var query = _context.Pets.Include(p => p.Responsavel).AsQueryable();

            if (User.IsInRole(PerfilUsuario.Responsavel))
            {
                var meuId = User.GetEntidadeId();
                query = query.Where(p => p.ResponsavelId == meuId);
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.Responsavel)
                .Include(p => p.Consultas).ThenInclude(c => c.Veterinario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null) return NotFound();

            if (User.IsInRole(PerfilUsuario.Responsavel) && !DonoDoPet(pet))
                return Forbid();

            ViewBag.TemConsultaAgendada = pet.Consultas.Any(c => c.Status == "AGENDADA");

            return View(pet);
        }

        [Authorize(Roles = "RESPONSAVEL")]
        public IActionResult Create() => View();

        [Authorize(Roles = "RESPONSAVEL")]
        [HttpPost]
        public async Task<IActionResult> Create(Pet p)
        {
            var meuId = User.GetEntidadeId();
            if (meuId == null) return Forbid();
            p.ResponsavelId = meuId.Value;

            ModelState.Remove(nameof(Pet.ResponsavelId));
            if (!ModelState.IsValid) return View(p);

            p.Sexo = p.Sexo.ToUpper();

            _context.Pets.Add(p);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Pet cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "RESPONSAVEL")]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.Pets.FindAsync(id);
            if (p == null) return NotFound();
            if (!DonoDoPet(p)) return Forbid();
            return View(p);
        }

        [Authorize(Roles = "RESPONSAVEL")]
        [HttpPost]
        public async Task<IActionResult> Edit(Pet atualizado)
        {
            var pet = await _context.Pets.FindAsync(atualizado.Id);
            if (pet == null) return NotFound();
            if (!DonoDoPet(pet)) return Forbid();

            pet.Nome = atualizado.Nome;
            pet.Raca = atualizado.Raca;
            pet.Idade = atualizado.Idade;
            pet.Peso = atualizado.Peso;
            pet.Sexo = atualizado.Sexo.ToUpper();

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Pet atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "RESPONSAVEL")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Pets.Include(x => x.Responsavel).Include(x => x.Consultas).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            if (!DonoDoPet(p)) return Forbid();
            ViewBag.TemConsultaAgendada = p.Consultas.Any(c => c.Status == "AGENDADA");
            return View(p);
        }

        [Authorize(Roles = "RESPONSAVEL")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _context.Pets.Include(x => x.Consultas).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            if (!DonoDoPet(p)) return Forbid();

            if (p.Consultas.Any(c => c.Status == "AGENDADA"))
            {
                TempData["Erro"] = "Não é possível remover um pet com consultas agendadas.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Pets.Remove(p);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Pet removido com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        private bool DonoDoPet(Pet p) => p.ResponsavelId == User.GetEntidadeId();
    }
}
