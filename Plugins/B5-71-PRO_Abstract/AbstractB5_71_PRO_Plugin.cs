﻿using AP.Math;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;

namespace B5_71_PRO_Abstract
{
    /// <summary>
    /// В этом пространстве имен будет реализован общий алгоритм поверки блоков питания без жесткой привязки к модели
    /// устройства
    /// </summary>
    public abstract class AbstractB571ProPlugin<T> : Program<T> where T : OperationMetrControlBase
    {
        #region Property

        public OperationMetrControlBase AbstraktOperation { get; protected set; }

        #endregion Property

        protected AbstractB571ProPlugin(ServicePack service) : base(service)
        {
            Grsi = "42467-09";
            Accuracy = "Напряжение ПГ ±(0,002 * U + 0.1); ток ПГ ±(0,01 * I + 0,05)";
        }
    }

    public class Operation : OperationMetrControlBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.

        public Operation(ServicePack servicePack)
        {
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class OpertionFirsVerf : ASMC.Core.Model.Operation
    {
        protected OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            DocumentName = "Б5-71_1";
            //Необходимые аксесуары
            Accessories = new[]
            {
                "Нагрузка электронная Keysight N3300A с модулем N3303A",
                "Мультиметр цифровой Agilent/Keysight 34401A",
                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Преобразователь интерфесов USB - RS-232 + нуль-модемный кабель",
                "Кабель banana - banana 6 шт.",
                "Кабель BNC - banan для В3-57"
            };
        }

        #region Methods

        /// <inheritdoc />
        public override void FindDevice()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        #endregion Methods
    }

    /// <summary>
    /// Внешний осмотр СИ
    /// </summary>
    public abstract class Oper0VisualTest : ParagraphBase<bool>
    {
        protected Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Результат внешнего осмотра" };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "ITBmVisualTest";
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = base.FillData();

            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperation<bool>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Getting ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWorkAsync = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText();
                service.Title = "Внешний осмотр";
                service.Entity = new Tuple<string, Assembly>("B5VisualTestText", null);
                service.Show();
                var res = service.Entity as Tuple<string, bool>;
                operation.Getting = res.Item2;
                operation.Comment = res.Item1;
                operation.IsGood = () => operation.Getting;

                return Task.CompletedTask;
            };

