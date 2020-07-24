using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASMC.Core;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using DevExpress.Mvvm;
using NLog;

namespace ASMC.Data.Model
{
    public interface IUserItemOperation<T> : IUserItemOperationBase
    {
        #region Property

        List<IBasicOperation<T>> DataRow { get; set; }

        #endregion
    }

    /// <summary>
    /// Интерфейст описывающий подключаемые настройки утроств, необходимыех для выполнения операций.
    /// </summary>
    public interface IDevice
    {
        #region Property
        /// <summary>
        /// Позволяет получить или задать признак возможности выбора строки подключения.
        /// </summary>
        bool IsCanStringConnect  {   get; set; }
        /// <summary>
        /// Позволяет получить описание устройства.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Позволяет получить статус подключения устройства.
        /// </summary>
        bool? IsConnect { get; }

        /// <summary>
        /// позволяет поучать или задавать перечень взаимозаменяемых устройств.
        /// </summary>
        string[] Name { get; set; }

        /// <summary>
        /// Позволяет задать или получить имя выбранного прибора.
        /// </summary>
        string SelectedName { get; set; }

        /// <summary>
        /// Позволяет задать или получить строку подключения к прибору.
        /// </summary>
        string StringConnect { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Позволяет вызвать окно
        /// </summary>
        void Setting();

        #endregion
    }

    /// <summary>
    /// Предоставлет интерфес операций метрологического контроля.
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
        string[] AddresDivece { get; set; }

        /// <summary>
        /// Возвращает перечень устройст используемых для МК подключаемых устройств.
        /// </summary>
        IDevice[] ControlDevices { get; set; }

        /// <summary>
        /// Возвращает перечень устройст подвергаемых МК.
        /// </summary>
        IDevice[] TestDevices { get; set; }

        /// <summary>
        /// Возвращает перечень операций
        /// </summary>
        IUserItemOperationBase[] UserItemOperation { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Выполняет обнавление списка устройств.
        /// </summary>
        void RefreshDevice();

        void FindDivice();

        #endregion
    }

    /// <summary>
    /// Содержет доступныйе виды Метрологического контроля.
    /// </summary>
    public class OperationBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public IMessageBoxService TaskMessageService { get; set; }
        public delegate void ChangeShemaHandler(IUserItemOperationBase sender);

        /// <summary>
        /// Содержит перечесления типов операции.
        /// </summary>
        [Flags]
        public enum TypeOpeation
        {
            PeriodicVerf = 1,
            PrimaryVerf =2,
            Calibration=4,
            Adjustment=8
        }
        /// <summary>
        /// Событие изменение схемы.
        /// </summary>
        public event ChangeShemaHandler ChangeShemaEvent;

        #region Property
        /// <summary>
        /// Предоставляет доступные операции.
        /// </summary>
        public TypeOpeation? EnabledOperation
        {
            get
            {
                TypeOpeation? res = null;
                if (UserItemOperationPrimaryVerf != null || SpeedUserItemOperationPrimaryVerf != null)
                    res = TypeOpeation.PrimaryVerf;
                if (UserItemOperationPeriodicVerf != null || SpeedUserItemOperationPeriodicVerf != null)
                    if (res != null)
                        res = res| TypeOpeation.PeriodicVerf;
                    else
                        res = TypeOpeation.PeriodicVerf;
                if (UserItemOperationCalibration != null || SpeedUserItemOperationCalibration != null)
                    if (res != null)
                        res |= TypeOpeation.Calibration;
                    else
                        res = TypeOpeation.Calibration;
                if (UserItemOperationAdjustment != null)
                    if (res != null)
                        res |= TypeOpeation.Adjustment;
                    else
                        res = TypeOpeation.Adjustment;
                return res;
            }
        }

        /// <summary>
        /// Позволяет задать или получить признак определяющий ускоренную работу(ПРОВЕРКА).
        /// </summary>
        public bool IsSpeedWork { get; set; }

        /// <summary>
        /// Позволяет получить выбранную операцию.
        /// </summary>
        public IUserItemOperation SelectedOperation
        {
            get
            {
                IUserItemOperation res = null;
                if(SelectedTypeOpeation.HasFlag(TypeOpeation.PrimaryVerf))
                {
                    res = IsSpeedWork ? SpeedUserItemOperationPrimaryVerf : UserItemOperationPrimaryVerf;
                }
                else if(SelectedTypeOpeation.HasFlag(TypeOpeation.PeriodicVerf))
                {
                    res = IsSpeedWork ? SpeedUserItemOperationPeriodicVerf : UserItemOperationPeriodicVerf;
                }
                else if(SelectedTypeOpeation.HasFlag(TypeOpeation.Calibration))
                {
                    res = IsSpeedWork ? SpeedUserItemOperationCalibration : UserItemOperationCalibration;
                }
                else if(SelectedTypeOpeation.HasFlag(TypeOpeation.Adjustment))
                {
                    res = SelectedTypeOpeation.HasFlag(TypeOpeation.Adjustment) ? UserItemOperationAdjustment : null;
                }

                if(res?.UserItemOperation != null)
                {
                    foreach(var t in res.UserItemOperation)
                    {
                        t.MessageBoxService = TaskMessageService;
                    }
                }  
                return res;
            }
        }

