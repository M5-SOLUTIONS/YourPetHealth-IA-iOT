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
    public class ConsultasController : Controller
    {
        private readonly AppDbContext _context;
        public ConsultasController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var query = _context.Consultas.Include(c => c.Pet).ThenInclude(p => p.Responsavel).Include(c => c.Veterinario).AsQueryable();

            var meuId = User.GetEntidadeId();
            if (User.IsInRole(PerfilUsuario.Responsavel))
                query = query.Where(c => c.Pet.ResponsavelId == meuId);
            else if (User.IsInRole(PerfilUsuario.Veterinario))
                query = query.Where(c => c.VeterinarioId == meuId);

            return View(await query.OrderByDescending(c => c.Data).ToListAsync());
        }

        [Authorize(Roles = "RESPONSAVEL")]
        public async Task<IActionResult> Create()
        {
            var meuId = User.GetEntidadeId();
            ViewBag.PetId = new SelectList(
                await _context.Pets.Where(p => p.ResponsavelId == meuId).ToListAsync(), "Id", "Nome");
            ViewBag.VeterinarioId = new SelectList(await _context.Veterinarios.ToListAsync(), "Id", "Nome");
            return View();
        }

        [Authorize(Roles = "RESPONSAVEL")]
        [HttpPost]
        public async Task<IActionResult> Create(Consulta c)
        {
            var meuId = User.GetEntidadeId();
            var meusPets = await _context.Pets.Where(p => p.ResponsavelId == meuId).ToListAsync();

            ViewBag.PetId = new SelectList(meusPets, "Id", "Nome", c.PetId);
            ViewBag.VeterinarioId = new SelectList(await _context.Veterinarios.ToListAsync(), "Id", "Nome", c.VeterinarioId);

            if (!meusPets.Any(p => p.Id == c.PetId))
            {
                ModelState.AddModelError("PetId", "Selecione um dos seus pets.");
                return View(c);
            }

            c.Status = "AGENDADA";
            ModelState.Remove(nameof(Consulta.Status));

            if (!ModelState.IsValid) return View(c);

            _context.Consultas.Add(c);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Consulta agendada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "VETERINARIO,ADMIN")]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _context.Consultas.FindAsync(id);
            if (c == null) return NotFound();
            if (!ResponsavelPelaConsulta(c)) return Forbid();

            ViewBag.Petid = new SelectList(await _context.Pets.ToListAsync(), "Id", "Nome", c.PetId);
            ViewBag.VeterinarioId = new SelectList(await _context.Veterinarios.ToListAsync(), "Id", "Nome", c.VeterinarioId);
            return View(c);
        }

        [Authorize(Roles = "VETERINARIO,ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Edit(Consulta atualizado)
        {
            var consulta = await _context.Consultas.FindAsync(atualizado.Id);
            if (consulta == null) return NotFound();
            if (!ResponsavelPelaConsulta(consulta)) return Forbid();

            ViewBag.PetId = new SelectList(await _context.Pets.ToListAsync(), "Id", "Nome", atualizado.PetId);
            ViewBag.VeterinarioId = new SelectList(await _context.Veterinarios.ToListAsync(), "Id", "Nome", atualizado.VeterinarioId);

            var status = atualizado.Status?.ToUpper() ?? "";
            if (status != "REALIZADA" && status != "CANCELADA")
            {
                ModelState.AddModelError("Status", "Status deve ser atualizado para Realizada ou Cancelada!");
                return View(atualizado);
            }

            consulta.Status = status;
            consulta.Observacoes = atualizado.Observacoes;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Consulta atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Consultas.Include(x => x.Pet).ThenInclude(p => p.Responsavel).Include(x => x.Veterinario).FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();
            if (!PodeVerConsulta(c)) return Forbid();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var c = await _context.Consultas.Include(x => x.Pet).FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();
            if (!PodeVerConsulta(c)) return Forbid();

            _context.Consultas.Remove(c);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Consulta removida com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        private bool ResponsavelPelaConsulta(Consulta c) =>
            User.IsInRole(PerfilUsuario.Admin) || c.VeterinarioId == User.GetEntidadeId();

        private bool PodeVerConsulta(Consulta c)
        {
            if (User.IsInRole(PerfilUsuario.Admin)) return true;
            if (User.IsInRole(PerfilUsuario.Veterinario)) return c.VeterinarioId == User.GetEntidadeId();
            if (User.IsInRole(PerfilUsuario.Responsavel)) return c.Pet.ResponsavelId == User.GetEntidadeId();
            return false;
        }
    }
}
