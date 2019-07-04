using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность события ремонта
    /// </summary>
    [Table("EKZRM")]
    public class EventRm
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>   
        [Key]
        [Column("IDEKZRM", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает № паспорта <see cref="Metr.Ekz"/>.
        /// </summary>              
        [ForeignKey("IDEKZ")]
        public Ekz Ekz { get; set; }

        /// <summary>
        /// Возвращает или задает место установки <see cref="Metr.InstallationLocation"/>.
        /// </summary> 
        [ForeignKey("IDSPMU")]
        public InstallationLocation InstallationLocation { get; set; }

        /// <summary>
        /// Возвращает или задает группу СИ <see cref="Metr.GroupMi"/>.
        /// </summary>
        [ForeignKey("IDGRSI")]
        public GroupMi GroupsMi { get; set; }

        /// <summary>
        /// Возвращает или задает ремонтир./ калибр.организацию / подразделение, факт <see cref="Metr.Organization"/>.
        ///  </summary>          
        [ForeignKey("IDFRPD")]
        public Organization OrganizationPerformingWork { get; set; }

        /// <summary>
        /// Возвращает или задает место ремонта <see cref="Metr.ServicePlace"/>.
        ///  </summary> 
        [ForeignKey("IDSPMPOB")]
        public ServicePlace ServicePlace { get; set; }

        /// <summary>
        /// Возвращает или задает ремонтника <see cref="Metr.Person"/>.
        /// </summary> 
        [ForeignKey("IDPRSN")]
        public Person PersonExecutor { get; set; }

        /// <summary>
        /// Возвращает или задает вид очередного ремонта <see cref="Metr.TypeRm"/>. 
        /// </summary>
        [ForeignKey("IDSPVDR")]
        public TypeRm TypeRm { get; set; }

        /// <summary>
        /// Возвращает или задает ремонтный цикл <see cref="Metr.TypeCycleRm"/>.
        /// </summary>
        [ForeignKey("IDSPVDRC")]
        public TypeCycleRm TypeCycleRm { get; set; }

        /// <summary>
        /// Возвращает или задает событие МК  <see cref="Metr.EventMc"/>.
        /// </summary>
        [ForeignKey("IDEKZMK")]
        public EventMc EventMc { get; set; }

        /// <summary>
        /// Возвращает или задает событие ТО <see cref="Metr.EventMa"/>.
        /// </summary>  
        [ForeignKey("IDEKZTO")]
        public EventMa EventMa { get; set; }

        /// <summary>
        /// Возвращает или задает собитие отказа <see cref="Metr.EventFault"/>.
        ///  </summary> 
        [ForeignKey("IDEKZOT")]
        public EventFault EventFault { get; set; }

        /// <summary>
        /// Возвращает или задает получателя  <see cref="Metr.Person"/>.
        ///  </summary> 
        [ForeignKey("IDPRSNVD")]
        public Person PersonRecipient { get; set; }

        /// <summary>
        /// Возвращает или задает период ремонта, мес.
        /// </summary>
        [Column("PRRM", TypeName = "int")]
        public int? IntertestingInterval { get; set; }

        /// <summary>
        /// Возвращает или задает дату окончания ремонта.
        /// </summary>
        [Column("DTORM", TypeName = "datetime")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Возвращает или задает плановую дату ремонта.
        /// </summary>
        [Column("DTRMPL", TypeName = "datetime")]
        public DateTime? DatePlanned { get; set; }

        /// <summary>
        /// Возвращает или задает дату приемки.
        /// </summary>
        [Column("DTPRR", TypeName = "datetime")]
        public DateTime? DateAcceptance { get; set; }

        /// <summary>
        /// Возвращает или задает дату выдачи.
        /// </summary>
        [Column("DTVDR", TypeName = "datetime")]
        public DateTime? DateIssue { get; set; }

        /// <summary>
        /// Возвращает или задает позицию в цикле.
        /// </summary>
        [Column("PZRC", TypeName = "int")]
        public int? PositionQueue { get; set; }

        /// <summary>
        /// Возвращает или задает характеристика ремонта
        /// </summary>
        [Column("HRRM", TypeName = "varchar(150)")]
        public string WorcPerformed { get; set; }

        /// <summary>
        /// Возвращает или задает стоимость.
        ///  </summary> 
        [Column("STMK", TypeName = "money")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// Возвращает или задает дополнительную стоимость.
        /// </summary>
        [Column("STTODP", TypeName = "money")]
        public decimal? CostAdditional { get; set; }

        /// <summary>
        /// Возвращает или задает наценку за срочность.
        /// </summary>
        [Column("NCSRRM", TypeName = "money")]
        public decimal? CostUrgency { get; set; }

        /// <summary>
        /// Возвращает или задает сдавшего  <see cref="Metr.Person"/>.
        /// </summary>
        [ForeignKey("idprsnsd")]
        public Person PersonPassed { get; set; }

        /// <summary>
        /// Возвращает или задает принявшего <see cref="Metr.Person"/>.
        /// </summary>
        [ForeignKey("idprsnpr")]
        public Person PersonAccepted { get; set; }

        /// <summary>
        /// Возвращает или задает выдавшего  <see cref="Metr.Person"/>.
        /// </summary>
        [ForeignKey("idprsnvy")]
        public Person PersonGiveOut { get; set; }

        /// <summary>
        /// Возвращает или задает техническое состояние в момент приемки <see cref="Metr.ConditionTechnical"/>.
        ///  </summary> 
        [ForeignKey("idsptsmp")]
        public ConditionTechnical SnapshotConditionTechnical { get; set; }

        /// <summary>
        /// Возвращает или задает штатное состояние в момент приемки <see cref="Metr.ConditionStandart"/>.
        ///  </summary> 
        [ForeignKey("idspssmp")]
        public ConditionStandart SnapshotConditionStandart { get; set; }

        /// <summary>
        /// Возвращает или задает (НЕ ЯСНО, ЧТО ЭТО).
        /// </summary>
        [Browsable(false)]
        [Column("DTMAXR", TypeName = "datetime")]
        public DateTime? _Filed { get; set; }
    }
}