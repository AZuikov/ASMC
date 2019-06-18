using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "категория СИ"
    /// </summary>
    [Table("SPKT")]
    public class СategoryMi
    {
        #region Properties
        /// <summary>
        /// Id Си
        /// </summary>
        [Key]
        [Column("IDSPKT", TypeName = "int")]
        public int IdСategoryMi { get; private set;
        }
        /// <summary>
        /// Наименование категории СИ
        /// </summary>
        [Required]
        [Column("NMKT", TypeName = "nvarchar(max)")]
        public string Name { get; set; }
        #endregion
    }
}
