using System;

namespace ASMC.Data.Model
{
    public interface IProtocolStringLine : IDeviceBase
    {
        #region Property

        /// <summary>
        /// Строка подключения (адрес последовательного порта или шины GPIB и т.д.)
        /// </summary>
        string StringConnection { get; }

       

        #endregion

        #region Methods

        /// <summary>
        /// Считывает строку. 
        /// </summary>
        /// <returns></returns>
        string ReadLine();

        /// <summary>
        /// Отправляет полученную команду, без изменений. 
        /// </summary>
        void WriteLine(string data );

        /// <summary>
        /// Отправляет данные и тут же считывает ответ.
        /// </summary>
        /// <param name="inStrData">Строка для отправки</param>
        /// <returns>Полученный ответ.</returns>
        string QueryLine(string inStrData);


        #endregion
    }

    public interface IDeviceBase : IDisposable
    {
        /// <summary>
        /// Вернет тип устройства заданный в библиотеке.
        /// </summary>
        string UserType { get; }
        /// <summary>
        /// Позволяет закрыть соединение с устройством.
        /// </summary>
        void Close();

        /// <summary>
        /// Позволяет открыть соединение с устройством.
        /// </summary>
        bool Open();
    }
}