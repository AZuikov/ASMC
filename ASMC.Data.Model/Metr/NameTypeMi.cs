using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность наименования типа СИ.
    /// </summary>
    [Table("SPNMTP")]
    public class NameTypeMi
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPNMTP", TypeName = "int")]
        public int? Id { get; set;}
        /// <summary>
        /// Возвращает или задает наименование.
        /// </summary>
        [Required]
        [Column("NMTP", TypeName = "varchar(80)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает специальное наименование.
        /// </summary>
        [Column("NMTPSP", TypeName = "varchar(150)")]
        public string SpecialName { get; set; }
    }
}
