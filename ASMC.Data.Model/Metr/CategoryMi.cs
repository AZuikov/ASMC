using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность категории СИ.
    /// </summary>
    [Table("SPKT")]
    public class CategoryMi
    {
        #region Properties
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPKT", TypeName = "int")]
        public int? Id
        { get; set;
        }
        /// <summary>
        /// Возвращает или задает наименование категории СИ.
        /// </summary>
        [Required]
        [Column("NMKT", TypeName = "nvarchar(30)")]
        public string Name { get; set; }
        #endregion
    }
}
