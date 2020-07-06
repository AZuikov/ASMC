using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ASMC.Data.Model.Interface;

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
        /// Возвращает перечень подключаемых устройств.
        /// </summary>
        IDevice[] Device { get; }

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

        #endregion
    }

    /// <summary>
    /// Содержет доступныйе виды Метрологического контроля.
    /// </summary>
    public abstract class AbstraktOperation
    {
        public delegate void ChangeShemaHandler(IUserItemOperationBase sender);

        /// <summary>
        /// Содержит перечесления типов операции.
        /// </summary>
        [Flags]
        public enum TypeOpeation
        {
            PrimaryVerf=1,
            PeriodicVerf=2,
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
        /// Позволяет задать или получить признак определяющий ускоренную работу.
        /// </summary>
        public bool IsSpeedWork { get; set; }

        /// <summary>
        /// Позволяет получить выбранную операцию.
        /// </summary>
        public IUserItemOperation SelectedOperation
        {
            get
            {
                switch (SelectedTypeOpeation)
                {
                    case TypeOpeation.PrimaryVerf:
                        return IsSpeedWork ? SpeedUserItemOperationPrimaryVerf : UserItemOperationPrimaryVerf;
                    case TypeOpeation.PeriodicVerf:
                        return IsSpeedWork ? SpeedUserItemOperationPeriodicVerf : UserItemOperationPeriodicVerf;
                    case TypeOpeation.Calibration:
                        return IsSpeedWork ? SpeedUserItemOperationCalibration : UserItemOperationCalibration;
                    case TypeOpeation.Adjustment:
                        return UserItemOperationAdjustment;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

        public void StartWork()
        {
            foreach (var opertion in SelectedOperation.UserItemOperation)
            {
                CurrentUserItemOperationBase = opertion;
                ChangeShemaEvent?.Invoke(opertion);
                opertion.StartWork();
            }
        }

        /// <summary>
        /// Запускает все операции асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task StartWorkAsync()
        {
            foreach (var opertion in SelectedOperation.UserItemOperation)
            {
                CurrentUserItemOperationBase = opertion;
                ChangeShemaEvent?.Invoke(opertion);
                await Task.Run(() => opertion.StartWork());
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
        void StartWork();

        #endregion
    }

    public abstract class AbstractUserItemOperationBase : TreeNode, IUserItemOperationBase
    {
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

        /// <inheritdoc />
        public abstract void StartWork();

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