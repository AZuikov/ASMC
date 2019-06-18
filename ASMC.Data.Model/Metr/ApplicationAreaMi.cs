using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "области применения СИ"
    /// </summary>
    [Table("SPOP")]
    public class ApplicationAreaMi
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDSPOP", TypeName = "int")]
        public int ApplicationAreaMiId { get; private set; }
        /// <summary>
        /// Область применения СИ
        /// </summary>
        [Required]
        [Column("NMOP", TypeName = "varchar(max)")]
        public string Name { get; set; }
        
    }
}
