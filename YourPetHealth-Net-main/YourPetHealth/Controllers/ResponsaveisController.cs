using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Diagnostics;
using YourPetHealth.Models;

namespace YourPetHealth.Controllers
{
    [Authorize(Roles = "VETERINARIO,ADMIN")]
    public class ResponsaveisController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public ResponsaveisController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index()
            => View(await _context.Responsaveis.ToListAsync());

        [AllowAnonymous]
        public IActionResult Create() => RedirectToAction("Register", "Auth");

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create(Responsavel r)
        {
            using var activity = AppDiagnostics.ActivitySource.StartActivity("Responsaveis.Create");
            activity?.SetTag("responsavel.email", r.Email);

            if (string.IsNullOrWhiteSpace(r.Senha))
            {
                ModelState.AddModelError("Senha", "Senha é obrigatória.");
            }

            if (!ModelState.IsValid) return View(r);

            var existente = await _context.Responsaveis.AnyAsync(x => x.Email == r.Email);
            if (existente)
            {
                ModelState.AddModelError("Email", "Email já cadastrado.");
                return View(r);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Responsaveis.Add(r);
            await _context.SaveChangesAsync();

            var usuario = new Usuario
            {
                Email = r.Email,
                Perfil = PerfilUsuario.Responsavel,
                ResponsavelId = r.Id,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, r.Senha!);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            AppDiagnostics.CadastrosResponsavel.Add(1);

            TempData["Sucesso"] = "Responsável cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var r = await _context.Responsaveis.FindAsync(id);
            if (r == null) return NotFound();
            return View(r);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Responsavel atualizado)
        {
            if (!ModelState.IsValid) return View(atualizado);

            var responsavel = await _context.Responsaveis.FindAsync(atualizado.Id);
            if (responsavel == null) return NotFound();

            responsavel.Nome = atualizado.Nome;
            responsavel.Email = atualizado.Email;
            responsavel.Telefone = atualizado.Telefone;

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.ResponsavelId == responsavel.Id);
            if (usuario != null)
            {
                usuario.Email = atualizado.Email;

                if (!string.IsNullOrWhiteSpace(atualizado.Senha))
                {
                    usuario.SenhaHash = _passwordHasher.HashPassword(usuario, atualizado.Senha);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Responsável atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.Responsaveis.FindAsync(id);
            if (r == null) return NotFound();
            return View(r);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var r = await _context.Responsaveis.FindAsync(id);
            if (r != null)
            {
                _context.Responsaveis.Remove(r);
                await _context.SaveChangesAsync();
            }
            TempData["Sucesso"] = "Responsável removido com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
