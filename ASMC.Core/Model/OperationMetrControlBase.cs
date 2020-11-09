using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ASMC.Data.Model;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Bars;
using NLog;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Содержит доступныйе виды Метрологического контроля.
    /// </summary>
    public class OperationMetrControlBase
    {
        /// <summary>
        /// Содержит перечесления типов операции.
        /// </summary>
        [Flags]
        public enum TypeOpeation
        {
            PeriodicVerf = 1,
            PrimaryVerf = 2,
            Calibration = 4,
            Adjustment = 8
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                        res = res | TypeOpeation.PeriodicVerf;
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
                Logger.Debug($@"Активны операции {res}");
                return res;
            }
        }

        /// <summary>
        /// Признак автоматического прохода всех операций.
        /// </summary>
        public bool IsManual { get; set; }

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
                if (SelectedTypeOpeation.HasFlag(TypeOpeation.PrimaryVerf))
                    res = IsSpeedWork ? SpeedUserItemOperationPrimaryVerf : UserItemOperationPrimaryVerf;
                else if (SelectedTypeOpeation.HasFlag(TypeOpeation.PeriodicVerf))
                    res = IsSpeedWork ? SpeedUserItemOperationPeriodicVerf : UserItemOperationPeriodicVerf;
                else if (SelectedTypeOpeation.HasFlag(TypeOpeation.Calibration))
                    res = IsSpeedWork ? SpeedUserItemOperationCalibration : UserItemOperationCalibration;
                else if (SelectedTypeOpeation.HasFlag(TypeOpeation.Adjustment))
                    res = SelectedTypeOpeation.HasFlag(TypeOpeation.Adjustment) ? UserItemOperationAdjustment : null;
                return res;
            }
        }

        /// <summary>
        /// Позволяет задать или получить тип операции.
        /// </summary>
        public TypeOpeation SelectedTypeOpeation { get; set; }

        /// <summary>
        /// Позволяет получить или задать последунюю отображенную схему.
        /// </summary>
        protected ShemeImage LastShem { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Запускает все операции асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task StartWorkAsync(CancellationTokenSource source)
        {
            var count = 0;
            //LastShem = null;
            foreach (var userItemOperationBase in SelectedOperation.UserItemOperation)
                count+=CountNode(userItemOperationBase);

            foreach (var userItemOperationBase in SelectedOperation.UserItemOperation)
                await ClrNode(userItemOperationBase);

            int CountNode(IUserItemOperationBase userItemOperationBase)
            {
                var cou = 0;
                try
                {
                    if (userItemOperationBase.IsCheked || !IsManual)
                    {
                        cou= userItemOperationBase.Count;
                    }
                }
                catch (Exception e)
                {
                    source.Cancel();
                    Logger.Error(e);
                    throw;
                }

                var tree = (ITreeNode)userItemOperationBase;
                foreach (var node in tree.Nodes)
                    cou+=CountNode((IUserItemOperationBase)node);
                return cou;
            }

            async Task ClrNode(IUserItemOperationBase userItemOperationBase)
            {
                try
                {
                    if (userItemOperationBase.IsCheked || !IsManual)
                    {
                        ShowShem(userItemOperationBase.Sheme, source);
                        await userItemOperationBase.StartWork(source);
                    }
                }
                catch (Exception e)
                {
                    source.Cancel();
                    Logger.Error(e);
                    throw;
                }

                var tree = (ITreeNode) userItemOperationBase;
                foreach (var node in tree.Nodes) await ClrNode((IUserItemOperationBase) node);
            }
        }

        private async void ShowShem(ShemeImage sheme, CancellationTokenSource source)
        {
            if (sheme == null || LastShem?.Number == sheme.Number) return;
            LastShem = sheme;
            var ser = SelectedOperation.ServicePack.ShemForm();
            
            if (ser == null)
            {
                Logger.Error("Сервис не найден");
                throw new NullReferenceException("Сервис не найден");
            }

            ser.Entity = sheme;
            do
            {
                if (!(ser.Show() is true))
                {
                    source.Cancel(true);
                }
                Logger.Debug($@"Была показана схема №{sheme.Number}");
            } while (!await sheme.ChekShem());
        }

        #endregion

        #region Operations

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

        #endregion Operations
    }
}