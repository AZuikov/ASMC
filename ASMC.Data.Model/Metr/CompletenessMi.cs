using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Комплектность СИ
    /// </summary>
    [Table("SPKMMK")]
    public class CompletenessMi
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDSPKMMK", TypeName = "int")]
        public int CompletenessMiId { get; }
        /// <summary>
        /// Наименование комплектности СИ
        /// </summary>
        [Required]
        [Column("KMMK", TypeName = "nvarchar(max)")]
        public string Name { get; set; }
        /// <summary>
        /// Признак в АИС «Метрконтроль»
        /// </summary>
        [Column("PRTPRBMETR", TypeName = "nvarchar(max)")]
        public string AisMetrkontrol { get; set; }
    }
}
