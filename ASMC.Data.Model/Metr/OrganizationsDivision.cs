using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Организации и подразделения
    /// </summary>
    [Table("FRPD")]
    public class OrganizationsDivision
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDFRPD", TypeName = "int")]
        public int OrganizationsUnitsId { get; private set; }
        [Column("IDFRPDR", TypeName = "int")]
        public int NONAME { get; set; }
        /// <summary>
        ///Наименование организации/подразделения
        /// </summary>
        [Required]
        [Column("NMFRPD", TypeName = "varchar(max)")]
        public string Name{ get; set; }
        /// <summary>
        /// Локальный код организации/подразделения
        /// </summary>
        [Column("KDFRPDLC", TypeName = "varchar(max)")]
        public string Code { get; set; }
        /// <summary>
        /// Глобальный индификатор
        /// </summary>
        [Column("FRPDGUID", TypeName = "varchar(max)")]
        public string Guid { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        [Column("DTSZFRPD", TypeName = "datetime")]
        public DateTime DateСreation { get; set; }
        /// <summary>
        /// Дата ликвидации
        /// </summary>
        [Column("DTLKFRPD", TypeName = "datetime")]
        public DateTime DataLiquidation { get; set; }

    }
}
