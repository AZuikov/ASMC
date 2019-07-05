using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность группы СИ
    /// </summary>
    [Table("GRSI")]
    public class GroupMi
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDGRSI", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает код группы.
        /// </summary>
        [Column("KDGRSI", TypeName = "int")]
        public int? Code { get; set; }
        /// <summary>
        /// Возвращает или задает наименование группы.
        /// </summary>
        [Column("NMGRSI", TypeName = "varchar(250)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает метрологические характеристики группы.
        /// </summary>
        [Column("MHGRSI", TypeName = "varchar(230)")]
        public string Characteristics { get; set; }
        /// <summary>
        /// Возвращает или задает НД на поверочную схему.
        /// </summary>
        [Column("NDPVSH", TypeName = "varchar(150)")]
        public string DocumentOfVerifScheme { get; set; }
        /// <summary>
        /// Возвращает или задает НД на методику поверки.
        /// </summary>
        [Column("NDMTPV", TypeName = "varchar(150)")]
        public string DocumentVerifMethodology
        { get; set; }

    }
}
