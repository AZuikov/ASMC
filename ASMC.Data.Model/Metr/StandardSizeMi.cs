using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность типоразмера СИ.
    /// </summary>
    [Table("TPRZ")]
    public class StandardSizeMi
    {
        #region Properties
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDTPRZ", TypeName = "int")]
        public int? Id { get; set;  }

        /// <summary>
        /// Возвращает или задает тип СИ <see cref="Metr.TypeMi"/>.   
        /// </summary>
        [ForeignKey("IDTIPS")]
        public TypeMi TypeMi { get; set; }
        /// <summary>
        /// Возвращает или задает комплексность МК <see cref="Metr.CompletenessMi"/>.
        /// </summary>
        [ForeignKey("IDSPKMMK")]
        public  CompletenessMi CompletenessMi { get; set; }
        /// <summary>
        /// Возвращает или задает диапазон.
        /// </summary>
        [Required]
        [Column("DPZN", TypeName = "varchar(50)")]
        public string Range { get; set; }
        /// <summary>
        /// Возвращает или задает характеристику точности.
        /// </summary>
        [Required]
        [Column("HRTC", TypeName = "varchar(40)")]
        public string Accuracy { get; set; }
        /// <summary>
        /// Возвращает или задает код ВНИИМС типа. 
        /// </summary>
        [Column("KDTRVNMS", TypeName = "int")]
        public int? CodeTypeVniims
        { get; set; }
        /// <summary>
        /// Возвращает или задает номер госреестра типоразмера.
        /// </summary>
        [Column("NNGSRS", TypeName = "varchar(8)")]
        public string RegisterNumber { get; set; }
        /// <summary>
        /// Возвращает или задает служебный код.
        /// </summary>
        [Column("KDSL", TypeName = "varchar(10)")]
        public string ServiceСode { get; set; }
        #endregion
    }
}
