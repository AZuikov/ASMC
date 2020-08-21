﻿using ASMC.Data.Model;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Предоставляет интерфес к циклу операций метрологического контроля.
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
        /// Возвращает перечень устройст используемых для МК подключаемых устройств.
        /// </summary>
        IDeviceUi[] ControlDevices { get; set; }

        /// <summary>
        /// Имя документа который будет формироватся без формата файла.
        /// </summary>
        string DocumentName { get; }

        ServicePack ServicePack { get; }

        /// <summary>
        /// Возвращает перечень устройст подвергаемых МК.
        /// </summary>
        IDeviceUi[] TestDevices { get; set; }

        /// <summary>
        /// Возвращает перечень операций
        /// </summary>
        IUserItemOperationBase[] UserItemOperation { get; }

        #endregion

        #region Methods

        void FindDivice();

        /// <summary>
        /// Выполняет обнавление списка устройств.
        /// </summary>
        void RefreshDevice();

        #endregion
    }
}