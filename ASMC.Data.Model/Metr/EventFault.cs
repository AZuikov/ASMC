using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность cобытия отказов
    /// </summary>
    [Table("EKZOT")]
    public class EventFault
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>  
        [Key]
        [Column("IDEKZOT", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает № паспорта <see cref="Metr.Ekz"/>.
        /// </summary>   
        [ForeignKey("IDEKZ")]
        public Ekz Ekz { get; set; }

        /// <summary>
        /// Возвращает или задает место проведения <see cref="Metr.InstallationLocation"/>.
        /// </summary> 
        [ForeignKey("IDSPMU")]
        public InstallationLocation InstallationLocation { get; set; }

        /// <summary>
        /// Возвращает или задает событие ремонта <see cref="Metr.EventRm"/>.
        /// </summary>    
        [ForeignKey("IDEKZRM")]
        public EventRm EventRm { get; set; }

        /// <summary>
        /// Возвращает или задает событие МК <see cref="Metr.EventMc"/>.
        /// </summary>  
        [ForeignKey("IDEKZMK")]
        public EventMc EventMc { get; set; }

        /// <summary>
        /// Возвращает или задает событие ТО <see cref="Metr.EventMa"/>.
        /// </summary>  
        [ForeignKey("IDEKZTO")]
        public EventMa EventMa { get; set; }

        /// <summary>
        /// Возвращает или задает дату обнаружения.
        /// </summary>
        [Column("DTOBOT", TypeName = "datetime")]
        public DateTime? DateOfDetection { get; set; }

        /// <summary>
        /// Возвращает или задает дату устранения.
        /// </summary>
        [Column("DTYSOT", TypeName = "datetime")]

        public DateTime? DateOfElimination { get; set; }

        /// <summary>
        /// Возвращает или задает наработку на отказ, час.
        /// </summary>
        [Column("NROT", TypeName = "int")]
        public int? TimeNoFailure { get; set; }

        /// <summary>
        /// Возвращает или задает время устранения, час.
        /// </summary>
        [Column("VRYSOT", TypeName = "int")]
        public int? TimeOfElimination { get; set; }

        /// <summary>
        /// Возвращает или задает сущность дефекта.
        /// </summary>
        [Column("SSDF", TypeName = "varchar(200)")]
        public string DefectOfDescription { get; set; }

        /// <summary>
        /// Возвращает или задает причину отказа.
        /// </summary>
        [Column("PROT", TypeName = "varchar(200)")]
        public string Reason { get; set; }

        /// <summary>
        /// Возвращает или задает (НЕ ЯСНО, ЧТО ЭТО).
        /// </summary>   
         [Browsable(false)]
        [Column("DTMAXOT", TypeName = "datetime")]
        public DateTime? _Filed { get; set; }
    }
}