using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность области применения. 
    /// </summary>
    [Table("SPOP")]
    public class AreaApplication
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPOP", TypeName = "int")]
        public int? Id { get; set; }

        protected bool Equals(AreaApplication other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AreaApplication) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Возвращает или задает область применения СИ.
        /// </summary>
        [Required]
        [Column("NMOP", TypeName = "varchar(50)")]
        public string Name { get; set; }
        
    }
}
