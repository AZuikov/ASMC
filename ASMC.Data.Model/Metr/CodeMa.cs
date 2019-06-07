using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Data.Entity.Core.Mapping;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Область измерений
    /// </summary>
    [Table("SPOI")]
    public class CodeMa 
    {
        public CodeMa()
        {
        }

        public CodeMa(int id)
        {
            CodeMaId = id;
        }

        #region Properties
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        [Column("IDSPOI", TypeName = "int")]
        public  int CodeMaId { get; }
        /// <summary>
        /// Код области измерений
        /// </summary>
        [Required]
        [Column("KDOI", TypeName = "nvarchar(max)")]
        public string Code { get; set; }
        /// <summary>
        /// Наименование области измерений
        /// </summary>
        [Required]
        [Column("NMOI", TypeName = "nvarchar(max)")]
        public  string Name { get; set; }

        #endregion

        #region Operators
        public static bool operator ==(CodeMa a, CodeMa b)
        {
            if (CodeMa.Equals(a, b)) return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }
            return (
                string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(a.Code, b.Code, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static bool operator !=(CodeMa a, CodeMa b)
        {
            if (CodeMa.Equals(a, b)) return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return true;
            }
            return !(
                string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(a.Code, b.Code, StringComparison.OrdinalIgnoreCase) == 0);
        }
        #endregion
    }
}
