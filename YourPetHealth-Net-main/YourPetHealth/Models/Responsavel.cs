using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourPetHealth.Models
{
    [Table("YP_T_RESPONSAVEIS")]
    public class Responsavel
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

        [NotMapped]
        [DataType(DataType.Password)]
        public string? Senha { get; set; }

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();

        public Usuario? Usuario { get; set; }
    }
}
