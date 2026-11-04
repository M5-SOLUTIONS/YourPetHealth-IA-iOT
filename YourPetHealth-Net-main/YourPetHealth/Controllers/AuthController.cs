using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Diagnostics;
using YourPetHealth.Models;

namespace YourPetHealth.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher, ILogger<AuthController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha, string? returnUrl = null)
        {
            using var activity = AppDiagnostics.ActivitySource.StartActivity("Auth.Login");
            activity?.SetTag("auth.email", email);

            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ModelState.AddModelError(string.Empty, "Informe email e senha.");
                return View();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Responsavel)
                .Include(u => u.Veterinario)
                .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

            if (usuario == null)
            {
                AppDiagnostics.LoginsFalhos.Add(1);
                _logger.LogWarning("Tentativa de login falhou: email {Email} não encontrado ou inativo", email);
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
            if (resultado == PasswordVerificationResult.Failed)
            {
                AppDiagnostics.LoginsFalhos.Add(1);
                _logger.LogWarning("Tentativa de login falhou: senha incorreta para {Email}", email);
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View();
            }

            var nomeExibicao =
                usuario.Responsavel?.Nome
                ?? usuario.Veterinario?.Nome
                ?? usuario.Email;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, nomeExibicao),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Perfil)
            };

            if (usuario.ResponsavelId.HasValue)
                claims.Add(new Claim("EntidadeId", usuario.ResponsavelId.Value.ToString()));
            if (usuario.VeterinarioId.HasValue)
                claims.Add(new Claim("EntidadeId", usuario.VeterinarioId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            AppDiagnostics.LoginsRealizados.Add(1);
            activity?.SetTag("auth.perfil", usuario.Perfil);
            _logger.LogInformation("Login bem-sucedido para {Email} (perfil {Perfil})", email, usuario.Perfil);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return usuario.Perfil switch
            {
                PerfilUsuario.Veterinario => RedirectToAction("Index", "Responsaveis"),
                PerfilUsuario.Responsavel => RedirectToAction("Index", "Pets"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Sucesso"] = "Você saiu da sua conta.";
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.TipoConta != PerfilUsuario.Responsavel && model.TipoConta != PerfilUsuario.Veterinario)
                ModelState.AddModelError(nameof(model.TipoConta), "Selecione o tipo de conta.");

            if (model.TipoConta == PerfilUsuario.Veterinario)
            {
                ModelState.Remove(nameof(model.Crmv));
                if (string.IsNullOrWhiteSpace(model.Crmv))
                    ModelState.AddModelError(nameof(model.Crmv), "CRMV é obrigatório para veterinários.");
            }
            else
            {
                ModelState.Remove(nameof(model.Crmv));
                ModelState.Remove(nameof(model.Especialidade));
            }

            if (!ModelState.IsValid) return View(model);

            using var transaction = await _context.Database.BeginTransactionAsync();
            int entidadeId;

            if (model.TipoConta == PerfilUsuario.Veterinario)
            {
                if (await _context.Veterinarios.AnyAsync(v => v.Email == model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), "Email já cadastrado.");
                    return View(model);
                }
                if (await _context.Veterinarios.AnyAsync(v => v.Crmv == model.Crmv))
                {
                    ModelState.AddModelError(nameof(model.Crmv), "CRMV já cadastrado.");
                    return View(model);
                }

                var veterinario = new Veterinario
                {
                    Nome = model.Nome,
                    Email = model.Email,
                    Telefone = model.Telefone,
                    Crmv = model.Crmv!,
                    Especialidade = model.Especialidade
                };
                _context.Veterinarios.Add(veterinario);
                await _context.SaveChangesAsync();
                entidadeId = veterinario.Id;
            }
            else
            {
                if (await _context.Responsaveis.AnyAsync(r => r.Email == model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), "Email já cadastrado.");
                    return View(model);
                }

                var responsavel = new Responsavel
                {
                    Nome = model.Nome,
                    Email = model.Email,
                    Telefone = model.Telefone
                };
                _context.Responsaveis.Add(responsavel);
                await _context.SaveChangesAsync();
                entidadeId = responsavel.Id;

                AppDiagnostics.CadastrosResponsavel.Add(1);
            }

            var usuario = new Usuario
            {
                Email = model.Email,
                Perfil = model.TipoConta,
                Ativo = true,
                CriadoEm = DateTime.UtcNow,
                ResponsavelId = model.TipoConta == PerfilUsuario.Responsavel ? entidadeId : null,
                VeterinarioId = model.TipoConta == PerfilUsuario.Veterinario ? entidadeId : null
            };
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, model.Senha);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Conta criada para {Email} (perfil {Perfil})", model.Email, model.TipoConta);
            TempData["Sucesso"] = "Conta criada com sucesso! Já pode entrar.";
            return RedirectToAction(nameof(Login));
        }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(8, ErrorMessage = "A senha precisa ter no mínimo 8 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        [Required(ErrorMessage = "Selecione o tipo de conta")]
        public string TipoConta { get; set; } = string.Empty;

        public string? Crmv { get; set; }
        public string? Especialidade { get; set; }
    }
}
