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
    /// Дополнительная классификация 3
    /// </summary>
    public class CharacteristicsAdditionalClass3
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDSPDPKL3", TypeName = "int")]
        public int CharacteristicsAdditionalClass3Id { get; private set;
        }
        /// <summary>
        /// Код
        /// </summary>
        [Required]
        [Column("KDDPKL3", TypeName = "nvarchar(max)")]
        public string Code { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        [Required]
        [Column("NMDPKL3", TypeName = "nvarchar(max)")]
        public string Name { get; set; }
    }
}
