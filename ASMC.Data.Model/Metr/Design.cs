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

        protected bool Equals(Design other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Design) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Возвращает или задает наименование исполнения.
        /// </summary>
        [Required]
        [Column("NMKI", TypeName = "nvarchar(30)")]
        public string Name { get; set; }
        #endregion

    }
}
