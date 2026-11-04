using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourPetHealth.Models
{
    [Table("YP_T_HISTORICO_CLINICO")]
    public class HistoricoClinico
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("PET_ID")]
        public int PetId { get; set; }

        [Required]
        [Column("TIPO")]
        [MaxLength(50)]
        public string Tipo { get; set; } = "CONSULTA";

        [Column("DESCRICAO")]
        [MaxLength(1000)]
        public string? Descricao { get; set; }

        [Required]
        [Column("DATA")]
        public DateTime Data { get; set; }

        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }
    }
}