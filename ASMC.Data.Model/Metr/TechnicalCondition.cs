using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "Техническое состояние"
    /// </summary>
    [Table("SPTS")]
    public class TechnicalCondition
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDSPTS", TypeName = "int")]
        public int TechnicalConditionId { get; private set;  }
        /// <summary>
        /// Наименование технического состояния
        /// </summary>
        [Required]
        [Column("NMTS", TypeName = "varchar(max)")]
        public string NameTechnicalCondition { get; set; }
        /// <summary>
        /// Флаг на операции с записью
        /// </summary>
        [Required]
        [Column("FLOZ", TypeName = "int")]
        public int FlagWrite { get; set; }
        /// <summary>
        ///  Код для стандартизации состояний
        /// </summary>
        [Column("KDTS", TypeName = "int")]
        public int Code { get; set; }
    }
}
