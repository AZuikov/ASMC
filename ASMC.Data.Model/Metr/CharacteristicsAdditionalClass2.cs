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
    /// Дополнительная классификация 2
    /// </summary>
    public class CharacteristicsAdditionalClass2
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDSPDPKL2", TypeName = "int")]
        public int CharacteristicsAdditionalClass2Id { get; private set;
        }
        /// <summary>
        /// Код
        /// </summary>
        [Required]
        [Column("KDDPKL2", TypeName = "nvarchar(max)")]
        public string Code { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        [Required]
        [Column("NMDPKL2", TypeName = "nvarchar(max)")]
        public string Name { get; set; }
    }
}
