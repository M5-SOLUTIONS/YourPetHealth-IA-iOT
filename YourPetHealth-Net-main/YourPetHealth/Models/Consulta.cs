using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourPetHealth.Models
{
    [Table("YP_T_CONSULTAS")]
    public class Consulta
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("PET_ID")]
        public int PetId { get; set; }

        [Required]
        [Column("VETERINARIO_ID")]
        public int VeterinarioId { get; set; }

        [Required(ErrorMessage = "Tipo é obrigatório")]
        [Column("TIPO")]
        [MaxLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Column("DESCRICAO")]
        [MaxLength(1000)]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Data é obrigatória")]
        [Column("DATA")]
        public DateTime Data { get; set; }

        [Column("OBSERVACOES")]
        [MaxLength(1000)]
        public string? Observacoes { get; set; }

        [Required]
        [Column("STATUS")]
        [MaxLength(30)]
        public string Status { get; set; } = "AGENDADA";

        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }

        [ForeignKey("VeterinarioId")]
        public Veterinario? Veterinario { get; set; }
    }
}