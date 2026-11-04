using Microsoft.EntityFrameworkCore;
using YourPetHealth.Data;
using YourPetHealth.Models;

namespace YourPetHealth.Tests.Unit.Fixtures
{
    public class ReferenceDataFixture
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        public int ResponsavelId { get; }
        public int OutroResponsavelId { get; }
        public int VeterinarioId { get; }
        public int PetId { get; }

        public ReferenceDataFixture()
        {
            using var context = CreateContext();

            var responsavel = new Responsavel { Nome = "Ana Tutora", Email = "ana.tutora@teste.com", Telefone = "11999990000" };
            var outroResponsavel = new Responsavel { Nome = "Bruno Tutor", Email = "bruno.tutor@teste.com", Telefone = "11999990001" };
            var veterinario = new Veterinario { Nome = "Dra. Bia", Email = "bia.vet@teste.com", Crmv = "SP-12345", Especialidade = "Clínica Geral" };

            context.Responsaveis.AddRange(responsavel, outroResponsavel);
            context.Veterinarios.Add(veterinario);
            context.SaveChanges();

            var pet = new Pet { Nome = "Luly", Raca = "SRD", Idade = 3, Peso = 8.5m, Sexo = "FEMEA", ResponsavelId = responsavel.Id };
            context.Pets.Add(pet);
            context.SaveChanges();

            ResponsavelId = responsavel.Id;
            OutroResponsavelId = outroResponsavel.Id;
            VeterinarioId = veterinario.Id;
            PetId = pet.Id;
        }

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(_dbName)
                .Options;
            return new AppDbContext(options);
        }
    }
}
