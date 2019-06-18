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
    /// Дополнительная классификация 1
    /// </summary>
    public class CharacteristicsAdditionalClass1
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDSPDPKL1", TypeName = "int")]
        public int CharacteristicsAdditionalClass1Id { get; private set;
        }
        /// <summary>
        /// Код
        /// </summary>
        [Required]
        [Column("KDDPKL1", TypeName = "nvarchar(max)")]
        public string Code { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        [Required]
        [Column("NMDPKL1", TypeName = "nvarchar(max)")]
        public string Name { get; set; }
    }
}
