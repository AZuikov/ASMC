using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Utils.Data;

namespace ASMC.Data.Model.Metr
{
    [Procedure("dbo.up_gr_EkzMkSelect", Operation = StoredProcedureOp.SelectMany)]
    public class EventMc
    {
        /// <summary>
        /// № карточки МК
        /// </summary>  
        [Key]
        [Column("IDEKZMK", TypeName = "int")]
        public int EventMcId { get; private set; }

        /// <summary>
        /// № паспорта
        /// </summary>                  
        [ForeignKey(nameof(Ekz))]
        [Column("IDEKZ", TypeName = "int")]
        public int PassportId { get; set; }

        public Ekz Ekz { get; set; }

        /// <summary>
        ///  Место установки
        /// </summary> 
        [ForeignKey(nameof(Installationlocation))]
        [Column("IDSPMU", TypeName = "int")]
        public int InstallationlocationId { get; set; }

        public Installationlocation Installationlocation { get; set; }

        [Column("IDGRSI", TypeName = "int")] public int id1 { get; set; }

        [Column("IDKSPRL", TypeName = "int")] public int id2 { get; set; }
        [Column("IDSPVDMK", TypeName = "int")] public int id3 { get; set; }

        /// <summary>
        /// Цикл МК
        ///  </summary> 
        [Column("IDSPVDMC", TypeName = "int")]
        public int id4 { get; set; }

        /// <summary>
        /// Пов./ калибр.организация / подразделение, факт
        ///  </summary>                   
        [ForeignKey(nameof(OrgPerformingWork))]
        [Column("IDFRPD", TypeName = "int")]
        public int OrgPerformingWorkId { get; set; }

        public OrganizationsDivision OrgPerformingWork { get; set; }

        /// <summary>
        ///Место МК
        ///  </summary> 
        [ForeignKey(nameof(ServicePlace))]
        [Column("IDSPMPOB", TypeName = "int")]
        public int ServicePlaceId { get; set; }

        public ServicePlace ServicePlace { get; set; }

        /// <summary>
        /// Поверитель
        /// </summary> 
        [ForeignKey(nameof(Virefier))]
        [Column("IDPRSN", TypeName = "int")]
        public int VirefierId { get; set; }

        public Person Virefier { get; set; }

        [Column("IDSPKMMK", TypeName = "int")] public int id5 { get; set; }
        [Column("IDEKZRM", TypeName = "int")] public int id6 { get; set; }

        /// <summary>
        /// Вид клейма
        ///  </summary> 
        [Column("IDSPVDKL", TypeName = "int")]
        public int id7 { get; set; }

        /// <summary>
        ///Получил
        ///  </summary>   
        [ForeignKey(nameof(Recipient))]
        [Column("IDPRSNVD", TypeName = "int")]
        public int RecipientId { get; set; }

        public Person Recipient { get; set; }

        [Column("IDEKZTO", TypeName = "int")] public int id8 { get; set; }
        [Column("IDEKZOT", TypeName = "int")] public int id9 { get; set; }

        /// <summary>
        /// Номер заявки на МК
        ///  </summary> 
        [Column("NNZVPV", TypeName = "int")]
        public int id10 { get; set; }

        /// <summary>
        ///  Шифр клейма
        ///  </summary> 
        [Column("SHFKL", TypeName = "varchar(20)")]
        public int id11 { get; set; }

        /// <summary>
        ///  Номер наклейки
        ///  </summary> 
        [Column("NNNKL", TypeName = "int")]
        public int id12 { get; set; }

        /// <summary>
        /// Период МК, мес.
        ///  </summary> 
        [Column("PRMK", TypeName = "int")]
        public int IntertestingInterval { get; set; }

        /// <summary>
        /// Фактическая дата МК
        ///  </summary> 
        [Column("DTMKFK", TypeName = "datetime")]
        public DateTime DateMcFk { get; set; }

        /// <summary>
        /// Плановая дата
        ///  </summary> 
        [Column("DTMKPL", TypeName = "datetime")]
        public DateTime DateMcPlanned { get; set; }

        /// <summary>
        /// Дата приемки
        ///  </summary> 
        [Column("DTPRM", TypeName = "datetime")]
        public DateTime DateAcceptance { get; set; }

        /// <summary>
        /// Дата выдачи
        ///  </summary> 
        [Column("DTVDM", TypeName = "datetime")]
        public DateTime DateIssue { get; set; }

        /// <summary>
        /// Годен: да / нет
        ///  </summary> 
        [Column("GDN", TypeName = "bit")]
        public bool Fitness { get; set; }

        /// <summary>
        /// Позиция в цикле
        ///  </summary> 
        [Column("PZMC", TypeName = "int")]
        public int id13 { get; set; }

        /// <summary>
        /// Стоимость
        ///  </summary> 
        [Column("STMK", TypeName = "money")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Стоимость доп.
        ///  </summary> 
        [Column("STMKDP", TypeName = "money")]
        public decimal CostAdditional { get; set; }

        /// <summary>
        ///  Наценка за срочность
        ///  </summary> 
        [Column("NCSRMK", TypeName = "money")]
        public decimal CostUrgency { get; set; }

        /// <summary>
        /// Сдал
        ///  </summary>
        [ForeignKey(nameof(PersonPassed))]
        [Column("idprsnsd", TypeName = "int")]
        public int PersonPassedId { get; set; }

        public Person PersonPassed { get; set; }

        /// <summary>
        /// Принял
        ///  </summary>    
        [ForeignKey(nameof(PersonAccepted))]
        [Column("idprsnpr", TypeName = "int")]
        public int PersonAcceptedId { get; set; }

        public Person PersonAccepted { get; set; }

        /// <summary>
        /// Выдал
        ///  </summary>                  
        [ForeignKey(nameof(GiveOutPerson))]
        [Column("idprsnvy", TypeName = "int")]
        public int GiveOutPersonId { get; set; }

        public Person GiveOutPerson { get; set; }

        /// <summary>
        /// Техническое состояние в момент приёмки
        ///  </summary> 
        [Column("idsptsmp", TypeName = "int")]
        [ForeignKey(nameof(SnapshotTechnicalCondition))]
        public int SnapshotTechnicalConditionId { get; set; }

        public TechnicalCondition SnapshotTechnicalCondition { get; set; }

        /// <summary>
        ///  Штатное состояние в момент приёмки
        ///  </summary> 
        [Column("idspssmp", TypeName = "int")]
        [ForeignKey(nameof(SnapshotNormalState))]
        public int SnapshotNormalStateId { get; set; }

        public NormalState SnapshotNormalState { get; set; }
        // [Column("DTMAXM", TypeName = "")]
    }
}