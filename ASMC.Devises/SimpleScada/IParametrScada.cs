using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Palsys.Data.Model.Metr;

namespace ASMC.Devises.SimpleScada
{
    public interface IMeasuredParametr : IParametr
    {
        MeasuredValue MeasuredValue
        {
            get; set;
        }
    }
    public interface IParametrScada : IMeasuredParametr
    {
        string DatebaseName
        {
            get; set;
        }
        string Procedure
        {
            get; set;
        }
        Tuple<string, object>[] Parameters
        {
            get; set;
        }
    }
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
        int Id
        {
            get;
        }

        /// <summary>
        /// Получение значение параметра
        /// </summary>
        /// <returns></returns>
        double Value
        {
            get; set;
        }

        /// <summary>
        /// Наименование измеряемого параметра
        /// </summary>
        /// <value>
        /// The name of the parametr.
        /// </value>
        string Name
        {
            get;
        }

        void FillValue();
    }
}