            operation.CompliteWorkAsync = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        #endregion Methods
    }

    /// <summary>
    /// Проведение опробования
    /// </summary>
    public abstract class Oper1Oprobovanie : ParagraphBase<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly decimal[] MyPoint = { 0.1M, 0.5M, 1M };

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion Property

        protected Oper1Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Результат опробования" };
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "ITBmOprobovanie";
        }

        protected override DataTable FillData()
        {
            var data = base.FillData();
            var dataRow = data.NewRow();
            if (DataRow.Count == 1)
            {
                var dds = DataRow[0] as BasicOperationVerefication<bool>;
                if (dds == null)
                    dataRow[0] = "";
                //ReSharper disable once PossibleNullReferenceException
                else if (dds.IsGood == null)
                    dataRow[0] = "не выполнено";
                else
                    dataRow[0] = dds.IsGood() ? "Соответствует" : dds.Comment;
                data.Rows.Add(dataRow);
            }

            return data;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperationVerefication<bool>();
            operation.InitWorkAsync = async () =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        Mult.StringConnection = GetStringConnect(Mult);
                        Load.StringConnection = GetStringConnect(Load);
                        Bp.StringConnection = GetStringConnect(Bp);
                        Load.WriteLine("*rst");
                        Load.FindThisModule();

                        //если модуль нагрузки найти не удалось
                        if (Load.ChanelNumber <= 0)
                            throw new
                                ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    });
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
            };
            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);
                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                        var resist = Bp.VoltMax / Bp.CurrMax + 3;
                        Load.ResistanceLoad.SetResistanceRange(resist).ResistanceLoad.Set(resist);
                        Load.SetOutputState(MainN3300.State.On);

                        Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax).OnOutput();
                        foreach (var pointMult in MyPoint)
                        {
                            var setPoint = pointMult * Bp.VoltMax;

                            //ставим точку напряжения
                            Bp.SetStateVolt(setPoint);
                            Thread.Sleep(500);

                            //измеряем напряжение

                            var measVolt = Math.Abs(Load.VoltageLoad.MeasureVolt);

                            operation.IsGood = () => { return Bp.VoltMax / measVolt >= 0.7M; };

                            if (!operation.IsGood())
                            {
                                Logger.Error($"Операция опробования не прошла по напряжению в точке {setPoint} В, измерено {operation.Getting} В");
                                return;
                            }
                        }

                        resist = Bp.VoltMax / Bp.CurrMax - 3;
                        Load.ResistanceLoad.SetResistanceRange(resist).ResistanceLoad.Set(resist);
                        Bp.SetStateVolt(Bp.VoltMax);
                        foreach (var pointMult in MyPoint)
                        {
                            var setPoint = pointMult * Bp.CurrMax;
                            //ставим точку напряжения
                            Bp.SetStateCurr(setPoint);
                            Thread.Sleep(500);
                            //измеряем напряжение

                            var measCurr = Math.Abs(Load.CurrentLoad.MeasureCurrent);
                            operation.IsGood = () => { return Bp.CurrMax / measCurr >= 0.7M; };

                            if (!operation.IsGood())
                            {
                                Logger.Error($"Операция опробования не прошла по току в точке {setPoint} А, измерено {operation.Getting} А");
                                return;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        Load.SetOutputState(MainN3300.State.Off);
                        Bp.OffOutput();
                    }
                }, cancellationToken);
               
            };
            operation.CompliteWorkAsync = () =>
            {
                Load.SetOutputState(MainN3300.State.On);
                Bp.OnOutput();

                //Теперь проверим внешнюю индикацию режимов
                var answer =
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show("Сейчас на лицевой панели блока питания индикатор \"СТАБ.ТОКА\" горит?",
                                           "Опробование", MessageButton.YesNo, MessageIcon.Question, MessageResult.Yes);

                if (answer == MessageResult.No)
                {
                    operation.IsGood = () => { return false; };
                    Logger.Error("режим CC: Не горит индикация стабилизации тока на источнике питания.");
                    return Task.FromResult(true);
                }

                var resist = Bp.VoltMax / Bp.CurrMax + 3;
                Load.ResistanceLoad.SetResistanceRange(resist).ResistanceLoad.Set(resist);

                answer =
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show("Сейчас на лицевой панели прибора индикатор \"СТАБ.ТОКА\" НЕ горит?",
                                           "Опробование", MessageButton.YesNo, MessageIcon.Question, MessageResult.Yes);

                if (answer == MessageResult.No)
                {
                    operation.IsGood = () => { return false; };
                    Logger.Error("режим CV: На источнике питания индикатор стабилизации тока должен не должен гореть.");
                    return Task.FromResult(true);
                }

                Load.SetOutputState(MainN3300.State.Off);
                Bp.OffOutput();

                operation.IsGood = () => true;
                return Task.FromResult(true);
            };
            DataRow.Add(operation);
        }

        #endregion Methods
    }

    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public abstract class Oper2DcvOutput : BaseOparationWithMultimeter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        private static readonly decimal[] MyPoint = { 0.1M, 0.5M, 1M };

        #region Property



        #endregion Property

        protected Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного напряжения";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Установленное значение напряжения, В",
                "Измеренное значение, В",
                "Минимальное допустимое значение, В",
                "Максимальное допустимое значение, В"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcvOutput";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null) continue;
                dataRow[0] = dds?.Expected + " В";
                dataRow[1] = dds?.Getting + " В";
                dataRow[2] = dds?.LowerTolerance + " В";
                dataRow[3] = dds?.UpperTolerance + " В";
                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            foreach (var point in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWorkAsync = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            Mult.StringConnection = GetStringConnect(Mult);

                            Load.StringConnection = GetStringConnect(Load);
                            Bp.StringConnection = GetStringConnect(Bp);

                            Load.FindThisModule();

                            //если модуль нагрузки найти не удалось
                            if (Load.ChanelNumber <= 0)
                                throw new
                                    ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                        });

                        while (!Mult.IsFrontTerminal)
                            UserItemOperation.ServicePack.MessageBox().Show("На панели прибора " + Mult.UserType +
                                                                          " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                                                          "Указание оператору", MessageButton.OK,
                                                                          MessageIcon.Information,
                                                                          MessageResult.OK);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);
                            Bp.InitDevice();

                            var setPoint = point * Bp.VoltMax;

                            //ставим точку напряжения
                            Bp.SetStateCurr(Bp.CurrMax).SetStateVolt(setPoint);
                            Bp.OnOutput();
                            Thread.Sleep(1300);

                            //измеряем напряжение

                            Mult.DcVoltage.RangeStorage.SetRange(new MeasPoint<Voltage>(setPoint));
                            Mult.DcVoltage.RangeStorage.IsAutoRange = true;
                            Mult.DcVoltage.Setting();
                            var result = Mult.DcVoltage.GetValue().MainPhysicalQuantity.Value;
                            MathStatistics.Round(ref result, 3);

                            //забиваем результаты конкретного измерения для последующей передачи их в протокол

                            operation.Expected = setPoint;
                            operation.Getting = result;

                            SetLowAndUppToleranceAndIsGood_Volt(operation);

                            Bp.OffOutput();
                            Load.SetOutputState(MainN3300.State.Off);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Load.SetOutputState(MainN3300.State.Off);
                            Bp.OffOutput();
                        }
                    }, cancellationToken);
                  
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                          $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                          "Повторить измерение этой точки?",
                                                                          "Информация по текущему измерению",
                                                                          MessageButton.YesNo, MessageIcon.Question,
                                                                          MessageResult.Yes);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }



        #endregion Methods
    }

    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public abstract class Oper3DcvMeasure : BaseOparationWithMultimeter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { 0.1M, 0.5m, 1 };

        #region Property

        //порт нужно спрашивать у интерфейса


        #endregion Property

        protected Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного напряжения";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Измеренное эталонным мультиметром значение, В",
            "Измеренное источником питания значение, В",
            "Минимальное допустимое значение, В",
            "Максимальное допустимое значение, В"
        }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcvMeasure";
        }

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds?.Expected + " В";
                dataRow[1] = dds?.Getting + " В";
                dataRow[2] = dds?.LowerTolerance + " В";
                dataRow[3] = dds?.UpperTolerance + " В";
                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <param name="token"></param>
        /// <param name="token1"></param>
        /// <inheritdoc />
        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            foreach (var point in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWorkAsync = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            Mult.StringConnection = GetStringConnect(Mult);
                            Load.StringConnection = GetStringConnect(Load);
                            Bp.StringConnection = GetStringConnect(Bp);

                            //узнаем на каком канале стоит модуль нагрузки
                            Load.FindThisModule();

                            //если модуль нагрузки найти не удалось
                            if (Load.ChanelNumber <= 0)
                                throw new
                                    ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                        });

                        while (!Mult.IsFrontTerminal)
                            UserItemOperation.ServicePack.MessageBox().Show("На панели прибора " + Mult.UserType +
                                                                          " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                                                          "Указание оператору", MessageButton.OK,
                                                                          MessageIcon.Information,
                                                                          MessageResult.OK);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };
                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);

                            Bp.InitDevice();

                            var setPoint = point * Bp.VoltMax;
                            //ставим точку напряжения
                            Bp.SetStateCurr(Bp.CurrMax).SetStateVolt(setPoint);
                            Bp.OnOutput();
                            Thread.Sleep(1300);

                            //измеряем напряжение
                            Mult.DcVoltage.RangeStorage.SetRange(new MeasPoint<Voltage>(setPoint));
                            Mult.DcVoltage.RangeStorage.IsAutoRange = true;
                            Mult.DcVoltage.Setting();
                            var resultMult = Mult.DcVoltage.GetValue().MainPhysicalQuantity.Value;
                            var resultBp = Bp.GetMeasureVolt();

                            MathStatistics.Round(ref resultMult, 3);
                            MathStatistics.Round(ref resultBp, 3);

                            //забиваем результаты конкретного измерения для последующей передачи их в протокол

                            operation.Expected = (decimal)resultMult;
                            operation.Getting = resultBp;

                            SetLowAndUppToleranceAndIsGood_Volt(operation);



                            Bp.OffOutput();
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Load.SetOutputState(MainN3300.State.Off);
                            Bp.OffOutput();
                        }
                    }, cancellationToken);
                   
                };

                operation.CompliteWorkAsync = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                          $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                          "Повторить измерение этой точки?",
                                                                          "Информация по текущему измерению",
                                                                          MessageButton.YesNo, MessageIcon.Question,
                                                                          MessageResult.Yes);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }



        #endregion Methods
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class Oper4VoltUnstable : BaseOparationWithMultimeter
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrСoefVoltUnstable = { 0.1M, 0.5m, 0.9m };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property





        #endregion Property

        protected Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного напряжения";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Рассчитанное значение нестабильности (U_МАКС - U_МИН)/2, В",
                "Допустимое значение, В"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcvUnstable";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();
            if (DataRow.Count == 1)
            {
                var dataRow = dataTable.NewRow();
                var dds = DataRow[0] as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                if (dds == null)
                {
                    dataRow[0] = "";
                    dataRow[1] = "";
                }
                else
                {
                    dataRow[0] = dds?.Getting + " В";
                    try
                    {
                        dataRow[1] = dds?.UpperTolerance + " В";
                    }
                    catch (NullReferenceException e)
                    {
                        dataRow[1] = "";
                    }
                }

                if (dds.IsGood == null)
                    dataRow[2] = "не выполнено";
                else
                    dataRow[2] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperationVerefication<decimal>();
            operation.InitWorkAsync = async () =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        Mult.StringConnection = GetStringConnect(Mult);
                        Load.StringConnection = GetStringConnect(Load);
                        Bp.StringConnection = GetStringConnect(Bp);

                        Load.FindThisModule();

                        //если модуль нагрузки найти не удалось
                        if (Load.ChanelNumber <= 0)
                            throw new
                                ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    });

                    while (!Mult.IsFrontTerminal)
                        UserItemOperation.ServicePack.MessageBox().Show("На панели прибора " + Mult.UserType +
                                                                      " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                                                      "Указание оператору", MessageButton.OK,
                                                                      MessageIcon.Information,
                                                                      MessageResult.OK);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);
                        Bp.InitDevice();
                        Bp.SetStateVolt(Bp.VoltMax).SetStateCurr(Bp.CurrMax);

                        // ------ настроим нагрузку
                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);

                        var pointResistance = Bp.VoltMax / (Bp.CurrMax * ArrСoefVoltUnstable[2]);
                        MathStatistics.Round(ref pointResistance, 3);

                        Load.ResistanceLoad.SetResistanceRange(pointResistance).ResistanceLoad.Set(pointResistance)
                            .SetOutputState(MainN3300.State.On);

                        //сюда запишем результаты
                        var voltUnstableList = new List<decimal>();

                        Bp.OnOutput();

                        foreach (var coef in ArrСoefVoltUnstable)
                        {
                            var resistance = Bp.VoltMax / (coef * Bp.CurrMax);
                            Load.ResistanceLoad.SetResistanceRange(resistance).ResistanceLoad
                                .Set(resistance); //ставим сопротивление

                            // время выдержки
                            Thread.Sleep(1000);
                            // записываем результаты

                            voltUnstableList.Add(Mult.DcVoltage.GetValue().MainPhysicalQuantity.Value);
                        }

                        Bp.OffOutput();

                        //считаем
                        var resultVoltUnstable = (voltUnstableList.Max() - voltUnstableList.Min()) / 2;
                        MathStatistics.Round(ref resultVoltUnstable, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = 0;
                        operation.Getting = resultVoltUnstable;

                        SetLowAndUppToleranceAndIsGood_VoltUnstable(operation);

                        Load.SetOutputState(MainN3300.State.Off);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        Load.SetOutputState(MainN3300.State.Off);
                        Bp.OffOutput();
                    }
                }, cancellationToken);
            
            };
            operation.CompliteWorkAsync = () =>
            {
                if (operation.IsGood()) return Task.FromResult(operation.IsGood());
                var answer =
                    UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                    $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                    "Повторить измерение этой точки?",
                        "Информация по текущему измерению",
                        MessageButton.YesNo, MessageIcon.Question,
                        MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);

                return Task.FromResult(operation.IsGood());
            };
            DataRow.Add(DataRow.IndexOf(operation) == -1
                            ? operation
                            : (BasicOperationVerefication<decimal>)operation.Clone());
        }



        #endregion Methods
    }

    /// <summary>
    /// Определение уровня пульсаций
    /// </summary>
    public abstract class Oper5VoltPulsation : BaseOperationPowerSupplyAndElectronicLoad
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrResistanceVoltUnstable = { (decimal)20.27M, (decimal)37.5M, (decimal)187.5M };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций по напряжению";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Измеренное значение пульсаций, мВ",
                "Допустимое значение пульсаций, мВ"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcvPulsation";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            if (DataRow.Count == 1)
            {
                var dataRow = dataTable.NewRow();
                var dds = DataRow[0] as BasicOperationVerefication<decimal>;
                if (dds == null)
                {
                    dataRow[0] = "";
                    dataRow[1] = "";
                }
                else
                {
                    dataRow[0] = dds?.Getting + " мВ";
                    try
                    {
                        dataRow[1] = dds?.UpperTolerance + "мВ";
                    }
                    catch (NullReferenceException e)
                    {
                        dataRow[1] = "";
                    }
                }

                // ReSharper disable once PossibleNullReferenceException

                if (dds.IsGood == null)
                    dataRow[2] = "не выполнено";
                else
                    dataRow[2] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperationVerefication<decimal>();

            operation.InitWorkAsync = async () =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        Mult.StringConnection = GetStringConnect(Mult);
                        Load.StringConnection = GetStringConnect(Load);
                        Bp.StringConnection = GetStringConnect(Bp);

                        Load.FindThisModule();

                        //если модуль нагрузки найти не удалось
                        if (Load.ChanelNumber <= 0)
                            throw new
                                ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                        var point = Bp.VoltMax / ((decimal)0.9M * Bp.CurrMax);
                        Load.SetWorkingChanel()
                            .SetModeWork(MainN3300.ModeWorks.Resistance)
                            .ResistanceLoad.SetResistanceRange(point)
                            .ResistanceLoad.Set(point)
                            .SetOutputState(MainN3300.State.On);

                        Bp.InitDevice();
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.SetStateCurr(Bp.CurrMax);
                        Bp.OnOutput();
                    });

                    Mult.Open();
                    while (Mult.IsTerminal)
                        UserItemOperation.ServicePack.MessageBox().Show("На панели прибора " + Mult.UserType +
                                                                      " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                                                      "Указание оператору", MessageButton.OK,
                                                                      MessageIcon.Information,
                                                                      MessageResult.OK);

                    Thread.Sleep(5000);
                    UserItemOperation.ServicePack.MessageBox()
                                     .Show("Установите на В3-57 подходящий предел измерения напряжения",
                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                           MessageResult.OK);

                    var windows = UserItemOperation.ServicePack.FreeWindow();
                    var vm = new SelectRangeViewModel();
                    windows.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                    windows.Title = "Выбор предела измерения В3-57";
                    windows.ViewModel = vm;
                    windows.DocumentType = "SelectRangeView";
                    windows.Show();

                    var a = vm.SelectRange;

                    Mult.Dc.Voltage.Range.Set(100);
                    var voltPulsV357 = (decimal)Mult.GetMeasValue();
                    voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
                    voltPulsV357 = MathStatistics.Mapping(voltPulsV357, 0, (decimal)0.99M, 0,
                                                          (decimal)a.MainPhysicalQuantity.Value);
                    MathStatistics.Round(ref voltPulsV357, 0);

                    UserItemOperation.ServicePack.MessageBox().Show(
                                                                  "Установите на В3-57 МАКСИМАЛЬНЫЙ предел измерения напряжения",
                                                                  "Указание оператору", MessageButton.OK,
                                                                  MessageIcon.Information,
                                                                  MessageResult.OK);

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = voltPulsV357;
                    operation.ErrorCalculation = (expected, getting) => expected - getting;

                    SetLowAndUppToleranceAndIsGood_VoltPuls(operation);


                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
                finally
                {
                    Load.SetOutputState(MainN3300.State.Off);
                    Bp.OffOutput();
                }
            };


            operation.CompliteWorkAsync = () =>
            {
                if (operation.IsGood == null) return Task.FromResult(false);
                if (!operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                      $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                      "Повторить измерение этой точки?",
                                                                      "Информация по текущему измерению",
                                                                      MessageButton.YesNo, MessageIcon.Question,
                                                                      MessageResult.Yes);

                    if (answer == MessageResult.No) return Task.FromResult(true);
                }

                return Task.FromResult(operation.IsGood());
            };
            DataRow.Add(operation);
        }



        #endregion Methods

        #region Fileds


        protected Mult_34401A Mult { get; set; }


        #endregion Fileds
    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public abstract class Oper6DciOutput : BaseOperationPowerSupplyAndElectronicLoad
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1M, (decimal)0.5M, 1 };

        #region Property



        #endregion Property

        protected Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного тока";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Установленное значение тока, А",
                "Измеренное значение, А",
                "Минимальное допустимое значение, А",
                "Максимальное допустимое значение, А"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcIOutput";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds?.Expected + " А";
                dataRow[1] = dds?.Getting + " А";
                dataRow[2] = dds?.LowerTolerance + " А";
                dataRow[3] = dds?.UpperTolerance + " А";
                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            foreach (var coef in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();

                operation.InitWorkAsync = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            Load.StringConnection = GetStringConnect(Load);
                            Bp.StringConnection = GetStringConnect(Bp);

                            Load.FindThisModule();
                            //если модуль нагрузки найти не удалось
                            if (Load.ChanelNumber <= 0)
                                throw new
                                    ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                            var resist = Bp.VoltMax / Bp.CurrMax - 3;
                            Load.ResistanceLoad.SetResistanceRange(resist).ResistanceLoad.Set(resist);
                            Load.SetOutputState(MainN3300.State.On);

                            Bp.InitDevice();
                            Bp.SetStateVolt(Bp.VoltMax);
                            Bp.OnOutput();

                            var setPoint = coef * Bp.CurrMax;
                            //ставим значение тока
                            //плавно подходим, что бы не было перегрузки.
                            Bp.SetStateCurr(setPoint * (decimal)0.9M);
                            Bp.SetStateCurr(setPoint);
                            Bp.OffOutput();
                            Bp.OnOutput();
                            Thread.Sleep(1000);
                            //измеряем ток

                            var result = Load.CurrentLoad.MeasureCurrent;

                            MathStatistics.Round(ref result, 3);

                            operation.Expected = setPoint;
                            operation.Getting = result;

                            SetLowAndUppToleranceAndIsGood_Curr(operation);

                            Bp.OffOutput();
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Load.SetOutputState(MainN3300.State.Off);
                            Bp.OffOutput();
                        }
                    }, cancellationToken);
                   
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                          $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                          "Повторить измерение этой точки?",
                                                                          "Информация по текущему измерению",
                                                                          MessageButton.YesNo, MessageIcon.Question,
                                                                          MessageResult.Yes);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }



        #endregion Methods
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public abstract class Oper7DciMeasure : BaseOperationPowerSupplyAndElectronicLoad
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1M, (decimal)0.5M, 1 };

        #region Fields



        #endregion Fields

        #region Property



        #endregion Property

        protected Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного тока";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[] { "Измеренное эталонным авмперметром значение тока, А",
                "Измеренное блоком питания значение тока, А",
                "Минимальное допустимое значение, А",
                "Максимальное допустимое значение, А"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcIMeasure";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                if (dds == null) continue;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds?.Expected + " А";
                dataRow[1] = dds?.Getting + " А";
                dataRow[2] = dds?.LowerTolerance + " А";
                dataRow[3] = dds?.UpperTolerance + " А";
                if (dds.IsGood == null)
                    dataRow[4] = "не выполнено";
                else
                    dataRow[4] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            foreach (var coef in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWorkAsync = async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            Bp.StringConnection = GetStringConnect(Bp);
                            Load.StringConnection = GetStringConnect(Load);

                            Load.FindThisModule();

                            //если модуль нагрузки найти не удалось
                            if (Load.ChanelNumber <= 0)
                                throw new
                                    ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                };

                operation.BodyWorkAsync = (cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                            var resist = Bp.VoltMax / Bp.CurrMax - 3;
                            Load.ResistanceLoad.SetResistanceRange(resist).ResistanceLoad.Set(resist);
                            Load.SetOutputState(MainN3300.State.On);

                            Bp.InitDevice();
                            Bp.SetStateVolt(Bp.VoltMax).SetStateCurr(Bp.CurrMax * (decimal)0.7M);
                            Bp.OnOutput();

                            var setPoint = coef * Bp.CurrMax;
                            //ставим точку напряжения
                            Bp.SetStateCurr(setPoint);
                            Thread.Sleep(1000);
                            //измеряем ток
                            var resultN3300 = Load.CurrentLoad.MeasureCurrent;
                            MathStatistics.Round(ref resultN3300, 3);

                            var resultBpCurr = Bp.GetMeasureCurr();
                            MathStatistics.Round(ref resultBpCurr, 3);

                            operation.Expected = resultN3300;
                            operation.Getting = resultBpCurr;

                            SetLowAndUppToleranceAndIsGood_Curr(operation);

                            Bp.OffOutput();
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            throw;
                        }
                        finally
                        {
                            Load.SetOutputState(MainN3300.State.Off);
                            Bp.OffOutput();
                        }
                    }, cancellationToken);
                   
                };
                operation.CompliteWorkAsync = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                          $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                          "Повторить измерение этой точки?",
                                                                          "Информация по текущему измерению",
                                                                          MessageButton.YesNo, MessageIcon.Question,
                                                                          MessageResult.Yes);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }



        #endregion Methods
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public class Oper8DciUnstable : BaseOperationPowerSupplyAndElectronicLoad
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1M, (decimal)0.5M, (decimal)0.9M };

        #region Fields



        #endregion Fields

        protected Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного тока";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Рассчитанное значение нестабильности (I_МАКС - I_МИН)/2, А",
                "Допустимое значение, А"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcIUnstable";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            if (DataRow.Count == 1)
            {
                var dataRow = dataTable.NewRow();
                var dds = DataRow[0] as BasicOperationVerefication<decimal>;
                if (dds == null)
                {
                    dataRow[0] = "";
                    dataRow[1] = "";
                }
                else
                {
                    dataRow[0] = dds?.Getting + " А";
                    try
                    {
                        dataRow[1] = dds?.UpperTolerance + " А";
                    }
                    catch (NullReferenceException e)
                    {
                        dataRow[1] = "";
                    }
                }

                // ReSharper disable once PossibleNullReferenceException

                if (dds.IsGood == null)
                    dataRow[2] = "не выполнено";
                else
                    dataRow[2] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperationVerefication<decimal>();
            operation.InitWorkAsync = async () =>
            {
                try
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);

                    Load.FindThisModule();

                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
            };

            operation.BodyWorkAsync = (cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                        var point = Bp.VoltMax * MyPoint[2] / Bp.CurrMax;
                        Load.ResistanceLoad.SetResistanceRange(point).ResistanceLoad.Set(point);
                        Load.SetOutputState(MainN3300.State.On);

                        ////инициализация блока питания
                        Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax * (decimal)0.7M);
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();

                        Bp.SetStateCurr(Bp.CurrMax * (decimal)0.8M);
                        Bp.SetStateCurr(Bp.CurrMax * (decimal)0.9M);
                        Bp.SetStateCurr(Bp.CurrMax);

                        //это нужно для нормальной работы источника
                        Bp.OffOutput();
                        Bp.OnOutput();

                        var currUnstableList = new List<decimal>();

                        foreach (var coef in MyPoint)
                        {
                            var resistance = coef * Bp.VoltMax / Bp.CurrMax;
                            Load.ResistanceLoad.SetResistanceRange(resistance).ResistanceLoad.Set(resistance);
                            Thread.Sleep(3500);
                            currUnstableList.Add(Load.CurrentLoad.MeasureCurrent);
                        }

                        Bp.OffOutput();

                        var resultCurrUnstable = (currUnstableList.Max() - currUnstableList.Min()) / 2;
                        MathStatistics.Round(ref resultCurrUnstable, 3);

                        operation.Expected = 0;
                        SetLowAndUppToleranceAndIsGood_CurrUnstable(operation);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        throw;
                    }
                    finally
                    {
                        Load.SetOutputState(MainN3300.State.Off);
                        Bp.OffOutput();
                    }
                }, cancellationToken);
               
            };
            operation.CompliteWorkAsync = () =>
            {
                if (operation.IsGood()) return Task.FromResult(operation.IsGood());
                var answer =
                    UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                    $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                    "Повторить измерение этой точки?",
                        "Информация по текущему измерению",
                        MessageButton.YesNo, MessageIcon.Question,
                        MessageResult.Yes);

                if (answer == MessageResult.No) return Task.FromResult(true);

                return Task.FromResult(operation.IsGood());
            };
            DataRow.Add(DataRow.IndexOf(operation) == -1
                            ? operation
                            : (BasicOperationVerefication<decimal>)operation.Clone());
        }



        #endregion Methods
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public abstract class Oper9DciPulsation : BaseOparationWithMultimeter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property



        #endregion Property

        protected Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций постоянного тока";

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        /// <inheritdoc />
        protected override string[] GenerateDataColumnTypeObject()
        {
            return new[]
            {
                "Измеренное значение пульсаций, мА",
                "Допустимое значение пульсаций, мА"
            }.Concat(base.GenerateDataColumnTypeObject()).ToArray();
        }

        /// <inheritdoc />
        protected override string GetReportTableName()
        {
            return "FillTabBmDcIPulsation";
        }

        protected override DataTable FillData()
        {
            var dataTable = base.FillData();

            if (DataRow.Count == 1)
            {
                var dataRow = dataTable.NewRow();
                var dds = DataRow[0] as BasicOperationVerefication<decimal>;
                if (dds == null)
                {
                    dataRow[0] = "";
                    dataRow[1] = "";
                }
                else
                {
                    dataRow[0] = dds?.Getting + " мА";
                    try
                    {
                        dataRow[1] = dds?.UpperTolerance + "мА";
                    }
                    catch (NullReferenceException e)
                    {
                        dataRow[1] = "";
                    }
                }

                if (dds.IsGood == null)
                    dataRow[2] = "не выполнено";
                else
                    dataRow[2] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        protected override void InitWork(CancellationTokenSource token)
        {
            base.InitWork(token);
            var operation = new BasicOperationVerefication<decimal>();

            operation.InitWorkAsync = async () =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        Bp.StringConnection = GetStringConnect(Bp);
                        Load.StringConnection = GetStringConnect(Load);
                        Mult.StringConnection = GetStringConnect(Mult);

                        Load.FindThisModule();

                        //если модуль нагрузки найти не удалось
                        if (Load.ChanelNumber <= 0)
                            throw new
                                ArgumentException(
                                                  $"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                        var point = (decimal)0.9M * Bp.VoltMax / Bp.CurrMax;
                        Load.ResistanceLoad.SetResistanceRange(point).ResistanceLoad.Set(point);
                        Load.SetOutputState(MainN3300.State.On);

                        //инициализация блока питания
                        Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax * (decimal)0.7M);
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();

                        Bp.SetStateCurr(Bp.CurrMax * (decimal)0.8M);
                        Bp.SetStateCurr(Bp.CurrMax * (decimal)0.9M);
                        Bp.SetStateCurr(Bp.CurrMax);
                    });

                    while (Mult.IsFrontTerminal)
                        UserItemOperation.ServicePack.MessageBox().Show("На панели прибора " + Mult.UserType +
                                                                        " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                                                        "Указание оператору", MessageButton.OK,
                                                                        MessageIcon.Information,
                                                                        MessageResult.OK);

                    //нужно дать время В3-57
                    Thread.Sleep(5000);

                    UserItemOperation.ServicePack.MessageBox().Show(
                                                                  "Установите на В3-57 подходящий предел измерения напряжения",
                                                                  "Указание оператору", MessageButton.OK,
                                                                  MessageIcon.Information,
                                                                  MessageResult.OK);

                    var windows = UserItemOperation.ServicePack.FreeWindow();
                    var vm = new SelectRangeViewModel();
                    windows.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                    windows.Title = "Выбор предела измерения В3-57";
                    windows.ViewModel = vm;
                    windows.DocumentType = "SelectRangeView";
                    windows.Show();

                    var a = vm.SelectRange;

                   
                    decimal currPuls34401 = -1;
                    while (currPuls34401 <= 0)
                    {
                        Mult.DcVoltage.RangeStorage.SetRange(new MeasPoint<Voltage>(100, UnitMultiplier.Mili));
                        Mult.DcVoltage.RangeStorage.IsAutoRange = true;
                        Mult.DcVoltage.Setting();
                        currPuls34401 = Mult.DcVoltage.GetValue().MainPhysicalQuantity.Value;
                        if (currPuls34401 > 0) break;

                        var answer = UserItemOperation.ServicePack.MessageBox().Show(
                                                                                   "Установите на В3-57 подходящий предел измерения напряжения",
                                                                                   "Указание оператору",
                                                                                   MessageButton.OKCancel,
                                                                                   MessageIcon.Information,
                                                                                   MessageResult.OK);
                        if (answer == MessageResult.Cancel)
                        {
                            UserItemOperation.ServicePack.MessageBox().Show(
                                                                          "Операция измерения пульсаций прервана, измерения не выполнены.",
                                                                          "Указание оператору", MessageButton.OK,
                                                                          MessageIcon.Information,
                                                                          MessageResult.OK);

                            return;
                        }
                    }

                    var currPulsV357 = MathStatistics.Mapping(currPuls34401, 0, (decimal)0.99M, 0, (decimal)a.MainPhysicalQuantity.Value);
                    //по закону ома считаем сопротивление
                    var measResist = Bp.GetMeasureVolt() / Bp.GetMeasureCurr();
                    // считаем пульсации
                    currPulsV357 = currPulsV357 / measResist;
                    MathStatistics.Round(ref currPulsV357, 3);

                    UserItemOperation.ServicePack.MessageBox().Show(
                                                                  "Установите на В3-57 МАКСИМАЛЬНЫЙ предел измерения напряжения",
                                                                  "Указание оператору", MessageButton.OK,
                                                                  MessageIcon.Information,
                                                                  MessageResult.OK);

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = currPulsV357;


                    SetLowAndUppToleranceAndIsGood_CurrPuls(operation);
                    operation.ErrorCalculation = (arg1, arg2) => arg1 - arg2;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
                finally
                {
                    Load.SetOutputState(MainN3300.State.Off);
                    Bp.OffOutput();
                }
            };

            operation.CompliteWorkAsync = () =>
            {
                if (!operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox().Show(operation +
                                                                      $"\nФАКТИЧЕСКАЯ погрешность {operation.Expected - operation.Getting}\n\n" +
                                                                      "Повторить измерение этой точки?",
                                                                      "Информация по текущему измерению",
                                                                      MessageButton.YesNo, MessageIcon.Question,
                                                                      MessageResult.Yes);

                    if (answer == MessageResult.No) return Task.FromResult(true);
                }

                return Task.FromResult(operation.IsGood());
            };
            DataRow.Add(operation);
        }



        #endregion Methods
    }

    internal static class ShemeTemplate
    {
        public static readonly SchemeImage TemplateSheme;

        static ShemeTemplate()
        {
            TemplateSheme = new SchemeImage
            {
                Description = "Измерительная схема",
                Number = 1,
                FileName = @"B5-71-4-PRO_N3303_34401_v3-57.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }
}