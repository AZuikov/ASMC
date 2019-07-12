using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AP.Utils.Data;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность штатного состояния
    /// </summary>
    [Table("SPSS")]
    [StoredProcedure("dbo.up_gr_NmtbNmfiSelect", Operation = StoredProcedureOp.SelectMany)]
    public class ConditionStandart
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDSPSS", TypeName = "int")]
        public int? Id { get; set;}
        /// <summary>
        /// Возвращает или задает наименование.
        /// </summary>
        [Required]
        [Column("NMSS", TypeName = "varchar(30)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает флаг на операцию с записью.
        /// </summary>
        [Required]
        [Browsable(false)]
        [Column("FLOZ", TypeName = "int")]
        public int? FlagWrite { get; set; }
        /// <summary>
        /// Возвращает или задает код для стандартизации состояний.
        /// </summary>
        [Column("KDSS", TypeName = "int")]
        public int? Code { get; set; }

        protected bool Equals(ConditionStandart other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConditionStandart) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
