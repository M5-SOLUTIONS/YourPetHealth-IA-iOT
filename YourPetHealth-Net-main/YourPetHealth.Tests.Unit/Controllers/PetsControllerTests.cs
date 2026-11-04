using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourPetHealth.Controllers;
using YourPetHealth.Models;
using YourPetHealth.Tests.Unit.Fixtures;
using YourPetHealth.Tests.Unit.TestHelpers;
using Xunit;

namespace YourPetHealth.Tests.Unit.Controllers
{
    public class PetsControllerTests : IClassFixture<ReferenceDataFixture>
    {
        private readonly ReferenceDataFixture _fixture;

        public PetsControllerTests(ReferenceDataFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_PetParaTutorLogado_SalvaComResponsavelIdCorreto()
        {
            using var context = _fixture.CreateContext();
            var controller = new PetsController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.ResponsavelId);
            var novoPet = new Pet { Nome = "Rex", Sexo = "MACHO", Raca = "Labrador", Idade = 2, Peso = 20m };

            var resultado = await controller.Create(novoPet);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(_fixture.ResponsavelId, novoPet.ResponsavelId);
        }

        [Fact]
        public async Task Create_UsuarioSemEntidadeVinculada_RetornaForbid()
        {
            using var context = _fixture.CreateContext();
            var controller = new PetsController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, entidadeId: null);
            var novoPet = new Pet { Nome = "Rex", Sexo = "MACHO" };

            var resultado = await controller.Create(novoPet);

            Assert.IsType<ForbidResult>(resultado);
        }

        [Fact]
        public async Task Edit_PetDeOutroTutor_RetornaForbid()
        {
            using var context = _fixture.CreateContext();
            var controller = new PetsController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.OutroResponsavelId);

            var resultado = await controller.Edit(_fixture.PetId);

            Assert.IsType<ForbidResult>(resultado);
        }

        [Fact]
        public async Task Edit_PetDoProprioTutor_RetornaView()
        {
            using var context = _fixture.CreateContext();
            var controller = new PetsController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.ResponsavelId);

            var resultado = await controller.Edit(_fixture.PetId);

            var view = Assert.IsType<ViewResult>(resultado);
            var pet = Assert.IsType<Pet>(view.Model);
            Assert.Equal(_fixture.PetId, pet.Id);
        }

        [Fact]
        public async Task Index_TutorLogado_RetornaSomenteOsPetsDele()
        {
            using var context = _fixture.CreateContext();
            var controller = new PetsController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.ResponsavelId);

            var resultado = await controller.Index();

            var view = Assert.IsType<ViewResult>(resultado);
            var pets = Assert.IsAssignableFrom<IEnumerable<Pet>>(view.Model);
            Assert.All(pets, p => Assert.Equal(_fixture.ResponsavelId, p.ResponsavelId));
        }

        [Fact]
        public async Task DeleteConfirmed_PetComConsultaAgendada_BloqueiaRemocaoERedirecionaParaDetails()
        {
            using var context = _fixture.CreateContext();
            context.Consultas.Add(new Consulta
            {
                PetId = _fixture.PetId,
                VeterinarioId = _fixture.VeterinarioId,
                Tipo = "ROTINA",
                Status = "AGENDADA",
                Data = DateTime.Now.AddDays(1)
            });
            await context.SaveChangesAsync();

            var controller = new PetsController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.ResponsavelId);

            var resultado = await controller.DeleteConfirmed(_fixture.PetId);

            var redirect = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Details", redirect.ActionName);
            Assert.True(await context.Pets.AnyAsync(p => p.Id == _fixture.PetId));
        }
    }
}
