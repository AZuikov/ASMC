using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Metr
{
    /// <summary>
    /// Места проведения МК
    /// </summary>   
    [Table("NMMPOB")]
    public class ServicePlace
    {
        /// <summary>
        ///  Индификатор место обслуживания 
        /// </summary>
        [Column("IDSPMPOB", TypeName = "int")]
        public int ServicePlaceId { get; private set; }   
        /// <summary>
        /// Место обслуживания
        /// </summary>
        [Column("NMMPOB", TypeName = "varchar(max)")]
        public string Name { get; set; }
    }
}