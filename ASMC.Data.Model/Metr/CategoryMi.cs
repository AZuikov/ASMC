using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Utils.Data;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность категории СИ.
    /// </summary>
    [Table("SPKT")] 
    [StoredProcedure("dbo.up_gr_NmtbNmfiSelect", Operation = StoredProcedureOp.SelectMany)]
    public class CategoryMi
    {
        #region Properties

        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPKT", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает наименование категории СИ.
        /// </summary>
        [Required]
        [Column("NMKT", TypeName = "nvarchar(30)")]
        public string Name { get; set; }

        #endregion

        protected bool Equals(CategoryMi other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CategoryMi) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}