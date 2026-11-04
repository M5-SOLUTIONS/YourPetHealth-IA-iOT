using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourPetHealth.Models
{
    [Table("YP_T_PETS")]
    public class Pet
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("RESPONSAVEL_ID")]
        public int ResponsavelId { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [Column("NOME")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Column("RACA")]
        [MaxLength(100)]
        public string? Raca { get; set; }

        [Column("IDADE")]
        public int? Idade { get; set; }

        [Column("PESO")]
        public decimal? Peso { get; set; }

        [Required(ErrorMessage = "Sexo é obrigatório")]
        [Column("SEXO_PET")]
        [MaxLength(20)]
        public string Sexo { get; set; } = string.Empty;

        [ForeignKey("ResponsavelId")]
        public Responsavel? Responsavel { get; set; }

        public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
        public ICollection<HistoricoClinico> Historicos { get; set; } = new List<HistoricoClinico>();
    }
}