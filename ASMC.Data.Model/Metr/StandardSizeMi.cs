using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{ 
    /// <summary>
    /// Карточка типоразмера СИ
    /// </summary>
    [Table("TPRZ")]
    public class StandardSizeMi
    {
        #region Properties
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDTPRZ", TypeName = "int")]
        public int StandardSizeMiId { get; private set;  }
        /// <summary>
        /// Id Тип СИ
        /// </summary>
        [ForeignKey(nameof(TypeMi))]
        [Column("IDTIPS", TypeName = "int")]
        public int TypeMiId { get; set; }
        /// <summary>
        /// Тип СИ
        /// </summary>
        public TypeMi TypeMi { get; set; }
        /// <summary>
        /// Id Комплексность МК
        /// </summary>
        [ForeignKey(nameof(CompletenessMi))]
        [Column("IDSPKMMK", TypeName = "int")]
        public int CompletenessMiId { get; set; }
        /// <summary>
        /// Комплектность СИ
        /// </summary>
        public  CompletenessMi CompletenessMi { get; set; }
        /// <summary>
        /// Диапазон
        /// </summary>
        [Required]
        [Column("DPZN", TypeName = "varchar(max)")]
        public string Range { get; set; }
        /// <summary>
        /// Характеристика точности
        /// </summary>
        [Required]
        [Column("HRTC", TypeName = "varchar(max)")]
        public string Accuracy { get; set; }
        /// <summary>
        /// Код ВНИИМС типа 
        /// </summary>
        [Column("KDTRVNMS", TypeName = "varchar(max)")]
        public string VniimsCode { get; set; }
        /// <summary>
        /// Номер госреестра типоразмера
        /// </summary>
        [Column("NNGSRS", TypeName = "varchar(max)")]
        public string RegisterNumber { get; set; }
        /// <summary>
        /// Служебный код
        /// </summary>
        [Column("KDSL", TypeName = "varchar(max)")]
        public string ServiceСode { get; set; }




       
       
       
        
      
        
        #endregion
    }
}
