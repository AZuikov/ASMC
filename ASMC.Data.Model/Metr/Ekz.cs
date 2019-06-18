using AP.Utils.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Представляет модель экземпляра СИ.
    /// </summary>                               
    [Procedure("dbo.up_gr_EkzEdit_ls", Operation = StoredProcedureOp.Update)]
    [Procedure("dbo.up_gr_EkzSelect", Operation =  StoredProcedureOp.SelectMany)]
    [Procedure("dbo.up_gr_EkzCardSelect", KeyName = "vbr", KeyFormat = "ekz.idekz={0}")]

    public class Ekz: IEquatable<Ekz>, ICloneable

    {
        #region Properties
        /// <summary>
        /// id экземпляра (№ паспорта)
        /// </summary>
        [Key]
        [Column("IDEKZ", TypeName = "int")]
        public int PassportId { get; private set; }
        /// <summary>
        /// Id Типоразмера
        /// </summary>
        [Required]
        [ForeignKey(nameof(StandardSizeMi))]
        [Column("IDTPRZ", TypeName = "int")]
        public int StandardSizeMiId { get; set; }
        /// <summary>
        /// Типоразмер
        /// </summary>
        public StandardSizeMi StandardSizeMi { get; set; }
        /// <summary>
        /// Id Категории СИ
        /// </summary>
        [ForeignKey(nameof(СategoryMi))]
        [Column("IDSPKT", TypeName = "int")]
        public int СategoryMiId { get; set; }
        /// <summary>
        /// Категория СИ
        /// </summary>
        public СategoryMi СategoryMi { get; set; }
        /// <summary>
        /// id  Места установки
        /// </summary>
        [ForeignKey(nameof(Installationlocation))]
        [Column("IDSPMU", TypeName = "int")]
        public int InstallationlocationId{ get; set; }
        /// <summary>
        /// Место установки
        /// </summary>
        public Installationlocation Installationlocation { get; set; }
        /// <summary>
        /// Id Штатное состояние (текущее)
        /// </summary>
        [ForeignKey(nameof(NormalState))]
        [Column("IDSPSS", TypeName = "int")]
        public int NormalStateId { get; set; }
        /// <summary>
        /// Штатное состояние (текущее)
        /// </summary>
        public NormalState NormalState { get; set; }
        /// <summary>
        /// Id Техническое состояние (текущее)
        /// </summary>
        [Column("IDSPTS", TypeName = "int")]
        public int TechnicalConditionId { get; set; }
        /// <summary>
        /// Техническое состояние (текущее)
        /// </summary>
        public TechnicalCondition TechnicalCondition { get; set; }
        /// <summary>
        /// id сферы государственного регулирования обеспечения единства измерений
        /// </summary>
        [ForeignKey(nameof(Ssreum))]
        [Column("IDSPSHMK", TypeName = "int")]
        public int SsreumId { get; set; }
        /// <summary>
        /// сфера государственного регулирования обеспечения единства измерений
        /// </summary>
        public Ssreum Ssreum { get; set; }
        /// <summary>
        /// id области применения СИ
        /// </summary>
        [ForeignKey(nameof(ApplicationAreaMi))]
        [Column("IDSPOP", TypeName = "int")]
        public int ApplicationAreaMiId { get; set; }
        /// <summary>
        /// области применения СИ
        /// </summary>
        public ApplicationAreaMi ApplicationAreaMi { get; set; }
        /// <summary>
        /// id персоны ответственного за МО
        /// </summary>
        [ForeignKey(nameof(PersonResponsibleMa))]
        [Column("IDPRSN", TypeName = "int")]
        public int PersonResponsibleMaId { get; set; }
        /// <summary>
        /// Oтветственный за МО
        /// </summary>
        public Person PersonResponsibleMa { get; set; }
        /// <summary>
        /// id владелеца
        /// </summary>
        [ForeignKey(nameof(Owner))]
        [Column("IDFRPDV", TypeName = "int")]
        public int OwnerId { get; set; }
        /// <summary>
        /// Владелец
        /// </summary>
        public OrganizationsDivision Owner { get; set; }
        /// <summary>
        /// id Изготовителя
        /// </summary>
        [ForeignKey(nameof(Manufacturer))]
        [Column("IDFRPDIZ", TypeName = "int")]
        public int ManufacturerId { get; set; }
        /// <summary>
        /// Изготовитель
        /// </summary>
        public OrganizationsDivision Manufacturer { get; set; }
        /// <summary>
        /// id подразделения ответственного за МО
        /// </summary>
        [ForeignKey(nameof(UnitResponsibleMi))]
        [Column("IDFRPDMO", TypeName = "int")]
        public int UnitResponsibleMiId{ get; set; }
        /// <summary>
        /// подразделения ответственное за МО
        /// </summary>
        public OrganizationsDivision UnitResponsibleMi { get; set; }
        /// <summary>
        /// Кол-во СИ
        /// </summary>
        [Required]
        [Column("KLSIPR", TypeName = "int")]
        public int MiNumber{ get; set; }
        /// <summary>
        /// Заводской №
        /// </summary>
        [Required]
        [Column("NNZV", TypeName = "varchar(max)")]
        public string SerialNumber { get; set; }
        /// <summary>
        /// Инвентарный №
        /// </summary>
        [Column("NNIN", TypeName = "varchar(max)")]
        public string InventoryNumber { get; set; }
        /// <summary>
        /// Дата выпуска
        /// </summary>
        [Column("DTVP", TypeName = "datetime")]
        public DateTime DateIssue { get; set; }
        /// <summary>
        /// Дата ввода в эксплуатацию
        /// </summary>
        [Column("DTVVEK", TypeName = "datetime")]
        public DateTime CommissioningDate { get; set; }
        /// <summary>
        /// Дата сдачи драгметаллов
        /// </summary>
        [Column("DTSDDR", TypeName = "datetime")]
        public DateTime SubmissionDatePreciousMetals { get; set; }
        /// <summary>
        /// Дата списания
        /// </summary>
        [Column("DTSPS", TypeName = "datetime")]
        public DateTime DateWriteOff { get; set; }
        /// <summary>
        /// Первоначальная стоимость
        /// </summary>
        [Column("PNCHST", TypeName = "money")]
        public decimal InitialCost { get; set; }
        /// <summary>
        /// Состояние при покупке
        /// </summary>
        [Column("SSPK", TypeName = "varchar(max)")]
        public string PurchaseCondition { get; set; }
        /// <summary>
        /// id по постановлению №250
        /// </summary>
        [ForeignKey(nameof(Resolution250))]
        [Column("IDSPPP250", TypeName = "int")]
        public int Resolution250Id { get; set; }
        /// <summary>
        /// Соответстие по простоновлению №250
        /// </summary>
        public Resolution250 Resolution250 { get; set; }
        /// <summary>
        /// id характеристики по доп. класс. 1
        /// </summary>
        [ForeignKey(nameof(CharacteristicsAdditionalClass1))]
        [Column("IDSPDPKL1", TypeName = "int")]
         public int CharacteristicsAdditionalClass1Id { get; set; }
        /// <summary>
        /// Характеристики по доп. класс. 1
        /// </summary>
        public CharacteristicsAdditionalClass1 CharacteristicsAdditionalClass1 { get; set; }
        /// <summary>
        /// id характеристики по доп. класс. 2
        /// </summary>
        [ForeignKey(nameof(CharacteristicsAdditionalClass2))]
        [Column("IDSPDPKL2", TypeName = "int")]
        public int CharacteristicsAdditionalClass2Id { get; set; }
        /// <summary>
        /// Характеристики по доп. класс. 2
        /// </summary>
        public CharacteristicsAdditionalClass2 CharacteristicsAdditionalClass2 { get; set; }
        /// <summary>
        /// id характеристики по доп. класс. 3
        /// </summary>
        [ForeignKey(nameof(CharacteristicsAdditionalClass3))]
        [Column("IDSPDPKL3", TypeName = "int")]
        public int CharacteristicsAdditionalClass3Id { get; set; }
        /// <summary>
        /// Характеристики по доп. класс. 3
        /// </summary>
        public CharacteristicsAdditionalClass3 CharacteristicsAdditionalClass3 { get; set; }
        /// <summary>
        /// Доп. поле
        /// </summary>
        [Column("DPPLEKZ", TypeName = "varchar(max)")]
        public string AdditionalField{ get; set; }
        /// <summary>
        /// Дополнительные сведения
        /// </summary>
        [Column("DSEKZ", TypeName = "varchar(max)")]
        public string AdditionalInformation { get; set; }
        /// <summary>
        /// Внесено в Госреестр
        /// </summary>
        [Column("GRVN", TypeName = "bit")]
        public bool StateRegistry { get; set; }
        /// <summary>
        /// Номер госреестра 
        /// </summary>
        [Column("NNEKZGR", TypeName = "varchar(max)")]
        public string RegisterNumber { get; set; }


        #endregion

        #region IEquatable

        public bool Equals(Ekz other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(AdditionalField, other.AdditionalField) && string.Equals(AdditionalInformation, other.AdditionalInformation) && ApplicationAreaMiId == other.ApplicationAreaMiId && CharacteristicsAdditionalClass1 == other.CharacteristicsAdditionalClass1 && CharacteristicsAdditionalClass2 == other.CharacteristicsAdditionalClass2 && CharacteristicsAdditionalClass3 == other.CharacteristicsAdditionalClass3 && CommissioningDate.Equals(other.CommissioningDate) && DateIssue.Equals(other.DateIssue) && DateWriteOff.Equals(other.DateWriteOff) && InitialCost == other.InitialCost && InstallationlocationId == other.InstallationlocationId && string.Equals(InventoryNumber, other.InventoryNumber) && ManufacturerId == other.ManufacturerId && MiNumber == other.MiNumber && NormalStateId == other.NormalStateId && OwnerId == other.OwnerId && PassportId == other.PassportId && PersonResponsibleMaId == other.PersonResponsibleMaId && string.Equals(PurchaseCondition, other.PurchaseCondition) && string.Equals(RegisterNumber, other.RegisterNumber) && Resolution250Id == other.Resolution250Id && string.Equals(SerialNumber, other.SerialNumber) && SsreumId == other.SsreumId && StandardSizeMiId == other.StandardSizeMiId && StateRegistry == other.StateRegistry && SubmissionDatePreciousMetals.Equals(other.SubmissionDatePreciousMetals) && TechnicalConditionId == other.TechnicalConditionId && UnitResponsibleMiId == other.UnitResponsibleMiId && СategoryMiId == other.СategoryMiId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Ekz) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PassportId; 
                hashCode = (hashCode * 397) ^ (AdditionalInformation != null ? AdditionalInformation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ApplicationAreaMiId;
                hashCode = (hashCode * 397) ^ CharacteristicsAdditionalClass1Id;
                hashCode = (hashCode * 397) ^ CharacteristicsAdditionalClass2Id;
                hashCode = (hashCode * 397) ^ CharacteristicsAdditionalClass3Id;
                hashCode = (hashCode * 397) ^ CommissioningDate.GetHashCode();
                hashCode = (hashCode * 397) ^ DateIssue.GetHashCode();
                hashCode = (hashCode * 397) ^ DateWriteOff.GetHashCode();
                hashCode = (hashCode * 397) ^ InitialCost.GetHashCode();
                hashCode = (hashCode * 397) ^ InstallationlocationId;
                hashCode = (hashCode * 397) ^ (InventoryNumber != null ? InventoryNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ManufacturerId;
                hashCode = (hashCode * 397) ^ MiNumber;
                hashCode = (hashCode * 397) ^ NormalStateId;
                hashCode = (hashCode * 397) ^ OwnerId;
                hashCode = (hashCode * 397) ^ PassportId;
                hashCode = (hashCode * 397) ^ PersonResponsibleMaId;
                hashCode = (hashCode * 397) ^ (PurchaseCondition != null ? PurchaseCondition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RegisterNumber != null ? RegisterNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Resolution250Id;
                hashCode = (hashCode * 397) ^ (SerialNumber != null ? SerialNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SsreumId;
                hashCode = (hashCode * 397) ^ StandardSizeMiId;
                hashCode = (hashCode * 397) ^ StateRegistry.GetHashCode();
                hashCode = (hashCode * 397) ^ SubmissionDatePreciousMetals.GetHashCode();
                hashCode = (hashCode * 397) ^ TechnicalConditionId;
                hashCode = (hashCode * 397) ^ UnitResponsibleMiId;
                hashCode = (hashCode * 397) ^ СategoryMiId;
                return hashCode;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

        public Ekz(int passportId)
        {
            PassportId = passportId;
        }

        public Ekz()
        {

        }
    }
}
