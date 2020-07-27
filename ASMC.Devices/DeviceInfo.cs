using System;
using AP.Reports.Utils;

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

        #endregion
    }

    /// <summary>
    /// Содержит доступные множители и их обозначения.
    /// </summary>
    public enum Multipliers
    {
        /// <summary>
        /// Множетель нано 1Е-9.
        /// </summary>
        [StringValue("н")] [DoubleValue(1E-9)] Nano,

        /// <summary>
        /// Множетель микро 1Е-6.
        /// </summary>
        [StringValue("мк")] [DoubleValue(1E-6)] Micro,

        /// <summary>
        /// Множетель мили 1Е-3.
        /// </summary>
        [StringValue("м")] [DoubleValue(1E-3)] Mili,

        /// <summary>
        /// Без множителя.
        /// </summary>
        [StringValue("")] [DoubleValue(1)] None,

        /// <summary>
        /// Множитель кило 1Е3
        /// </summary>
        [StringValue("к")] [DoubleValue(1E3)] Kilo,

        /// <summary>
        /// Мноэитель мега 1Е6
        /// </summary>
        [StringValue("М")] [DoubleValue(1E6)] Mega,

        /// <summary>
        /// Мноэитель мега 1Е6
        /// </summary>
        [StringValue("Г")] [DoubleValue(1E9)] Giga
    }

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
        /// Предоставляет описание диапазона.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Предоставляет численное значение диапазона.
        /// </summary>
        double Value { get; }

        #endregion
    }
}