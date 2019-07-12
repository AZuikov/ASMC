using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Справочник "сферы государственного регулирования обеспечения единства измерений"
    /// </summary>
    [Table("SPSHMK")]
    public class AreasMetrologicalControl
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPSHMK", TypeName = "int")]
        public  int? Id { get; set;}

        protected bool Equals(AreasMetrologicalControl other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AreasMetrologicalControl) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Cфера ГРОЕИ
        /// </summary>
        [Required]
        [Column("NMSHMK", TypeName = "varchar(50)")]
        public string Name { get; set; }
        /// <summary>
        ///  Код сферы ГРОЕИ
        /// </summary>
        [Column("KDSHMK", TypeName = "varchar(3)")]
        public string Code { get; set; }

    }
}
