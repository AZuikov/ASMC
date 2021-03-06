﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ASMC.Data.Model;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Bars;
using Newtonsoft.Json;
using NLog;

namespace ASMC.Core.Model
{
    /// <summary>
    /// Содержит доступные виды Метрологического контроля.
    /// </summary>
    public class OperationMetrControlBase
    {
        /// <summary>
        /// Содержит перечисления типов операции.
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
        /// Признак, ускоренной операции.
        /// </summary>
        public bool IsSpeed { get; private set; } = false;

        /// <summary>
        /// Предоставляет доступные операции.
        /// </summary>
        public TypeOpeation? EnabledOperation
        {
            get
            {
                TypeOpeation? res = null;
                if (UserItemOperationPrimaryVerf != null || SpeedUserItemOperationPrimaryVerf != null)
                {
                    res = TypeOpeation.PrimaryVerf;
                    if (SpeedUserItemOperationPrimaryVerf != null) IsSpeed = true;
                }
                if (UserItemOperationPeriodicVerf != null || SpeedUserItemOperationPeriodicVerf != null)
                {  if (res != null)
                        res = res | TypeOpeation.PeriodicVerf;
                    else
                        res = TypeOpeation.PeriodicVerf;
                    if (SpeedUserItemOperationPeriodicVerf != null) IsSpeed = true;
                }

                if (UserItemOperationCalibration != null || SpeedUserItemOperationCalibration != null)
                {
                    if (res != null)
                        res |= TypeOpeation.Calibration;
                    else
                        res = TypeOpeation.Calibration;
                    if (SpeedUserItemOperationCalibration != null) IsSpeed = true;
                }
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
        protected SchemeImage LastShem { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Запускает все операции асинхронно
        /// </summary>
        /// <returns></returns>
        public async Task StartWorkAsync(CancellationTokenSource source)
        {
            var count = SelectedOperation.UserItemOperation.Sum(CountNode);

            foreach (var userItemOperationBase in SelectedOperation.UserItemOperation)
                 await ClrNodeAsync(userItemOperationBase);

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
                cou += tree.Nodes.Sum(node => CountNode((IUserItemOperationBase) node));
                return cou;
            }

            async Task ClrNodeAsync(IUserItemOperationBase userItemOperationBase)
            {
                userItemOperationBase.EndOperationEvent += UserItemOperationBase_EndOperationEvent;
                try
                {
                    if (userItemOperationBase.IsCheked || !IsManual)
                    {
                        ShowShemAsync(userItemOperationBase.Sheme, source);
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
                foreach (var node in tree.Nodes) 
                   await  ClrNodeAsync((IUserItemOperationBase) node);
            }
        }

        private void UserItemOperationBase_EndOperationEvent(object sender)
        {
            //var ser = new JsonSerializer();
            //var a = JsonConvert.SerializeObject(SelectedOperation.UserItemOperation., Formatting.Indented, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All,  });


            //using (var sw = new StreamWriter(@"D:\Новый текстовый документ.txt"))
            //{
            //    sw.Write(a);
            //} 
          
        }

        private async void ShowShemAsync(SchemeImage sheme, CancellationTokenSource source)
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
            } while (!await sheme.CheckShemAsync());
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