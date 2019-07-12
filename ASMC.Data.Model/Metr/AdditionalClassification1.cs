using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность дополнительной классификации 1.
    /// </summary>  
    [Table("SPDPKL1")]
    public class AdditionalClassification1
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPDPKL1", TypeName = "int")]
        public int? Id { get; set; }

        protected bool Equals(AdditionalClassification1 other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AdditionalClassification1) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Возвращает или задает код классификации.
        /// </summary>
        [Required]
        [Column("KDDPKL1", TypeName = "nvarchar(30)")]
        public string Code { get; set; }
        /// <summary>
        ///  Возвращает или задает наименование классификации.
        /// </summary>
        [Column("NMDPKL1", TypeName = "nvarchar(7)")]
        public string Name { get; set; }
    }
}
