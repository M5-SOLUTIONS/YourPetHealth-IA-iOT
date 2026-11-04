using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourPetHealth.Models
{
    [Table("YP_T_VETERINARIOS")]
    public class Veterinario
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [Column("NOME")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [Column("EMAIL")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Column("TELEFONE")]
        [MaxLength(20)]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "CRMV é obrigatório")]
        [Column("CRMV")]
        [MaxLength(30)]
        public string Crmv { get; set; } = string.Empty;

        [Column("ESPECIALIDADE")]
        [MaxLength(100)]
        public string? Especialidade { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        public string? Senha { get; set; }

        public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();

        public Usuario? Usuario { get; set; }
    }
}
