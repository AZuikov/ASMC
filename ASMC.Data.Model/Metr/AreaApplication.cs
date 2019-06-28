using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность области применения. 
    /// </summary>
    [Table("SPOP")]
    public class AreaApplication
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPOP", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает область применения СИ.
        /// </summary>
        [Required]
        [Column("NMOP", TypeName = "varchar(50)")]
        public string Name { get; set; }
        
    }
}
