using System.Collections.Generic;

namespace ASMC.Data.Model.Interface
{
    
    

    /// <summary>
    /// Описывает измеряемые параметры
    /// </summary>
    public interface IParametr
    {
        /// <summary>
        /// Параметр для ВМ
        /// </summary>
        bool IsChecked
        {
            get; set;
        }
        /// <summary>
        /// id параметра
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; }

        /// <summary>
        /// Получение значение параметра
        /// </summary>
        /// <returns></returns>
        double Value { get; set; }

        /// <summary>
        /// Наименование измеряемого параметра
        /// </summary>
        /// <value>
        /// The name of the parametr.
        /// </value>
        string Name { get;  }

        void FillValue();
    }
}

