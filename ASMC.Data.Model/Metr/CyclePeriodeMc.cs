using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    ///  Сущность цикла периода МК.
    /// </summary>
    [Table("TPRMCP")]
    public class CyclePeriodeMc
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDTPRMCP", TypeName = "int")]
        public int? Id
        { get; set;
        }
        /// <summary>
        /// Возвращает или задает типоразмер <see cref="Metr.StandardSizeMi"/>.
        /// </summary>
        [ForeignKey("IDTPRZ")]
        public StandardSizeMi StandardSizeMi { get; set; }
        /// <summary>
        /// Возвращает или задает вид цикла МК <see cref="Metr.TypeCycleMc"/>.
        /// </summary>
        [ForeignKey("IDSPVDMC")]
        public TypeCycleMc TypeCycleMc { get; set; }
        /// <summary>
        /// Возвращает или задает группу СИ <see cref="Metr.GroupMi"/>.
        /// </summary>
        [ForeignKey("IDGRSI")]
        public GroupMi GroupMi
        { get; set; }
        /// <summary>
        /// Возвращает или задает период МК, мес.
        /// </summary>
        [Column("PRMK", TypeName = "int")]
        public int? Period { get; set; }
        /// <summary>
        /// Возвращает или задает комментарий.
        /// </summary>
        [Column("KM", TypeName = "text")]
        public string Comment { get; set; }
    }
}
