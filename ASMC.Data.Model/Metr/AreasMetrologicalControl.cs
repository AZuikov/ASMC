using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "сферы государственного регулирования обеспечения единства измерений"
    /// </summary>
    [Table("SPSHMK")]
    public class AreasMetrologicalControl
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPSHMK", TypeName = "int")]
        public  int? Id { get; set;}
        /// <summary>
        /// Cфера ГРОЕИ
        /// </summary>
        [Required]
        [Column("NMSHMK", TypeName = "varchar(50)")]
        public string Name { get; set; }
        /// <summary>
        ///  Код сферы ГРОЕИ
        /// </summary>
        [Column("KDSHMK", TypeName = "varchar(3)")]
        public string Code { get; set; }

    }
}
