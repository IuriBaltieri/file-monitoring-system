using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileMonitoring.Models
{
    [Table("arquivos")]
    public class Arquivo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("tipo_registro")]
        public int TipoRegistro { get; set; }
        
        [Required]
        [Column("empresa")]
        [MaxLength(20)]
        public string Empresa { get; set; } = string.Empty;
        
        [Required]
        [Column("estabelecimento")]
        [MaxLength(20)]
        public string Estabelecimento { get; set; } = string.Empty;
        
        [Required]
        [Column("data_processamento")]
        public DateTime DataProcessamento { get; set; }
        
        [Column("periodo_inicial")]
        public DateTime? PeriodoInicial { get; set; }
        
        [Column("periodo_final")]
        public DateTime? PeriodoFinal { get; set; }
        
        [Column("sequencia")]
        [MaxLength(10)]
        public string? Sequencia { get; set; }
        
        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Recepcionado";
        
        [Column("caminho_backup")]
        public string? CaminhoBackup { get; set; }
        
        [Column("criado_em")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}