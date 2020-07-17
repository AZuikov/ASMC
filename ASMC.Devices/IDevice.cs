using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASMC.Devices
{
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// подключиться к устройству
        /// </summary>
        /// <returns></returns>
        bool Open();
        /// <summary>
        /// подключиться к устройству
        /// </summary>
        void Close();
        /// <summary>
        /// Вернет тип устройства
        /// </summary>
        string DeviceType { get; }
        /// <summary>
        /// Строка подключения (адрес последовательного порта или шины GPIB и т.д.)
        /// </summary>
        string StringConnection { get; }


    }
}
