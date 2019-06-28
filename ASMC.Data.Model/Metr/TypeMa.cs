using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность вида ТО
    /// </summary>
    [Table("SPVDTO")]
   public class TypeMa
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPVDTO", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает вид ТО
        /// </summary>
        [Column("NMVDTO", TypeName = "varchar(50)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает период ТО, мес.
        /// </summary>
        [Column("PRTO", TypeName = "int")]
        public int? Periode { get; set; }
    }
}
