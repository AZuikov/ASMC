using System;

namespace ASMC.Devices
{
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

        #endregion Property

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
        public string Description { get; protected set; }

        /// <inheritdoc />
        public string StrCommand { get; protected set; }

        /// <inheritdoc />
        public double Value { get; protected set; }

        public int CompareTo(ICommand other)
        {
            return Value.CompareTo(other.Value);
        }
    }

    public interface ICommand : IComparable<ICommand>
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

        #endregion Property
    }
}