
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Utils.Data;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность экземпляра СИ.
    /// </summary> 
    [Table("EKZ")]
    [StoredProcedure("dbo.up_gr_EkzEdit_ls", Operation = StoredProcedureOp.Update)]
    [StoredProcedure("dbo.up_gr_EkzSelect", Operation = StoredProcedureOp.SelectMany)]
    [StoredProcedure("dbo.up_gr_EkzCardSelect", KeyName = "vbr", KeyFormat = "ekz.idekz={0}")]
    public class Ekz

    {
        #region Properties

        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Column("IDEKZ", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает типоразмер <see cref="Metr.StandardSizeMi"/>.
        /// </summary>         
        [Required]
        [ForeignKey("IDTPRZ")] public StandardSizeMi StandardSizeMi { get; set; }

        /// <summary>
        /// Возвращает или задает категорию СИ <see cref="Metr.CategoryMi"/>. 
        /// </summary>   
        [ForeignKey("IDSPKT")]
        public CategoryMi CategoryMi { get; set; }

        /// <summary>
        /// Возвращает или задает место установки <see cref="Metr.InstallationLocation"/>.
        /// </summary>
        [ForeignKey("IDSPMU")]
        public InstallationLocation Installationlocation { get; set; }

        /// <summary>
        /// Возвращает или задает штатное состояние (текущее) <see cref="Metr.ConditionStandart"/>.
        /// </summary>
        [ForeignKey("IDSPSS")]
        public ConditionStandart ConditionOfStandart { get; set; }

        /// <summary>
        /// Возвращает или задает техническое состояние (текущее) <see cref="Metr.ConditionTechnical"/>.
        /// </summary>
        [ForeignKey("IDSPTS")]
        public ConditionTechnical ConditionOfTechnical { get; set; }

        /// <summary>
        /// Возвращает или задает сферу государственного
        /// регулирования обеспечения единства измерений <see cref="Metr.AreasMetrologicalControl"/>.
        /// </summary>
        [ForeignKey("IDSPSHMK")]
        public AreasMetrologicalControl AreasOfMetrologicalControl { get; set; }

        /// <summary>
        /// Возвращает или задает области применения СИ <see cref="Metr.AreaApplication"/>.
        /// </summary>
        [ForeignKey("IDSPOP")]
        public AreaApplication AreaApplication { get; set; }

        /// <summary>
        /// Возвращает или задает ответственного за МО <see cref="Metr.Person"/>.
        /// </summary>
        [ForeignKey("IDPRSN")]
        public Person PersonResponsibleMs { get; set; }

        /// <summary>
        /// Возвращает или задает владелеца <see cref="Metr.Organization"/>.
        /// </summary>
        [ForeignKey("IDFRPDV")]
        public Organization OrganizationOfOwner { get; set; }

        /// <summary>
        /// Возвращает или задает изготовителя <see cref="Metr.Organization"/>.
        /// </summary>

        [ForeignKey("IDFRPDIZ")]
        public Organization OrganizationOfManufacturer { get; set; }

        /// <summary>
        /// Возвращает или задает подразделение ответственное за МО <see cref="Metr.Organization"/>.
        /// </summary>
        [ForeignKey("IDFRPDMO")]
        public Organization OrganizationOfResponsibleMs { get; set; }

        /// <summary>
        /// Возвращает или задает кол-во СИ.
        /// </summary>
        [Required]
        [Column("KLSIPR", TypeName = "int")]
        public int? NumberOfMi { get; set; }

        /// <summary>
        /// Возвращает или задает заводской №.
        /// </summary>
        [Required]
        [Column("NNZV", TypeName = "varchar(30)")]
        public string NumberOfSerial
        { get; set; }

        /// <summary>
        /// Возвращает или задает инвентарный №.
        /// </summary>
        [Column("NNIN", TypeName = "varchar(30)")]
        public string NumberOfInventory
        { get; set; }

        /// <summary>
        /// Возвращает или задает дату выпуска.
        /// </summary>
        [Column("DTVP", TypeName = "datetime")]
        public DateTime? DateOfIssue
        { get; set; }

        /// <summary>
        /// Возвращает или задает дату ввода в эксплуатацию.
        /// </summary>
        [Column("DTVVEK", TypeName = "datetime")]
        public DateTime? DateOfCommissioning
        { get; set; }

        /// <summary>
        /// Возвращает или задает дату сдачи драгметаллов.
        /// </summary>
        [Column("DTSDDR", TypeName = "datetime")]
        public DateTime? DateOfSubmissionPreciousMetals
        { get; set; }

        /// <summary>
        /// Возвращает или задает дату списания.
        /// </summary>
        [Column("DTSPS", TypeName = "datetime")]
        public DateTime? DateOfWriteOff
        { get; set; }

        /// <summary>
        /// Возвращает или задает первоначальную стоимость.
        /// </summary>
        [Column("PNCHST", TypeName = "money")]
        public decimal? CostOfInitial
        { get; set; }

        /// <summary>
        /// Возвращает или задает состояние при покупке.
        /// </summary>
        [Column("SSPK", TypeName = "varchar(7)")]
        public string ConditionOfPurchase
        { get; set; }


        /// <summary>
        /// Возвращает или задает соответстие по простоновлению №250 <see cref="Metr.Resolution250"/>.
        /// </summary>
        [ForeignKey("IDSPPP250")]
        public Resolution250 Resolution250 { get; set; }


        /// <summary>
        /// Возвращает или задает характеристики по доп. класс. 1 <see cref="Metr.AdditionalClassification1"/>.
        /// </summary>
        [ForeignKey("IDSPDPKL1")]
        public AdditionalClassification1 AdditionalClassification1 { get; set; }

        /// <summary>
        /// Возвращает или задает характеристики по доп. класс. 2 <see cref="Metr.AdditionalClassification2"/>.
        /// </summary>
        [ForeignKey("IDSPDPKL2")]
        public AdditionalClassification2 AdditionalClassification2 { get; set; }


        /// <summary>
        /// Возвращает или задает характеристики по доп. класс. 3 <see cref="Metr.AdditionalClassification3"/>.
        /// </summary>

        [ForeignKey("IDSPDPKL3")]
        public AdditionalClassification3 AdditionalClassification3 { get; set; }

        /// <summary>
        /// Возвращает или задает доп. поле.
        /// </summary>
        [Column("DPPLEKZ", TypeName = "varchar(50)")]
        public string AdditionalField { get; set; }

        /// <summary>
        /// Возвращает или задает дополнительные сведения.
        /// </summary>
        [Column("DSEKZ", TypeName = "varchar(8000)")]
        public string AdditionalInformation { get; set; }

        /// <summary>
        /// Возвращает или задает флаг внесен в Госреестр экземпляр или нет.
        /// </summary>
        [Column("GRVN", TypeName = "bit")]
        public bool? StateOfRegistry { get; set; }

        /// <summary>
        /// Возвращает или задает номер госреестра. 
        /// </summary>
        [Column("NNEKZGR", TypeName = "varchar(8)")]
        public string NumberOfRegister
        { get; set; }

        #endregion
    }
}