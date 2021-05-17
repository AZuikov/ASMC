using System;
using System.Threading.Tasks;

namespace ASMC.Data.Model
{
    public interface IProtocolStringLine : IDeviceRemote
    {
        /// <summary>
        /// Строка подключения (адрес последовательного порта или шины GPIB и т.д.)
        /// </summary>
        string StringConnection { get; set; }

  
    }

    public interface IDeviceRemote :IUserType, IDisposable
    {
        /// <summary>
        /// Предоставляет сведенье об успешности подключения.
        /// </summary>
        bool IsTestConnect { get; }
        /// <summary>
        /// Инициализация прибора.
        /// </summary>
        void Initialize();
    }

    public interface IUserType
    {
        /// <summary>
        /// Вернет тип устройства заданный в библиотеке.
        /// </summary>
        string UserType { get; }
    }
}