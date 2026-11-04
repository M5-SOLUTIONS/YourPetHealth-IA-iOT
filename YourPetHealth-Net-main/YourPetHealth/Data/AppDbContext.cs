using Microsoft.EntityFrameworkCore;
using YourPetHealth.Models;

namespace YourPetHealth.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Responsavel> Responsaveis { get; set; }
        public DbSet<Veterinario> Veterinarios { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<HistoricoClinico> HistoricosClinicos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Responsavel>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Veterinario>()
                .Property(v => v.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Pet>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Consulta>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<HistoricoClinico>()
                .Property(h => h.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Responsavel>()
                .HasIndex(r => r.Email)
                .IsUnique()
                .HasDatabaseName("UQ_RESPONSAVEL_EMAIL");

            modelBuilder.Entity<Veterinario>()
                .HasIndex(v => v.Email)
                .IsUnique()
                .HasDatabaseName("UQ_VETERINARIO_EMAIL");

            modelBuilder.Entity<Veterinario>()
                .HasIndex(v => v.Crmv)
                .IsUnique()
                .HasDatabaseName("UQ_VETERINARIO_CRMV");

            if (Database.IsRelational())
            {
                modelBuilder.Entity<Pet>().ToTable(tb =>
                    tb.HasCheckConstraint("CK_PET_SEXO", "SEXO_PET IN ('MACHO','FEMEA')"));

                modelBuilder.Entity<Consulta>().ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_CONSULTA_STATUS", "STATUS IN ('AGENDADA','REALIZADA','CANCELADA')");
                    tb.HasCheckConstraint("CK_CONSULTA_TIPO", "TIPO IN ('ROTINA','VACINACAO','EMERGENCIA','RETORNO','EXAME')");
                });

                modelBuilder.Entity<Usuario>().ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_USUARIO_PERFIL",
                        "PERFIL IN ('RESPONSAVEL','VETERINARIO','ADMIN')");

                    tb.HasCheckConstraint("CK_USUARIO_VINCULO",
                        "(PERFIL = 'RESPONSAVEL' AND RESPONSAVEL_ID IS NOT NULL AND VETERINARIO_ID IS NULL) " +
                        "OR (PERFIL = 'VETERINARIO' AND VETERINARIO_ID IS NOT NULL AND RESPONSAVEL_ID IS NULL) " +
                        "OR (PERFIL = 'ADMIN' AND RESPONSAVEL_ID IS NULL AND VETERINARIO_ID IS NULL)");
                });
            }

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Responsavel)
                .WithMany(r => r.Pets)
                .HasForeignKey(p => p.ResponsavelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Pet)
                .WithMany(p => p.Consultas)
                .HasForeignKey(c => c.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Veterinario)
                .WithMany(v => v.Consultas)
                .HasForeignKey(c => c.VeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HistoricoClinico>()
                .HasOne(h => h.Pet)
                .WithMany(p => p.Historicos)
                .HasForeignKey(h => h.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UK_USUARIO_EMAIL");

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.ResponsavelId)
                .IsUnique()
                .HasDatabaseName("UK_USUARIO_RESPONSAVEL");

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.VeterinarioId)
                .IsUnique()
                .HasDatabaseName("UK_USUARIO_VETERINARIO");

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Responsavel)
                .WithOne(r => r.Usuario)
                .HasForeignKey<Usuario>(u => u.ResponsavelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Veterinario)
                .WithOne(v => v.Usuario)
                .HasForeignKey<Usuario>(u => u.VeterinarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
