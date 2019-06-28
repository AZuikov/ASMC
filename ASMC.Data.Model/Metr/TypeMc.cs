using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность вида MK
    /// </summary>
    [Table("SPVDMK")]
    public class TypeMc
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPVDMK", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает вид МК
        /// </summary>
        [Column("NMVDMK", TypeName = "varchar(50)")]
        public string Name { get; set; }    
        /// <summary>
        /// Возвращает или задает обозначение вида МК
        /// </summary>
        [Column("OBVDMK", TypeName = "varchar(1)")]
        public string Symbol {get;  set;  }
    }
}
