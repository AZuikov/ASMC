using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Palsys.Data.Model.Metr;
using Palsys.Utils.Data;

namespace ASMC.Data.Model.Asumc
{
    public class InfluencingCondition
    {
        [Key]
        [Browsable(false)]
        [Column("InfluencingConditionID", TypeName = "int")]
        public int? Id;
        /// <summary>
        /// Возвращает или задает номер госреестра.
        /// </summary>
        [Column("NNEKZGR", TypeName = "varchar(8)")]
        public string NumberOfRegister
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает цикл МК.
        /// </summary>
        [ForeignKey("IDTPRMK")]
        public CyclePeriodeMc CyclePeriodeMc
        {
            get; set;
        }
        [ForeignKey("IDEKZIP")]
        public MeasuredParametersExpansion MeasuredParameters { get; set; }
    }
    [StoredProcedure("delele_MeasureParametr", Operation = StoredProcedureOp.Delete)]
    [StoredProcedure("insert_MeasureParametr", Operation = StoredProcedureOp.Insert)]
    [StoredProcedure("select_MeasureParametr", Operation = StoredProcedureOp.SelectMany)]
    public class MeasuredParametersExpansion
    {
        [Key]
        [Column("Id", TypeName = "int")]
        public int? Id
        {
            get; set;
        }

        [ForeignKey("IDSPIV")]
        public MeasuredValue MeasuredValue
        {
            get; set;
        }
        [ForeignKey("IDTPRZ")]
        public StandardSizeMi StandardSizeMi
        {
            get; set;
        }
        [ForeignKey("IDSPVDMK")]
        public TypeMc TypeMc
        {
            get; set;
        }
        [Column("MinValue", TypeName = "varchar(40)")]
        public string MinValue
        {
            get; set;
        }
        [Column("MaxValue", TypeName = "varchar(50)")]
        public string MaxValue
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
