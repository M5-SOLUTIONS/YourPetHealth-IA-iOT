using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourPetHealth.Models
{
    [Table("YP_T_USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress]
        [Column("EMAIL")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("SENHA_HASH")]
        [MaxLength(255)]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        [Column("PERFIL")]
        [MaxLength(20)]
        public string Perfil { get; set; } = string.Empty;

        [Column("ATIVO")]
        public bool Ativo { get; set; } = true;

        [Column("CRIADO_EM")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        [Column("RESPONSAVEL_ID")]
        public int? ResponsavelId { get; set; }

        [Column("VETERINARIO_ID")]
        public int? VeterinarioId { get; set; }

        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }

        [ForeignKey("VeterinarioId")]
        public Veterinario? Veterinario { get; set; }
    }

    public static class PerfilUsuario
    {
        public const string Responsavel = "RESPONSAVEL";
        public const string Veterinario = "VETERINARIO";
        public const string Admin = "ADMIN";
    }
}
