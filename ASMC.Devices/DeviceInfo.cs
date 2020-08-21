using System;
using AP.Reports.Utils;
using AP.Utils.Data;

namespace ASMC.Devices
{
    public interface IDevice : IDisposable
    {
        #region Property

        /// <summary>
        /// Строка подключения (адрес последовательного порта или шины GPIB и т.д.)
        /// </summary>
        string StringConnection { get; }

        /// <summary>
        /// Вернет тип устройства заданный в библиотеке.
        /// </summary>
        string UserType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Позволяет закрыть соединение с устройством.
        /// </summary>
        void Close();

        /// <summary>
        /// Позволяет открыть соединение с устройством.
        /// </summary>
        bool Open();

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

   
    /// <summary>
    /// Описывает информацию об устройстве.
    /// </summary>
    public class DeviceInfo
    {
        #region Property

        /// <summary>
        /// Предоставляет идентификационный номер ПО
        /// </summary>
        public string FirmwareVersion { get; }

        /// <summary>
        /// Предоставляет заводской номер.
        /// </summary>
        public string Manufacturer { get; }

        /// <summary>
        /// Предоставляет заводской номер устройства.
        /// </summary>
        public string SerialNumber { get; }

        /// <summary>
        /// Предастовляет тип
        /// </summary>
        public string Type { get; }

        #endregion

        public DeviceInfo(string firmwareVersion, string manufacturer, string serialNumber, string type)
        {
            FirmwareVersion = firmwareVersion;
            Manufacturer = manufacturer;
            SerialNumber = serialNumber;
            Type = type;
        }
    }
    /// <summary>
    /// Описывает комманду для отправки в устройство.
    /// </summary>
    public class Command : ICommand
    {
        public Command(string strCommand, string description, double value)
        {
            StrCommand = strCommand;
            Description = description;
            Value = value;
        }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public string StrCommand { get; }

        /// <inheritdoc />
        public double Value { get; }
    }
    public interface ICommand
    {
        #region Property

        /// <summary>
        /// Предоставляет описание комманды отправляемой в устройство.
        /// </summary>
        string StrCommand { get; }

        /// <summary>
        /// Предоставляет описание команды.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Предоставляет численное значение команды.
        /// </summary>
        double Value { get; }

        #endregion
    }
}