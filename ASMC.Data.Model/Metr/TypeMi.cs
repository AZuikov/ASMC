using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Тип СИ
    /// </summary>
    [Table("TIPS")] 
    public class TypeMi
    {
        #region Properties
        /// <summary>
        /// Id типа СИ
        /// </summary>
        [Key]
        [Column("IDTIPS", TypeName = "int")]
        public int TypeMiId { get; private set;  }
        /// <summary>
        ///  Id Код области измерения СИ
        /// </summary>
        [ForeignKey(nameof(CodeMa))]
        [Column("IDSPOI", TypeName = "int")]
        public int CodeMaId { get; set; }
        /// <summary>
        /// Код области измерения СИ
        /// </summary>
        public CodeMa CodeMa { get; set; }
        /// <summary>
        /// Id Наименование типа СИ
        /// </summary>
        [ForeignKey(nameof(NameType))]
        [Column("IDSPNMTP", TypeName = "int")]
        public int NameTypeId { get; set; }
        /// <summary>
        /// Наименование типа СИ
        /// </summary>
        public NameTypeMi NameType { get; set; }
        /// <summary>
        /// Id Категория СИ
        /// </summary>
        [ForeignKey(nameof(Сategory))]
        [Required]
        [Column("IDSPKT", TypeName = "int")]
        public int СategoryMiId { get; set; }
        /// <summary>
        /// Категория СИ
        /// </summary>
        public СategoryMi Сategory { get; set; }
        /// <summary>
        /// Id Конструктивное исполнение
        /// </summary>
        [ForeignKey(nameof(Design))]
        [Column("IDSPKI", TypeName = "int")]
        public int DesignId { get; set; }
        /// <summary>
        /// Конструктивное исполнение
        /// </summary>
        public Design Design { get; set; }
        /// <summary>
        /// Тип(обоначение) СИ
        /// </summary>
        [Required]
        [Column("TP", TypeName = "nvarchar(max)")]
        public string Type { get; set; }
        /// <summary>
        /// Срок службы
        /// </summary>
        [Column("SRSL", TypeName = "int")]
        public int LifeTime { get; set; }
        /// <summary>
        /// Межповерочный интервал
        /// </summary>
        [Column("PRMKGR", TypeName = "int")]
        public int IntertestingInterval { get; set; }
        /// <summary>
        /// Номер гасреестра типа
        /// </summary>
        [Column("NNTPGR", TypeName = "nvarchar(max)")]
        public string RegisterNumber { get; set; }
        /// <summary>
        /// Код ВНИИМС типа
        /// </summary>
        [Column("KDTPVNMS", TypeName = "nvarchar(max)")]
        public string VniimsCode { get; set; }
        /// <summary>
        /// Автоматизированная информационная система «Метрконтроль». Уникальный общероссийский номер (штрих-код)
        /// </summary>
        [Column("KDKMETR", TypeName = "nvarchar(max)")]
        public string AisMetrkontrol { get; set; }
        /// <summary>
        /// Дополнительные сведения
        /// </summary>
        [Column("DSTP", TypeName = "text")]
        public string AddInformation { get; set; }


        #endregion

    }
}
