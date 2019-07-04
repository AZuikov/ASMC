using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность по Постановлению №250 “О перечне средств измерений, 
    /// поверка которых осуществляется только аккредитованными в установленном 
    /// порядке в области обеспечения единства измерений государственными региональными центрами метрологии”.
    /// </summary>
    [Table("SPPP250")]
    public class Resolution250
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPPP250", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает позицию по Постановлению №250.
        /// </summary>
        [Required]
        [Column("NMPP250", TypeName = "varchar(450)")]
        public string Position { get; set; }
        /// <summary>
        /// Возвращает или задает код П. № 250.
        /// </summary>
        [Required]
        [Column("KDPP250", TypeName = "varchar(3)")]
        public string Code { get; set; }

    }
}
