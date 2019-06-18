using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Спраочник "Конструктивное исполнение"
    /// </summary>
    [Table("SPKI")]
    public class Design
    {
        #region Properties
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDSPKI", TypeName = "int")]
        public int DesignId { get; private set; }
        /// <summary>
        /// Наименование исполнения
        /// </summary>
        [Required]
        [Column("NMKI", TypeName = "nvarchar(max)")]
        public string Name { get; set; }
        #endregion

    }
}
