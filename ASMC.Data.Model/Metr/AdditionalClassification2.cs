using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность дополнительной классификации 2.
    /// </summary> 
    [Table("SPDPKL2")]
    public class AdditionalClassification2
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPDPKL2", TypeName = "int")]
        public int? Id
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает код классификации.
        /// </summary>
        [Required]
        [Column("KDDPKL2", TypeName = "nvarchar(30)")]
        public string Code
        {
            get; set;
        }

        protected bool Equals(AdditionalClassification2 other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AdditionalClassification2) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        ///  Возвращает или задает наименование классификации.
        /// </summary>
        [Column("NMDPKL2", TypeName = "nvarchar(7)")]
        public string Name
        {
            get; set;
        }
    }
}
