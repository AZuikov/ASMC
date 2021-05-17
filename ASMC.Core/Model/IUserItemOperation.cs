using ASMC.Data.Model;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Предоставляет интерфейс к циклу операций метрологического контроля.
    /// Например: поверка, калибровка.
    /// </summary>
    public interface IUserItemOperation
    {
        #region Property
        /// <summary>
        /// Возвращает перечень необходимого оборудования.
        /// </summary>
        string[] Accessories { get; }

        /// <summary>
        /// Возвращает все доступные подключения
        /// </summary>
        string[] AddresDevice { get; set; }

        /// <summary>
        /// Возвращает перечень устройств используемых для операций поверки (или иного контроля).
        /// </summary>
        IDeviceUi[] ControlDevices { get; set; }

        /// <summary>
        /// Имя документа который будет формироваться без формата файла.
        /// </summary>
        string DocumentName { get; }

        ServicePack ServicePack { get; }

        /// <summary>
        /// Возвращает перечень устройств подвергаемых операций поверки (или иного контроля).
        /// </summary>
        IDeviceUi[] TestDevices { get; set; }

        /// <summary>
        /// Возвращает перечень операций
        /// </summary>
        IUserItemOperationBase[] UserItemOperation { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Выполняет поиск устройств.
        /// </summary>
        void FindDevice();

        /// <summary>
        /// Выполняет обновление списка устройств.
        /// </summary>
        void RefreshDevice();

        #endregion
    }
}