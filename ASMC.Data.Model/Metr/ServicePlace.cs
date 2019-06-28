using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность места проведения МК.
    /// </summary>   
    [Table("SPMPOB")]
    public class ServicePlace
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]    
        [Browsable(false)]
        [Column("IDSPMPOB", TypeName = "int")]
        public int? Id { get; set; }
        /// <summary>
        /// Возвращает или задает место обслуживания.
        /// </summary>
        [Column("NMMPOB", TypeName = "varchar(50)")]
        public string Name { get; set; }
    }
}