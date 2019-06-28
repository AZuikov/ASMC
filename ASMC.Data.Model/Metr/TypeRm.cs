using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность вида ремонта.
    /// </summary>
    public class TypeRm
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPVDR", TypeName = "int")]
        public int Id { get; set; }

        /// <summary>
        /// Возвращает или задает вид ремонта.
        /// </summary>
        [Column("NMVDR", TypeName = "varchar(50)")]
        public string Name { get; set; }

        /// <summary>
        /// Возвращает или задает обозначение вида ремонта.
        /// </summary>
        [Column("OBVDR", TypeName = "varchar(1)")]
        public string Symbol { get; set; }
    }
}