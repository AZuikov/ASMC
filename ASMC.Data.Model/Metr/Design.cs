using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    ///  Сущность конструктивного исполнения.
    /// </summary>
    [Table("SPKI")]
    public class Design
    {
        #region Properties
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPKI", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает наименование исполнения.
        /// </summary>
        [Required]
        [Column("NMKI", TypeName = "nvarchar(30)")]
        public string Name { get; set; }
        #endregion

    }
}
