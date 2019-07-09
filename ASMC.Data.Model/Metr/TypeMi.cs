using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Utils.Data;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность вида СИ
    /// </summary>
    [Table("TIPS")]
    [StoredProcedure("dbo.up_gr_TipsSelect", KeyName = "vbr", KeyFormat = "tips.idtips={0}")]
    public class TypeMi
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDTIPS", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает код области измерения СИ <see cref="Metr.MeasuringType"/>.
        /// </summary>
        [ForeignKey("IDSPOI")]
        public MeasuringType MeasuringType { get; set; }

        /// <summary>
        /// Возвращает или задает наименование типа СИ <see cref="Metr.NameTypeMi"/>.
        /// </summary>      
        [ForeignKey("IDSPNMTP")]
        public NameTypeMi NameTypeMi { get; set; }

        /// <summary>
        /// Возвращает или задает категорию СИ <see cref="Metr.CategoryMi"/>.
        /// </summary>
        [Required]
        [ForeignKey("IDSPKT")] public CategoryMi CategoryMi { get; set; }

        /// <summary>
        /// Возвращает или задает конструктивное исполнение <see cref="Metr.Design"/>.
        /// </summary>
        [ForeignKey("IDSPKI")]
        public Design Design { get; set; }

        /// <summary>
        /// Возвращает или задает тип(обоначение).
        /// </summary>
        [Required]
        [Column("TP", TypeName = "nvarchar(35)")]
        public string Type { get; set; }

        /// <summary>
        /// Возвращает или задает срок службы.
        /// </summary>
        [Column("SRSL", TypeName = "int")]
        public int? LifeTime { get; set; }

        /// <summary>
        /// Возвращает или задает межповерочный интервал.
        /// </summary>
        [Column("PRMKGR", TypeName = "int")]
        public int? IntertestingInterval { get; set; }

        /// <summary>
        /// Возвращает или задает номер госреестра типа
        /// </summary>
        [Column("NNTPGR", TypeName = "nvarchar(8)")]
        public string NumberOfRegister { get; set; }

        /// <summary>
        /// Возвращает или задает код ВНИИМС типа
        /// </summary>
        [Column("KDTPVNMS", TypeName = "int")]
        public int? CodeTypeVniims { get; set; }

        /// <summary>
        /// Возвращает или задает уникальный общероссийский номер (штрих-код) АИС.
        /// </summary>
        [Column("KDKMETR", TypeName = "nvarchar(11)")]
        public string Ais { get; set; }

        /// <summary>
        /// Возвращает или задает дополнительные сведения.
        /// </summary>
        [Column("DSTP", TypeName = "text")]
        public string AdditionalInformation { get; set; }
    }
}