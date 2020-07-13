using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using Palsys.Data.Model.Metr;

namespace ASMC.Data.Model
{
    public class Program
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDPROG", TypeName = "int")]
        public int? Id
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает типоразмер <see cref="Metr.StandardSizeMi"/>.
        /// </summary>         
        [Required]
        [ForeignKey("IDTPRZ")]
        public StandardSizeMi StandardSizeMi
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает номер госреестра. 
        /// </summary>
        [Column("NNEKZGR", TypeName = "varchar(8)")]
        public string NumberOfRegister
        {
            get; set;
        }
    }
}