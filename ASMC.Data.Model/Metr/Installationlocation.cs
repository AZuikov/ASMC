using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    ///  Сущность места установки.
    /// </summary>
    [Table("SPMU")]
    public class InstallationLocation
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Required]
        [Column("IDSPMU", TypeName = "int")]
        public int? Id{ get; set;}
        /// <summary>
        /// Возвращает или задает (НЕ ЯСНО, ЧТО ЭТО).
        /// </summary>
        [Column("IDSPMUR", TypeName = "int")]
        public int? _Filed{ get; set; }
        /// <summary>
        /// Возвращает или задает номер уровня.
        /// </summary>
        [Required]
        [Column("NNUR", TypeName = "int")]
        public int? LevelOfNumber{ get; set; }
        /// <summary>
        /// Возвращает или задает тип ветви (НЕ ЯСНО, ОТ КУДА ДАННЫЕ).
        /// </summary>
        [Required]
        [Column("TPVT", TypeName = "int")]
        public int? TypeOfBranch{ get; set; }
        /// <summary>
        /// Возвращает или задает наименование места установки.
        /// </summary>
        [Column("NMMU", TypeName = "varchar(50)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает полное наименование места установки.
        /// </summary>
        [Required]
        [Column("NMMUP", TypeName = "varchar(80)")]
        public string FullName { get; set; }
        /// <summary>
        /// Возвращает или задает дополнительные сведения.
        /// </summary>
        [Required]
        [Column("DPSV", TypeName = "varchar(100)")]
        public string AdditionalInformation { get; set; }
    }
}
