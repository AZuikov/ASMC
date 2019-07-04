using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Сущность персоны.
    /// </summary>
    [Table("PRSN")]
    //[Procedure("dbo.up_gr_PRSNSelect_dk", Operation =  StoredProcedureOp.SelectMany)]
    //[Procedure("dbo.up_gr_PRSNSelect_dk", KeyName = "@fltr", KeyFormat = "prsn.idprsn={0}")]
    public class Person
    {
        /// <summary>
        /// Возвращает или задает ключ сущности.
        /// </summary>
        [Key]
        [Browsable(false)]
        [Column("IDPRSN", TypeName = "int")]
        public int? Id { get; set;}
        /// <summary>
        /// Возвращает или задает ФИО.
        /// </summary>
        [Required]
        [Column("PRFIO", TypeName = "varchar(35)")]
        public string FullName { get; set; }
        /// <summary>
        /// Возвращает или задает фамилию.
        /// </summary>
        [Column("PRFM", TypeName = "varchar(30)")]
        public string Surname { get; set; }
        /// <summary>
        /// Возвращает или задает имя.
        /// </summary>
        [Column("PRNM", TypeName = "varchar(25)")]
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает отчество.
        /// </summary>
        [Column("PROT", TypeName = "varchar(25)")]
        public string MiddleName { get; set; }
        /// <summary>
        /// Возвращает или задает телефон.
        /// </summary>
        [Column("TEL", TypeName = "varchar(50)")]
        public string Phone { get; set; }
        /// <summary>
        /// Возвращает или задает e-mail.
        /// </summary>
        [Column("EMAIL", TypeName = "varchar(50)")]
        public string Email{ get; set;
        }
        /// <summary>
        /// Возвращает или задает дополнительные сведения о персоне.
        /// </summary>
        [Column("DSPRSN", TypeName = "varchar(2000)")]
        public string AdditionalInformationAbout
        {
            get; set;
        }
        /// <summary>
        /// Возвращает или задает глобальный идентификатор персоны.
        /// </summary>
        /// 
        [Column("GUIDPRSN", TypeName = "varchar(50)")]
        public string Guid { get; set; }
        /// <summary>
        /// Возвращает или задает дополнительный идентификатор персоны.
        /// </summary>
        [Column("PRDPID", TypeName = "varchar(50)")]
        public string AdditionalIdentifier
        {
            get; set;
        }
    }
}
