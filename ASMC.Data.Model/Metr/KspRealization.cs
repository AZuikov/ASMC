using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    ///  Сущность реализации КСП.
    /// </summary>
    [Table("KSPRL")]
    public class KspRealization
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDKSPRL", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает КСП.  
        /// </summary>
        [Column("IDKSP", TypeName = "int")]
        public Ksp Ksp
        { get; set; }

        /// <summary>
        /// Возвращает или задает код реализации КСП.
        /// </summary>
        [Column("KDKSPRL", TypeName = "varchar(11)")]
        public string Name { get; set; }
    }
}
