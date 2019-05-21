using System.Collections.Generic;

namespace ASMC.Data.Model.Interface
{
    /// <summary>
    /// Описывает помещениея
    /// </summary>
    public interface IRoom
    {
        /// <summary>
        /// Наименование помещения
        /// </summary>
        /// <value>
        /// The name room.
        /// </value>
        string NameRoom { get; set; }
        /// <summary>
        /// Датчики в помещении
        /// </summary>
        /// <value>
        /// The sensors list.
        /// </value>
        List<ISensor> SensorsList { get; set; }
    }
    /// <summary>
    /// Описывает датчики
    /// </summary>
    public interface ISensor
    {
        /// <summary>
        /// Наименование датчика
        /// </summary>
        /// <value>
        /// The name of the sensor.
        /// </value>
        string SensorName { get; set; }
        /// <summary>
        /// Перечень параметров измеряемых датчиком
        /// </summary>
        /// <value>
        /// The parametrs.
        /// </value>
        List<IParametrs> Parametrs { get; set; }
    }
    /// <summary>
    /// Описывает измеряемые параметры
    /// </summary>
    public interface IParametrs
    {
        /// <summary>
        /// id параметра
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; set; }

        /// <summary>
        /// Получение значение параметра
        /// </summary>
        /// <returns></returns>
        double GetValue { get; }

        /// <summary>
        /// Наименование измеряемого параметра
        /// </summary>
        /// <value>
        /// The name of the parametr.
        /// </value>
        string ParametrName { get; set; }
        /// <summary>
        /// Количество символов после запятой
        /// </summary>
        /// <value>
        /// The sing.
        /// </value>
        int Sing { get; set; }
    }
}

