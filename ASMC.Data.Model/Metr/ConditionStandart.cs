﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность штатного состояния
    /// </summary>
    [Table("SPSS")]
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
    }
}
