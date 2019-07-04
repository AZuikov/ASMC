using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность КСП
    /// </summary>
    public class Ksp
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>    

        [Key]
        [Browsable(false)]
        [Column("IDKSP", TypeName = "int")]
        public int? Id
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает наименование
        /// </summary>
        [Column("NMKSP", TypeName = "varchar(80)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает код
        /// </summary>
        [Column("KDKSP", TypeName = "int")]
        public int? Code
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает количество поверителей
        /// </summary>
        [Column("KLPV", TypeName = "int")]
        public int? 
            
            ExecutorNumber
        {
            get; set;
        }

}
}
