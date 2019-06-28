using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность вида цикла ремонта
    /// </summary>
    public class TypeCycleRm
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPVDRC", TypeName = "int")]
        public int? Id
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает наименование цикла ремонта
        /// </summary>
        [Column("NMVDRC", TypeName = "varchar(30)")]
        public string Name
        {
            get; set;
        }
    }
}
