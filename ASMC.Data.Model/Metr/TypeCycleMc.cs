using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность вида цикла MK
    /// </summary>
    [Table("SPVDMC")]
   public class TypeCycleMc
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPVDMC", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает наименование цикла МК
        /// </summary>
        [Column("NMVDMC", TypeName = "varchar(30)")]
        public string Name { get; set; }
    }
}
