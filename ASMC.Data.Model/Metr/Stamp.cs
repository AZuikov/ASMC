using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность клейма.
    /// </summary>
    [Table("SPVDKL")]
    public class Stamp
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPVDKL", TypeName = "int")]
        public int? StampId { get; set; }

        /// <summary>
        /// Возвращает или задает вид клейма.
        /// </summary>
        [Column("NMVDKL", TypeName = "varchar(40)")]
        public string Name { get; set; }

        /// <summary>
        /// Возвращает или задает код вида клейма.
        /// </summary>
        [Column("KDVDKL", TypeName = "int")]
        public int? Code { get; set; }
        /// <summary>
        /// Возвращает или задает флаг на операции с записью.
        /// </summary>     
        [Browsable(false)]
        [Column("FLOZ", TypeName = "int")]
        public int? FlagWrite
        {
            get; set;
        }
    }
}
