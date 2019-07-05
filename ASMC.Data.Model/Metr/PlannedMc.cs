using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{

    /// <summary>
    /// Сущность планового события МК.
    /// </summary>
    [Table("EKZMCP")]
    public class PlannedMc
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDEKZMCP", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает id Экземпляра <see cref="Metr.Ekz"/>.
        /// </summary>
        [ForeignKey("IDEKZ")]
        public Ekz Ekz {get;set;}
        /// <summary>
        /// Возвращает или задает цикл МК <see cref="Metr.CyclePeriodeMc"/>.
        /// </summary>
        [ForeignKey("IDTPRMCP")]
        public CyclePeriodeMc CyclePeriodeMc
        { get; set; }
        /// <summary>
        /// Возвращает или задает дату МК.
        /// </summary>
        [Column("DTMKPLO", TypeName = "datetime")]
        public DateTime? Date{ get; set; }
        /// <summary>
        /// Возвращает или задает позицию очер. МК в цикле.
        /// </summary>
        [Column("PZMCO", TypeName = "int")]
        public int? PositionOfQueue { get; set; }
        /// <summary>
        /// Возвращает или задает вид очередного МК <see cref="Metr.TypeMc"/>.
        /// </summary>      
        [ForeignKey("IDSPVDMK")]
        public TypeMc TypeMc
        { get; set; }

    }
}
