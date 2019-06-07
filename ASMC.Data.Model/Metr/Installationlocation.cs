using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "Мест установки"
    /// </summary>
    [Table("SPMU")]
    public class Installationlocation
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Required]
        [Column("IDSPMU", TypeName = "int")]
        public int InstallationlocationId{ get; set; }
        [Column("IDSPMUR", TypeName = "int")]
        public int NONAME{ get; set; }
        /// <summary>
        /// Номер уровня
        /// </summary>
        [Required]
        [Column("NNUR", TypeName = "int")]
        public int LevelNumber{ get; set; }
        /// <summary>
        /// Тип ветви
        /// </summary>
        [Required]
        [Column("TPVT", TypeName = "int")]
        public int BranchType{ get; set; }
        /// <summary>
        /// Место установки
        /// </summary>
        [Column("NMMU", TypeName = "varchar(max)")]
        public string InstallationLocation { get; set; }
        /// <summary>
        /// Полное наименование места установки
        /// </summary>
        [Required]
        [Column("NMMUP", TypeName = "varchar(max)")]
        public string FullInstallationLocation { get; set; }
        /// <summary>
        /// Дополнительные сведения
        /// </summary>
        [Required]
        [Column("DPSV", TypeName = "varchar(max)")]
        public string AdditionalInformation { get; set; }
    }
}