        /// <summary>
        /// Позволяет задать или получить тип операции.
        /// </summary>
        public TypeOpeation SelectedTypeOpeation { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции ускоренной калибровки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationCalibration { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции ускоренной переодической поверки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationPeriodicVerf { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции ускоренной первичной поверки.
        /// </summary>
        protected IUserItemOperation SpeedUserItemOperationPrimaryVerf { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции регрулировки
        /// </summary>
        protected IUserItemOperation UserItemOperationAdjustment { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции калибровки.
        /// </summary>
        protected IUserItemOperation UserItemOperationCalibration { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции переодической поверки.
        /// </summary>
        protected IUserItemOperation UserItemOperationPeriodicVerf { get; set; }

        /// <summary>
        /// Позволяет  задать или получить операции первичной поверки.
        /// </summary>
        protected IUserItemOperation UserItemOperationPrimaryVerf { get; set; }

        private IUserItemOperationBase CurrentUserItemOperationBase { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Запускает все операции асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task StartWorkAsync(CancellationTokenSource source)
        {    
            foreach(var opertion in SelectedOperation.UserItemOperation)
            {
                CurrentUserItemOperationBase = opertion;
                try
                {
                    await opertion.StartWork(source.Token);
                }
                catch (Exception e)
                {
                    source.Cancel();
                    source.Token.ThrowIfCancellationRequested();
                    Logger.Error(e);
                }
            }
        }

        #endregion
    }


    /// <summary>
    /// Предоставляет интерфес пункта операции
    /// </summary>
    public interface IUserItemOperationBase
    {
        #region Property
        IMessageBoxService MessageBoxService { get; set; }
        /// <summary>
        /// Предоставляет данные для отображения операций.
        /// </summary>
        DataTable Data { get; }
        /// <summary>
        /// Предоставляет гуид операции.
        /// </summary>
        Guid Guid { get; }
        /// <summary>
        /// Предоставляет результат операции
        /// </summary>
        bool? IsGood { get; set; }
        /// <summary>
        /// Предоставляет наименование операции
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Предоставляет инструкцию для подключения.
        /// </summary>
        ShemeImage Sheme { get; }

        #endregion

        #region Methods

        void StartSinglWork(Guid guid);
       Task StartWork(CancellationToken token);

        #endregion

    }

    public abstract class AbstractUserItemOperationBase : TreeNode, IUserItemOperationBase
    {
        /// <summary>
        /// Связывает строку подключения из интрефеса пользователя с выбранным прибором. Работает для контрольных и контролируемых приборов.
        /// </summary>
        /// <param name="currentDevice">Прибор из списка контрольных (эталонов) или контролируемых (поверяемых/проверяемых) приборов.</param>
        /// <returns></returns>
        protected string GetStringConnect(Devices.IDevice currentDevice)
        {
            var connect = this.UserItemOperation.ControlDevices
                                 .FirstOrDefault(q => string.Equals(q.SelectedName, currentDevice.DeviceType, StringComparison.InvariantCultureIgnoreCase))?.StringConnect ??
                             this.UserItemOperation.TestDevices
                                 .FirstOrDefault(q => string.Equals(q.SelectedName, currentDevice.DeviceType, StringComparison.InvariantCultureIgnoreCase))?.StringConnect;
           
            if (string.IsNullOrEmpty(connect))
                throw new ArgumentException($@"Строка подключения не указана для {currentDevice.DeviceType}");

            return connect;
        }



        protected IUserItemOperation UserItemOperation { get; }

        protected AbstractUserItemOperationBase(IUserItemOperation userItemOperation)
        {
            UserItemOperation = userItemOperation;
        }
        #region Property

        /// <summary>
        /// Позволяет задать и получить признак ускоренного выполнения операций.
        /// </summary>
        public bool IsSpeedWork { get; set; }

        #endregion

        #region Methods

        protected abstract DataTable FillData();

        #endregion

        /// <summary>
        /// Позволяет получить гуид операции.
        /// </summary>
        public Guid Guid { get; } = Guid.NewGuid();

        /// <summary>
        /// Запускает выполнение операций с указаном Гуидом.
        /// </summary>
        /// <param name = "guid"></param>
        public abstract void StartSinglWork(Guid guid);

       

        /// <inheritdoc />
        public ShemeImage Sheme { get; set; }

        /// <inheritdoc />
        public bool? IsGood { get; set; }
            public Func<CancellationToken, Task>BodyWork
        {
            get; set;
        }
        /// <inheritdoc />
        public abstract Task StartWork(CancellationToken token);

      

        public IMessageBoxService MessageBoxService { get; set; }

        /// <inheritdoc />
        public DataTable Data => FillData();
    }

    public class ShemeImage
    {
        #region Property

        public int Number { get; set; }
        public string Path { get; set; }

        #endregion
    }
}