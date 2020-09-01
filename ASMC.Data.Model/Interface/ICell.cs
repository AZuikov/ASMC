using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    /// Предоставляет интерфейс ячейки
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// Наименование ячейки
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Знаечние ячейки
        /// </summary>
        object Value { get; set; }
        /// <summary>
        /// Описание ячейки
        /// </summary>
        string Description { get;  }
    }
}
