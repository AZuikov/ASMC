using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// справочник "Персон"
    /// </summary>
    [Table("PRSN")]
    public class Person
    {
        /// <summary>
        /// Индификатор
        /// </summary>
        [Key]
        [Column("IDPRSN", TypeName = "int")]
        public int PersonsId { get; private set;}
        /// <summary>
        /// ФИО
        /// </summary>
        [Required]
        [Column("PRFIO", TypeName = "varchar(max)")]
        public string FullName { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        [Column("PRFM", TypeName = "varchar(max)")]
        public string Surname { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        [Column("PRNM", TypeName = "varchar(max)")]
        public string Name { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        [Column("PROT", TypeName = "varchar(max)")]
        public string MiddleName { get; set; }
        /// <summary>
        /// Телефон
        /// </summary>
        [Column("TEL", TypeName = "varchar(max)")]
        public string Phone { get; set; }
        /// <summary>
        /// E-mail
        /// </summary>
        [Column("EMAIL", TypeName = "varchar(max)")]
        public string Email{ get; set; }
        /// <summary>
        /// Дополнительный идентификатор персоны
        /// </summary>
        [Column("PRDPID", TypeName = "varchar(max)")]
        public string AdditionalPersonIdentifier{ get; set; }
        /// <summary>
        /// Дополнительные сведения о персоне
        /// </summary>
        [Column("DSPRSN", TypeName = "varchar(max)")]
        public string AdditionalInformationAboutPerson{ get; set; }
        /// <summary>
        /// Глобальный идентификатор персоны
        /// </summary>
        /// 
        [Column("GUIDPRSN", TypeName = "varchar(max)")]
        public string GUID { get; set; }
    }
}
