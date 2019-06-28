using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    ///  Сущность метрологического комплектного контроля СИ.
    /// </summary>
    [Table("SPKMMK")]
    public class CompletenessMi
    {
        /// <summary>
        ///  Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPKMMK", TypeName = "int")]
        public int? Id { get; set;}
        /// <summary>
        ///  Возвращает или задает наименование комплектности СИ.
        /// </summary>
        [Required]
        [Column("KMMK", TypeName = "nvarchar(50)")]
        public string Name { get; set; }
        /// <summary>
        ///  Возвращает или задает признак в АИС «Метрконтроль».
        /// </summary>
        [Column("PRTPRBMETR", TypeName = "nvarchar(3)")]
        public string Ais { get; set; }
    }
}
