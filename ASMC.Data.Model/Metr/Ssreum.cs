using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "сферы государственного регулирования обеспечения единства измерений"
    /// </summary>
    [Table("SPSHMK")]
    public class Ssreum
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDSPSHMK", TypeName = "int")]
        public  int SsreumId { get; private set;}
        /// <summary>
        /// Cфера ГРОЕИ
        /// </summary>
        [Required]
        [Column("NMSHMK", TypeName = "varchar(max)")]
        public  string NameSsreum { get; set; }
        /// <summary>
        ///  Код сферы ГРОЕИ
        /// </summary>
        [Column("KDSHMK", TypeName = "varchar(max)")]
        public string CodeSsreum { get; set; }

    }
}
