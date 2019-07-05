using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность организации/подразделения
    /// </summary>
    [Table("FRPD")]
    public class Organization
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDFRPD", TypeName = "int")]
        public int? Id { get; set; }

        /// <summary>
        /// Возвращает или задает (НЕ ЯСНО, ЧТО ЭТО).
        /// </summary>
        [Column("IDFRPDR", TypeName = "int")]
        public int? _Filed { get; set; }
        /// <summary>
        /// Возвращает или задает наименование организации/подразделения.
        /// </summary>
        [Required]
        [Column("NMFRPD", TypeName = "varchar(80)")]
        public string Name{ get; set; }
        /// <summary>
        /// Возвращает или задает локальный код организации/подразделения.
        /// </summary>
        [Column("KDFRPDLC", TypeName = "varchar(20)")]
        public string Code { get; set; }
        /// <summary>
        /// Возвращает или задает глобальный индификатор
        /// </summary>
        [Column("FRPDGUID", TypeName = "varchar(50)")]
        public string Guid { get; set; }
        /// <summary>
        /// Возвращает или задает дату создания
        /// </summary>
        [Column("DTSZFRPD", TypeName = "datetime")]
        public DateTime? DateOfСreation { get; set; }
        /// <summary>
        /// Возвращает или задает дату ликвидации
        /// </summary>
        [Column("DTLKFRPD", TypeName = "datetime")]
        public DateTime? DataOfLiquidation { get; set; }

    }
}
