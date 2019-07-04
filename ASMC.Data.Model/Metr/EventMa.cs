using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность технического обслуживания
    /// </summary>
    [Table("EKZTO")]
    public class EventMa
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>  
        [Key]
        [Column("IDEKZTO", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает № паспорта <see cref="Metr.Ekz"/>.
        /// </summary>   
        [ForeignKey("IDEKZ")]
        public Ekz Ekz { get; set; }

        /// <summary>
        /// Возвращает или задает место остуществления деятельности <see cref="Metr.InstallationLocation"/>.
        /// </summary>
        [ForeignKey("IDSPMU")] public InstallationLocation Installationlocation;

        /// <summary>
        /// Возвращает или задает группу СИ <see cref="Metr.GroupMi"/>.
        /// </summary>
        [ForeignKey("IDGRSI")]
        public GroupMi GroupsMi { get; set; }

        /// <summary>
        /// Возвращает или задает организацию/подр.– исполнитель, факт <see cref="Metr.Organization"/>.
        /// </summary>
        [ForeignKey("IDFRPD")]
        public Organization OrganizationPerformingWork { get; set; }

        /// <summary>
        /// Возвращает или задает место обслуживания <see cref="Metr.ServicePlace"/>.
        /// </summary>
        [ForeignKey("IDSPMPOB")]
        public ServicePlace ServicePlace { get; set; }

        /// <summary>
        /// Возвращает или задает исполнитель  <see cref="Metr.Person"/>.
        /// </summary>
        [ForeignKey("IDPRSN")]
        public Person PersonExecutor { get; set; }

        /// <summary>
        /// Возвращает или задает вид ТО <see cref="Metr.TypeMa"/>.
        /// </summary> 
        [ForeignKey("IDSPVDTO")]
        public TypeMa TypeMa { get; set; }

        /// <summary>
        /// Возвращает или задает событие МК <see cref="Metr.EventMc"/>.
        /// </summary> 
        [ForeignKey("IDEKZMK")]
        public EventMc EventMc { get; set; }

        /// <summary>
        /// Возвращает или задает событие ремонта <see cref="Metr.EventRm"/>.
        /// </summary>
        [ForeignKey("IDEKZRM")]
        public EventRm EventRm { get; set; }

        /// <summary>
        /// Возвращает или задает собитие отказа <see cref="Metr.EventFault"/>.
        ///  </summary> 
        [ForeignKey("IDEKZOT")]
        public EventFault EventFault
        { get; set; }

        /// <summary>
        /// Возвращает или задает получатиля <see cref="Metr.Person"/>.
        /// </summary>
        [ForeignKey("IDPRSNVD")]
        public Person PersonRecipient { get; set; }

        /// <summary>
        /// Возвращает или задает период ТО, мес.
        /// </summary>
        [Column("PRTO", TypeName = "int")]
        public int? IntertestingInterval { get; set; }

        /// <summary>
        /// Возвращает или задает дату.
        /// </summary>
        [Column("DTTO", TypeName = "datetime")]
        public DateTime? DateFk { get; set; }

        /// <summary>
        /// Возвращает или задает плановая дату.
        /// </summary>
        [Column("DTPLTO", TypeName = "datetime")]
        public DateTime? DatePlanned { get; set; }

        /// <summary>
        /// Возвращает или задает дату приёмки.
        /// </summary>
        [Column("DTPRTO", TypeName = "datetime")]
        public DateTime? DateAcceptance { get; set; }

        /// <summary>
        /// Возвращает или задает дату выдачи из ТО.
        /// </summary>
        [Column("DTVDTO", TypeName = "datetime")]
        public DateTime? DateIssue { get; set; }

        /// <summary>
        /// Возвращает или задает стоимость.
        /// </summary>
        [Column("STTO", TypeName = "money")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// Возвращает или задает дополнительную стоимость.
        /// </summary>
        [Column("STTODP", TypeName = "money")]
        public decimal? CostAdditional { get; set; }

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
        /// </summary>
        [ForeignKey("idsptsmp")]
        public ConditionTechnical SnapshotConditionTechnical
        { get; set; }

        /// <summary>
        /// Возвращает или задает штатное состояние в момент приемки <see cref="Metr.ConditionStandart"/>.
        /// </summary>
        [ForeignKey("idspssmp")]
        public ConditionStandart SnapshotConditionStandart
        { get; set; }

        /// <summary>
        /// Возвращает или задает (НЕ ЯСНО, ЧТО ЭТО).
        /// </summary>   
        [Browsable(false)]
        [Column("DTMAXTO", TypeName = "datetime")]
        public DateTime? _Filed { get; set; }
    }
}