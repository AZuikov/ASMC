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
    /// Сущность планового события ремонта.
    /// </summary>
    [Table("EKZRCP")]
    public class PlannedRm
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.  
        /// </summary>  
        [Key]
        [Browsable(false)]
        [Column("IDEKZRCP", TypeName = "int")]
        public int Id { get; set; }

        /// <summary>
        /// Возвращает или задает id Экземпляра <see cref="Metr.Ekz"/>.
        /// </summary>
        [ForeignKey("IDEKZ")]
        public Ekz Ekz { get; set; }
        /// <summary>
        /// Возвращает или задает цикл ремонта <see cref="Metr.CyclePeriodeRm"/>.
        /// </summary>
        [ForeignKey("IDTPRRCP")]
        public CyclePeriodeRm CyclePeriodeRm
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает дата очередного ремонта.
        /// </summary>
        [Column("DTRMPLO", TypeName = "datetime")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Возвращает или задает позиция в цикле очер. ремонта.
        /// </summary>
        [Column("PZRCO", TypeName = "int")]
        public int? PositionQueue { get; set; }

        /// <summary>
        /// Возвращает или задает вид очередного МК <see cref="Metr.TypeRm"/>.
        /// </summary>      
        [ForeignKey("IDSPVDR")]
        public TypeRm TypeRm { get; set; }
    }
}