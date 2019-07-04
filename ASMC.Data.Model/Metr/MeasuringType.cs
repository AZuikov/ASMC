using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность области измерений
    /// </summary>
    [Table("SPOI")]
    public class MeasuringType 
    {   
        /// <summary>
        ///  Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPOI", TypeName = "int")]
        public  int? Id { get; set;}
        /// <summary>
        ///  Возвращает или задает код области измерений
        /// </summary>
        [Required]
        [Column("KDOI", TypeName = "nvarchar(3)")]
        public string Code { get; set; }
        /// <summary>
        /// Возвращает или задает наименование области измерений
        /// </summary>
        [Required]
        [Column("NMOI", TypeName = "nvarchar(80)")]
        public  string Name { get; set; }
    }
}
