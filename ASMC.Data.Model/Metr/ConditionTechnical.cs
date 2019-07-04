using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность технического состояня
    /// </summary>
    [Table("SPTS")]
    public class ConditionTechnical
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPTS", TypeName = "int")]
        public int? Id { get; set;  }
        /// <summary>
        /// Возвращает или задает наименование технического состояния
        /// </summary>
        [Required]
        [Column("NMTS", TypeName = "varchar(30)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает флаг на разришение операции на запись
        /// </summary>
        [Required]
        [Browsable(false)]
        [Column("FLOZ", TypeName = "int")]
        public int? FlagWrite { get; set; }
        /// <summary>
        /// Возвращает или задает код для стандартизации состояний
        /// </summary>
        [Column("KDTS", TypeName = "int")]
        public int? Code { get; set; }
    }
}
