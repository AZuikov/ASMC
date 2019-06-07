using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "Штатное состояние"
    /// </summary>
    [Table("SPSS")]
    public class NormalState
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDSPSS", TypeName = "int")]
        public int NormalStateId { get; }
        /// <summary>
        /// Наименование шатного состояния
        /// </summary>
        [Required]
        [Column("NMSS", TypeName = "varchar(max)")]
        public string NameNormalState { get; set; }
        /// <summary>
        /// Флаг на операции с записью
        /// </summary>
        [Required]
        [Column("FLOZ", TypeName = "int")]
        public int FlagWrite { get; set; }
        /// <summary>
        ///  Код для стандартизации состояний
        /// </summary>
        [Column("KDSS", TypeName = "int")]
        public int Code { get; set; }
    }
}
