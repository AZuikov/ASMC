using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Metr
{     
    /// <summary>
    ///  Сущность цикла периода ремонта.
    /// </summary>
    [Table("TPRRCP")]
    public class CyclePeriodeRm
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDTPRRCP", TypeName = "int")]
        public int? Id
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает типоразмер <see cref="Metr.StandardSizeMi"/>.
        /// </summary>
        [ForeignKey("IDTPRZ")]
        public StandardSizeMi StandardSizeMi
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает ремонтный цикл <see cref="Metr.TypeCycleRm"/>.
        /// </summary>
        [ForeignKey("IDSPVDRC")]
        public TypeCycleRm TypeCycleRm
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает группу СИ <see cref="Metr.GroupMi"/>.
        /// </summary>
        [ForeignKey("IDGRSI")]
        public GroupMi GroupsMi
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает период ремонта, мес.
        /// </summary>
        [Column("PRRM", TypeName = "int")]
        public int? Period
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает комментарий.
        /// </summary>
        [Column("KM", TypeName = "text")]
        public string Comment
        {
            get; set;
        }

    }
}
