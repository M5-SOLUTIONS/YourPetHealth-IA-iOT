using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Models;

namespace YourPetHealth.Controllers
{
    [Authorize]
    public class VeterinariosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public VeterinariosController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index()
            => View(await _context.Veterinarios.ToListAsync());

        [AllowAnonymous]
        public IActionResult Create() => RedirectToAction("Register", "Auth");

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create(Veterinario v)
        {
            if (string.IsNullOrWhiteSpace(v.Senha))
            {
                ModelState.AddModelError("Senha", "Senha é obrigatória.");
            }

            if (!ModelState.IsValid) return View(v);

            var emailExistente = await _context.Veterinarios.AnyAsync(x => x.Email == v.Email);
            if (emailExistente)
            {
                ModelState.AddModelError("Email", "Email já cadastrado.");
                return View(v);
            }

            var crmvExistente = await _context.Veterinarios.AnyAsync(x => x.Crmv == v.Crmv);
            if (crmvExistente)
            {
                ModelState.AddModelError("Crmv", "CRMV já cadastrado.");
                return View(v);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Veterinarios.Add(v);
            await _context.SaveChangesAsync();

            var usuario = new Usuario
            {
                Email = v.Email,
                Perfil = PerfilUsuario.Veterinario,
                VeterinarioId = v.Id,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, v.Senha!);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Sucesso"] = "Veterinário cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "VETERINARIO,ADMIN")]
        public async Task<IActionResult> Edit(int id)
        {
            var v = await _context.Veterinarios.FindAsync(id);
            if (v == null) return NotFound();
            return View(v);
        }

        [Authorize(Roles = "VETERINARIO,ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Edit(Veterinario atualizado)
        {
            if (!ModelState.IsValid) return View(atualizado);

            var veterinario = await _context.Veterinarios.FindAsync(atualizado.Id);
            if (veterinario == null) return NotFound();

            veterinario.Nome = atualizado.Nome;
            veterinario.Email = atualizado.Email;
            veterinario.Telefone = atualizado.Telefone;
            veterinario.Crmv = atualizado.Crmv;
            veterinario.Especialidade = atualizado.Especialidade;

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.VeterinarioId == veterinario.Id);
            if (usuario != null)
            {
                usuario.Email = atualizado.Email;

                if (!string.IsNullOrWhiteSpace(atualizado.Senha))
                {
                    usuario.SenhaHash = _passwordHasher.HashPassword(usuario, atualizado.Senha);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Veterinário atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "VETERINARIO,ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var v = await _context.Veterinarios.FindAsync(id);
            if (v == null) return NotFound();
            return View(v);
        }

        [Authorize(Roles = "VETERINARIO,ADMIN")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var v = await _context.Veterinarios.FindAsync(id);
            if (v != null)
            {
                _context.Veterinarios.Remove(v);
                await _context.SaveChangesAsync();
            }
            TempData["Sucesso"] = "Veterinário removido com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
