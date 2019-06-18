using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник по Постановлению №250 “О перечне средств измерений, 
    /// поверка которых осуществляется только аккредитованными в установленном 
    /// порядке в области обеспечения единства измерений государственными региональными центрами метрологии”
    /// </summary>
    [Table("SPPP250")]
    public class Resolution250
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDSPPP250", TypeName = "int")]
        public int Resolution250Id { get; private set; }
        /// <summary>
        /// Позиция по Постановлению №250
        /// </summary>
        [Required]
        [Column("NMPP250", TypeName = "varchar(max)")]
        public string Position { get; set; }
        /// <summary>
        /// Код П № 250
        /// </summary>
        [Required]
        [Column("KDPP250", TypeName = "varchar(max)")]
        public  string Code { get; set; }

    }
}
