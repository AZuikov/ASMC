using System;
using System.Threading.Tasks;

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

    public interface IDeviceBase :IUserType, IDisposable
    {
      
        /// <summary>
        /// Позволяет закрыть соединение с устройством.
        /// </summary>
        void Close();

        /// <summary>
        /// Позволяет открыть соединение с устройством.
        /// </summary>
        void Open();
        /// <summary>
        /// Предоставлячет возможность проверки состояния соединения.
        /// </summary>
        bool IsOpen { get; }
        /// <summary>
        /// Предоставляет сведенье об успешности подключения.
        /// </summary>
        bool IsTestConnect { get; }
        /// <summary>
        /// Инициализация прибора.
        /// </summary>
        Task InitializeAsync();
    }

    public interface IUserType
    {
        /// <summary>
        /// Вернет тип устройства заданный в библиотеке.
        /// </summary>
        string UserType { get; }
    }
}