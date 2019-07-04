using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Utils.Data;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность события МК.
    /// </summary>
    [Table("EKZMK")]
    [StoredProcedure("dbo.up_gr_EkzMkSelect", Operation = StoredProcedureOp.SelectMany)]
    public class EventMc
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>  
        [Key]
        [Column("IDEKZMK", TypeName = "int")]
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
        /// Возвращает или задает реализацию КСП <see cref="Metr.KspRealization"/>.
        /// </summary>
        [ForeignKey("IDKSPRL")]
        public KspRealization KspRealization { get; set; }

        /// <summary>
        /// Возвращает или задает вид очередного МК <see cref="Metr.TypeMc"/>.
        /// </summary>
        [ForeignKey("IDSPVDMK")]
        public TypeMc TypeMc { get; set; }

        /// <summary>
        /// Возвращает или задает цикл МК <see cref="Metr.TypeCycleMc"/>.
        ///  </summary>
        [ForeignKey("IDSPVDMC")]
        public TypeCycleMc TypeCycleMc { get; set; }

        /// <summary>
        /// Возвращает или задает пов./ калибр.организацию / подразделение, факт <see cref="Metr.Organization"/>.
        ///  </summary>          
        [ForeignKey("IDFRPD")]
        public Organization OrganizationPerformingWork { get; set; }

        /// <summary>
        /// Возвращает или задает место МК <see cref="Metr.ServicePlace"/>.
        ///  </summary> 
        [ForeignKey("IDSPMPOB")]
        public ServicePlace ServicePlace { get; set; }

        /// <summary>
        /// Возвращает или задает поверителя <see cref="Metr.Person"/>.
        /// </summary> 
        [ForeignKey("IDPRSN")]
        public Person PersonExecutor { get; set; }

        /// <summary>
        /// Возвращает или задает комплексность МК <see cref="Metr.CompletenessMi"/>.
        /// </summary>
        [ForeignKey("IDSPKMMK")]
        public CompletenessMi CompletenessMi { get; set; }

        /// <summary>
        /// Возвращает или задает событие ремонта <see cref="Metr.EventRm"/>.
        /// </summary> 
        [ForeignKey("IDEKZRM")]
        public EventRm EventRm { get; set; }

        /// <summary>
        /// Возвращает или задает вид клейма <see cref="Metr.Stamp"/>.
        ///  </summary> 
        [Column("IDSPVDKL", TypeName = "int")]
        [ForeignKey("IDSPVDKL")]
        public Stamp Stamp { get; set; }

        /// <summary>
        /// Возвращает или задает получателя  <see cref="Metr.Person"/>.
        ///  </summary> 
        [ForeignKey("IDPRSNVD")]
        public Person PersonRecipient { get; set; }

        /// <summary>
        /// Возвращает или задает событие ТО <see cref="Metr.EventMa"/>.
        /// </summary>  
        [ForeignKey("IDEKZTO")]
        public EventMa EventMa
        { get; set; }

        /// <summary>
        /// Возвращает или задает собитие отказа <see cref="Metr.EventFault"/>.
        ///  </summary> 
        [ForeignKey("IDEKZOT")]
        public EventFault EventFault
        { get; set; }

        /// <summary>
        /// Возвращает или задает номер заявки на МК.
        ///  </summary> 
        [Column("NNZVPV", TypeName = "int")]
        public int? NumberRequest { get; set; }

        /// <summary>
        /// Возвращает или задает шифр клейма.
        ///  </summary> 
        [Column("SHFKL", TypeName = "varchar(20)")]
        public string StampCipher { get; set; }

        /// <summary>
        /// Возвращает или задает номер наклейки.
        ///  </summary> 
        [Column("NNNKL", TypeName = "int")]
        public int? StickerNumber { get; set; }

        /// <summary>
        /// Возвращает или задает период МК, мес.
        ///  </summary> 
        [Column("PRMK", TypeName = "int")]
        public int? IntertestingInterval { get; set; }

        /// <summary>
        /// Возвращает или задает дату МК.
        ///  </summary> 
        [Column("DTMKFK", TypeName = "datetime")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Возвращает или задает плановую дату МК.
        ///  </summary> 
        [Column("DTMKPL", TypeName = "datetime")]
        public DateTime? DatePlanned { get; set; }

        /// <summary>
        /// Возвращает или задает дату приемки.
        ///  </summary> 
        [Column("DTPRM", TypeName = "datetime")]
        public DateTime? DateAcceptance { get; set; }

        /// <summary>
        /// Возвращает или задает дату выдачи.
        ///  </summary> 
        [Column("DTVDM", TypeName = "datetime")]
        public DateTime? DateIssue { get; set; }

        /// <summary>
        /// Возвращает или задает результат МК (Годен: да / нет).
        ///  </summary> 
        [Column("GDN", TypeName = "bit")]
        public bool? Fitness { get; set; }

        /// <summary>
        /// Возвращает или задает позицию в цикле.
        ///  </summary> 
        [Column("PZMC", TypeName = "int")]
        public int? PositionQueue { get; set; }

        /// <summary>
        /// Возвращает или задает стоимость.
        ///  </summary> 
        [Column("STMK", TypeName = "money")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// Возвращает или задает дополнительную стоимость.
        ///  </summary> 
        [Column("STMKDP", TypeName = "money")]
        public decimal? CostAdditional { get; set; }

        /// <summary>
        /// Возвращает или задает наценку за срочность.
        ///  </summary> 
        [Column("NCSRMK", TypeName = "money")]
        public decimal? CostUrgency { get; set; }

        /// <summary>
        /// Возвращает или задает сдавшего  <see cref="Metr.Person"/>.
        ///  </summary>
        [ForeignKey("idprsnsd")]
        public Person PersonPassed { get; set; }

        /// <summary>
        /// Возвращает или задает принявшего  <see cref="Metr.Person"/>.
        ///  </summary> 
        [ForeignKey("idprsnpr")]
        public Person PersonAccepted { get; set; }

        /// <summary>
        /// Возвращает или задает выдавшего  <see cref="Metr.Person"/>.
        ///  </summary> 
        [ForeignKey("idprsnvy")]
        public Person PersonGiveOut { get; set; }

        /// <summary>
        /// Возвращает или задает техническое состояние в момент приёмки <see cref="Metr.ConditionTechnical"/>.
        ///  </summary>
        [ForeignKey("idsptsmp")]
        public ConditionTechnical SnapshotConditionTechnical { get; set; }

        /// <summary>
        /// Возвращает или задает штатное состояние в момент приёмки  <see cref="Metr.ConditionStandart"/>.
        ///  </summary>
        [ForeignKey("idspssmp")]
        public ConditionStandart SnapshotConditionStandart { get; set; }

        /// <summary>
        /// Возвращает или задает (НЕ ЯСНО, ЧТО ЭТО).
        /// </summary>
        [Browsable(false)]
        [Column("DTMAXM", TypeName = "datetime")]
        public DateTime? _Filed { get; set; }
    }
}