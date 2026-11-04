using Microsoft.AspNetCore.Mvc;
using YourPetHealth.Controllers;
using YourPetHealth.Models;
using YourPetHealth.Tests.Unit.Fixtures;
using YourPetHealth.Tests.Unit.TestHelpers;
using Xunit;

namespace YourPetHealth.Tests.Unit.Controllers
{
    public class ConsultasControllerTests : IClassFixture<ReferenceDataFixture>
    {
        private readonly ReferenceDataFixture _fixture;

        public ConsultasControllerTests(ReferenceDataFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_PetNaoPertenceAoTutorLogado_AdicionaErroDeValidacao()
        {
            using var context = _fixture.CreateContext();
            var controller = new ConsultasController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.OutroResponsavelId);
            var consulta = new Consulta
            {
                PetId = _fixture.PetId,
                VeterinarioId = _fixture.VeterinarioId,
                Tipo = "ROTINA",
                Data = DateTime.Now.AddDays(1)
            };

            var resultado = await controller.Create(consulta);

            var view = Assert.IsType<ViewResult>(resultado);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("PetId"));
        }

        [Fact]
        public async Task Create_ConsultaValidaParaPetDoProprioTutor_NasceComStatusAgendada()
        {
            using var context = _fixture.CreateContext();
            var controller = new ConsultasController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Responsavel, _fixture.ResponsavelId);
            var consulta = new Consulta
            {
                PetId = _fixture.PetId,
                VeterinarioId = _fixture.VeterinarioId,
                Tipo = "ROTINA",
                Data = DateTime.Now.AddDays(1)
            };

            var resultado = await controller.Create(consulta);

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("AGENDADA", consulta.Status);
        }

        [Fact]
        public async Task Edit_StatusDiferenteDeRealizadaOuCancelada_AdicionaErroDeValidacao()
        {
            using var context = _fixture.CreateContext();
            var consulta = new Consulta
            {
                PetId = _fixture.PetId,
                VeterinarioId = _fixture.VeterinarioId,
                Tipo = "ROTINA",
                Status = "AGENDADA",
                Data = DateTime.Now.AddDays(1)
            };
            context.Consultas.Add(consulta);
            await context.SaveChangesAsync();

            var controller = new ConsultasController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Veterinario, _fixture.VeterinarioId);
            var atualizado = new Consulta { Id = consulta.Id, Status = "AGENDADA" };

            var resultado = await controller.Edit(atualizado);

            var view = Assert.IsType<ViewResult>(resultado);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_VeterinarioMarcandoComoRealizada_AtualizaStatusERedireciona()
        {
            using var context = _fixture.CreateContext();
            var consulta = new Consulta
            {
                PetId = _fixture.PetId,
                VeterinarioId = _fixture.VeterinarioId,
                Tipo = "ROTINA",
                Status = "AGENDADA",
                Data = DateTime.Now.AddDays(-1)
            };
            context.Consultas.Add(consulta);
            await context.SaveChangesAsync();

            var controller = new ConsultasController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Veterinario, _fixture.VeterinarioId);
            var atualizado = new Consulta { Id = consulta.Id, Status = "REALIZADA", Observacoes = "Tudo certo" };

            var resultado = await controller.Edit(atualizado);

            Assert.IsType<RedirectToActionResult>(resultado);
            var salva = await context.Consultas.FindAsync(consulta.Id);
            Assert.Equal("REALIZADA", salva!.Status);
        }

        [Fact]
        public async Task Edit_VeterinarioDiferenteDoResponsavelPelaConsulta_RetornaForbid()
        {
            using var context = _fixture.CreateContext();
            var outroVeterinario = new Veterinario { Nome = "Dr. Outro", Email = "outro.vet@teste.com", Crmv = "SP-99999" };
            context.Veterinarios.Add(outroVeterinario);
            var consulta = new Consulta
            {
                PetId = _fixture.PetId,
                VeterinarioId = _fixture.VeterinarioId,
                Tipo = "ROTINA",
                Status = "AGENDADA",
                Data = DateTime.Now.AddDays(1)
            };
            context.Consultas.Add(consulta);
            await context.SaveChangesAsync();

            var controller = new ConsultasController(context);
            ControllerTestHelper.SetUser(controller, PerfilUsuario.Veterinario, outroVeterinario.Id);

            var resultado = await controller.Edit(consulta.Id);

            Assert.IsType<ForbidResult>(resultado);
        }
    }
}
