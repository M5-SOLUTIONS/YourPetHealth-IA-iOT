using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using YourPetHealth.Controllers;
using YourPetHealth.Models;
using YourPetHealth.Tests.Unit.Fixtures;
using YourPetHealth.Tests.Unit.TestHelpers;
using Xunit;

namespace YourPetHealth.Tests.Unit.Controllers
{
    public class AuthControllerTests : IClassFixture<ReferenceDataFixture>
    {
        private readonly ReferenceDataFixture _fixture;
        private readonly Mock<IPasswordHasher<Usuario>> _passwordHasherMock;

        public AuthControllerTests(ReferenceDataFixture fixture)
        {
            _fixture = fixture;
            _passwordHasherMock = new Mock<IPasswordHasher<Usuario>>();
        }

        private AuthController CriarController(YourPetHealth.Data.AppDbContext context)
        {
            var controller = new AuthController(context, _passwordHasherMock.Object, Mock.Of<ILogger<AuthController>>());
            ControllerTestHelper.SetUser(controller, "", null);
            return controller;
        }

        [Fact]
        public async Task Login_EmailNaoCadastrado_RetornaViewComErroSemAutenticar()
        {
            using var context = _fixture.CreateContext();
            var controller = CriarController(context);

            var resultado = await controller.Login("naoexiste@teste.com", "qualquersenha");

            var view = Assert.IsType<ViewResult>(resultado);
            Assert.False(controller.ModelState.IsValid);
            _passwordHasherMock.Verify(
                h => h.VerifyHashedPassword(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_SenhaIncorreta_RetornaViewComErro()
        {
            using var context = _fixture.CreateContext();
            var usuario = new Usuario
            {
                Email = "login.teste@teste.com",
                SenhaHash = "hash-qualquer",
                Perfil = PerfilUsuario.Responsavel,
                ResponsavelId = _fixture.ResponsavelId,
                Ativo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(It.IsAny<Usuario>(), usuario.SenhaHash, "senhaErrada"))
                .Returns(PasswordVerificationResult.Failed);

            var controller = CriarController(context);

            var resultado = await controller.Login(usuario.Email, "senhaErrada");

            Assert.IsType<ViewResult>(resultado);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Login_CredenciaisValidasDeVeterinario_RedirecionaParaListaDeResponsaveis()
        {
            using var context = _fixture.CreateContext();
            var usuario = new Usuario
            {
                Email = "vet.login@teste.com",
                SenhaHash = "hash-correto",
                Perfil = PerfilUsuario.Veterinario,
                VeterinarioId = _fixture.VeterinarioId,
                Ativo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(It.IsAny<Usuario>(), usuario.SenhaHash, "senhaCorreta"))
                .Returns(PasswordVerificationResult.Success);

            var controller = CriarController(context);

            var resultado = await controller.Login(usuario.Email, "senhaCorreta");

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Responsaveis", redirect.ControllerName);
        }

        [Fact]
        public async Task Login_CredenciaisValidasDeTutor_RedirecionaParaMeusPets()
        {
            using var context = _fixture.CreateContext();
            var usuario = new Usuario
            {
                Email = "tutor.login@teste.com",
                SenhaHash = "hash-correto",
                Perfil = PerfilUsuario.Responsavel,
                ResponsavelId = _fixture.ResponsavelId,
                Ativo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            _passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(It.IsAny<Usuario>(), usuario.SenhaHash, "senhaCorreta"))
                .Returns(PasswordVerificationResult.Success);

            var controller = CriarController(context);

            var resultado = await controller.Login(usuario.Email, "senhaCorreta");

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Pets", redirect.ControllerName);
        }
    }
}
