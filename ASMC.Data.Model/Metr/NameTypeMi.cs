using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "наименование типа СИ"
    /// </summary>
    [Table("SPNMTP")]
    public class NameTypeMi
    {
        /// <summary>
        /// Id наименовани типа СИ
        /// </summary>
        [Key]
        [Column("IDSPNMTP", TypeName = "int")]
        public int NameTypeMiId { get; private set;}
        /// <summary>
        /// Наименование типа
        /// </summary>
        [Required]
        [Column("NMTP", TypeName = "varchar(max)")]
        public string Name { get; set; }
        /// <summary>
        /// Специальное наименование типа СИ
        /// </summary>
        [Column("NMTPSP", TypeName = "varchar(max)")]
        public string SpecialName { get; set; }
    }
}
