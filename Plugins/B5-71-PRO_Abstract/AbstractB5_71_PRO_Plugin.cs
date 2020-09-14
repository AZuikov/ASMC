using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using ASMC.Core.Model;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using NLog;
using WindowService = ASMC.Common.UI.WindowService;

namespace B5_71_PRO_Abstract
{
    /// <summary>
    /// В этом пространчтве имен будет реализован общий алгоритм поверки блоков питания без жесткой привязки к модели
    /// устройства
    /// </summary>
    public abstract class AbstractB571ProPlugin : Program
    {
        #region Property

        public OperationMetrControlBase AbstraktOperation { get; protected set; }

        #endregion

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

    public abstract class OpertionFirsVerf : ASMC.Core.Model.Operation
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

        #endregion
    }

    /// <summary>
    /// Внешний осмотр СИ
    /// </summary>
    public abstract class Oper0VisualTest : ParagraphBase, IUserItemOperation<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = new DataTable {TableName = "ITBmVisualTest"};
            ;
            data.Columns.Add("Результат внешнего осмотра");
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

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => Equals(operation.Getting, operation.Expected);
            operation.InitWork = () =>
            {
                var service = UserItemOperation.ServicePack.QuestionText;
                service.Title = "Внешний осмотр";
                service.Entity = new Tuple<string, Assembly>("VisualTestText", null);
                service.Show();
                var res = service.Entity as Tuple<string, bool>;
                operation.Getting = res.Item2;
                operation.Comment = res.Item1;
                operation.IsGood = () => operation.Getting;

                return Task.CompletedTask;
            };

            operation.CompliteWork = () => { return Task.FromResult(true); };
            DataRow.Add(operation);
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    /// <summary>
    /// Проведение опробования
    /// </summary>
    public abstract class Oper1Oprobovanie : ParagraphBase, IUserItemOperation<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion

        protected Oper1Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable {TableName = "ITBmOprobovanie"};
            data.Columns.Add("Результат опробования");
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

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperationVerefication<bool>();
            operation.InitWork = async () =>
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

                    //while (!Mult.IsTerminal)
                    //    this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                    //                                                       " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                    //                                                       "Указание оператору", MessageButton.OK, MessageIcon.Information,
                    //                                                       MessageResult.OK);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    throw;
                }
            };
            operation.BodyWork = () =>
            {
                try
                {
                    Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);
                    Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                    var resist = Bp.VoltMax / Bp.CurrMax + 3;
                    Load.Resistance.SetResistanceRange(resist).Resistance.Set(resist);
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

                        var measVolt = Math.Abs(Load.Voltage.MeasureVolt);

                        operation.IsGood = () => { return Bp.VoltMax / measVolt >= (decimal) 0.7; };

                        if (!operation.IsGood())
                        {
                            Logger.Error($"Операция опробования не прошла по напряжению в точке {setPoint} В, измерено {operation.Getting} В");
                            return;
                        }
                    }

                    resist = Bp.VoltMax / Bp.CurrMax - 3;
                    Load.Resistance.SetResistanceRange(resist).Resistance.Set(resist);
                    Bp.SetStateVolt(Bp.VoltMax);
                    foreach (var pointMult in MyPoint)
                    {
                        var setPoint = pointMult * Bp.CurrMax;
                        //ставим точку напряжения
                        Bp.SetStateCurr(setPoint);
                        Thread.Sleep(500);
                        //измеряем напряжение

                        var measCurr = Math.Abs(Load.Current.MeasureCurrent);
                        operation.IsGood = () => { return Bp.CurrMax / measCurr >= (decimal) 0.7; };

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
            };
            operation.CompliteWork = () =>
            {
                Load.SetOutputState(MainN3300.State.On);
                Bp.OnOutput();

                //Теперь проверим внешнюю индикацию режимов
                var answer =
                    UserItemOperation.ServicePack.MessageBox
                                     .Show("Сейчас на лицевой панели блока питания индикатор \"СТАБ.ТОКА\" горит?",
                                           "Опробование", MessageButton.YesNo, MessageIcon.Question, MessageResult.Yes);

                if (answer == MessageResult.No)
                {
                    operation.IsGood = () => { return false; };
                    Logger.Error("режим CC: Не горит индикация стабилизации тока на источнике питания.");
                    return Task.FromResult(true);
                }

                var resist = Bp.VoltMax / Bp.CurrMax + 3;
                Load.Resistance.SetResistanceRange(resist).Resistance.Set(resist);

                answer =
                    UserItemOperation.ServicePack.MessageBox
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

                operation.IsGood = () => { return true; };
                return Task.FromResult(true);
            };
            DataRow.Add(operation);
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public abstract class Oper2DcvOutput : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        private static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion

        protected Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcvOutput"};
            dataTable.Columns.Add("Установленное значение напряжения, В");
            dataTable.Columns.Add("Измеренное значение, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");
            dataTable.Columns.Add("Результат");

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

        protected override void InitWork()
        {
            DataRow.Clear();
            foreach (var point in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWork = async () =>
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

                        while (!Mult.IsTerminal)
                            UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
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
                operation.BodyWork = () =>
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
                        var result = Mult.Dc.Voltage.Range.Set((double) setPoint).GetMeasValue();
                        MathStatistics.Round(ref result, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = setPoint;
                        operation.Getting = (decimal) result;
                        //operation.Error = Bp.tolleranceFormulaVolt(setPoint);
                        operation.ErrorCalculation = (c, b) => ErrorCalculation(setPoint);
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);

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
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox.Show(operation +
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
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA)
        {
            inA = Bp.TolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public abstract class Oper3DcvMeasure : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        //порт нужно спрашивать у интерфейса
        protected B571Pro Bp { get; set; }

        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion

        protected Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcvMeasure"};
            dataTable.Columns.Add("Измеренное эталонным мультиметром значение, В");
            dataTable.Columns.Add("Измеренное источником питания значение, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");
            dataTable.Columns.Add("Результат");

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

        protected override void InitWork()
        {
            DataRow.Clear();
            foreach (var point in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWork = async () =>
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

                        while (!Mult.IsTerminal)
                            UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
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
                operation.BodyWork = () =>
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
                        var resultMult = Mult.Dc.Voltage.Range.Set((double) setPoint).GetMeasValue();
                        var resultBp = Bp.GetMeasureVolt();

                        MathStatistics.Round(ref resultMult, 3);
                        MathStatistics.Round(ref resultBp, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = (decimal) resultMult;
                        operation.Getting = resultBp;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);

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
                };

                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox.Show(operation +
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
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.TolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public abstract class Oper4VoltUnstable : ParagraphBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrСoefVoltUnstable = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion

        protected Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcvUnstable"};
            dataTable.Columns.Add("Рассчитанное значение нестабильности (U_МАКС - U_МИН)/2, В");
            dataTable.Columns.Add("Допустимое значение, В");
            dataTable.Columns.Add("Результат");
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
                        dataRow[1] = dds?.Error + " В";
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

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperationVerefication<decimal>();
            operation.InitWork = async () =>
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

                    while (!Mult.IsTerminal)
                        UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
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

            operation.BodyWork = () =>
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

                    Load.Resistance.SetResistanceRange(pointResistance).Resistance.Set(pointResistance)
                        .SetOutputState(MainN3300.State.On);

                    //сюда запишем результаты
                    var voltUnstableList = new List<decimal>();

                    Bp.OnOutput();

                    foreach (var coef in ArrСoefVoltUnstable)
                    {
                        var resistance = Bp.VoltMax / (coef * Bp.CurrMax);
                        Load.Resistance.SetResistanceRange(resistance).Resistance
                            .Set(resistance); //ставим сопротивление

                        // время выдержки
                        Thread.Sleep(1000);
                        // записываем результаты
                        voltUnstableList.Add((decimal) Mult.Dc.Voltage.Range.Set((double) Bp.VoltMax).GetMeasValue());
                    }

                    Bp.OffOutput();

                    //считаем
                    var resultVoltUnstable = (voltUnstableList.Max() - voltUnstableList.Min()) / 2;
                    MathStatistics.Round(ref resultVoltUnstable, 3);

                    //забиваем результаты конкретного измерения для последующей передачи их в протокол

                    operation.Expected = 0;
                    operation.Getting = resultVoltUnstable;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                             (operation.Getting >= operation.LowerTolerance);
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
            };
            operation.CompliteWork = () =>
            {
                if (!operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox.Show(operation +
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
                            : (BasicOperationVerefication<decimal>) operation.Clone());
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceVoltageUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public abstract class Oper5VoltPulsation : ParagraphBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrResistanceVoltUnstable = {(decimal) 20.27, (decimal) 37.5, (decimal) 187.5};

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций по напряжению";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcvPulsation"};
            dataTable.Columns.Add("Измеренное значение пульсаций, мВ");
            dataTable.Columns.Add("Допустимое значение пульсаций, мВ");
            dataTable.Columns.Add("Результат");

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
                        dataRow[1] = dds?.Error + "мВ";
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

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperationVerefication<decimal>();

            operation.InitWork = async () =>
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

                        var point = Bp.VoltMax / ((decimal) 0.9 * Bp.CurrMax);
                        Load.SetWorkingChanel()
                            .SetModeWork(MainN3300.ModeWorks.Resistance)
                            .Resistance.SetResistanceRange(point)
                            .Resistance.Set(point)
                            .SetOutputState(MainN3300.State.On);

                        Bp.InitDevice();
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.SetStateCurr(Bp.CurrMax);
                        Bp.OnOutput();
                    });

                    Mult.Open();
                    while (Mult.IsTerminal)
                        UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                                      " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                                                      "Указание оператору", MessageButton.OK,
                                                                      MessageIcon.Information,
                                                                      MessageResult.OK);

                    Thread.Sleep(5000);
                    UserItemOperation.ServicePack.MessageBox
                                     .Show("Установите на В3-57 подходящий предел измерения напряжения",
                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                           MessageResult.OK);

                    var windows = (WindowService) UserItemOperation.ServicePack.FreeWindow;
                    var vm = new SelectRangeViewModel();
                    windows.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                    windows.Title = "Выбор предела измерения В3-57";
                    windows.MaxHeight = 200;
                    windows.MaxWidth = 350;
                    windows.Show("SelectRangeView", vm);

                    var a = vm.SelectRange;

                    Mult.Dc.Voltage.Range.Set(100);
                    var voltPulsV357 = (decimal) Mult.GetMeasValue();
                    voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
                    voltPulsV357 = MathStatistics.Mapping(voltPulsV357, 0, (decimal) 0.99, 0,
                                                          a.Value);
                    MathStatistics.Round(ref voltPulsV357, 0);

                    UserItemOperation.ServicePack.MessageBox.Show(
                                                                  "Установите на В3-57 МАКСИМАЛЬНЫЙ предел измерения напряжения",
                                                                  "Указание оператору", MessageButton.OK,
                                                                  MessageIcon.Information,
                                                                  MessageResult.OK);

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = voltPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () =>
                        (operation.Getting >= operation.LowerTolerance) &
                        (operation.Getting <= operation.UpperTolerance);
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

            operation.BodyWork = () => { };
            operation.CompliteWork = () =>
            {
                if (operation.IsGood == null) return Task.FromResult(false);
                if (!operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox.Show(operation +
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

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceVoltPuls;
        }

        #endregion

        #region Fileds

        protected B571Pro Bp { get; set; }
        protected Mult_34401A Mult { get; set; }
        protected MainN3300 Load { get; set; }
        public List<IBasicOperation<decimal>> DataRow { get; set; }

        #endregion Fileds
    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public abstract class Oper6DciOutput : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }

        #endregion

        protected Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcIOutput"};
            dataTable.Columns.Add("Установленное значение тока, А");
            dataTable.Columns.Add("Измеренное значение, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");
            dataTable.Columns.Add("Результат");

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

        protected override void InitWork()
        {
            DataRow.Clear();
            foreach (var coef in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();

                operation.InitWork = async () =>
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

                operation.BodyWork = () =>
                {
                    try
                    {
                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                        var resist = Bp.VoltMax / Bp.CurrMax - 3;
                        Load.Resistance.SetResistanceRange(resist).Resistance.Set(resist);
                        Load.SetOutputState(MainN3300.State.On);

                        Bp.InitDevice();
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();

                        var setPoint = coef * Bp.CurrMax;
                        //ставим значение тока
                        Bp.SetStateCurr(setPoint);
                        Thread.Sleep(1000);
                        //измеряем ток

                        var result = Load.Current.MeasureCurrent;

                        MathStatistics.Round(ref result, 3);

                        operation.Expected = setPoint;
                        operation.Getting = result;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);

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
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox.Show(operation +
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
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.TolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public abstract class Oper7DciMeasure : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Fields

        protected B571Pro Bp;

        #endregion

        #region Property

        protected MainN3300 Load { get; set; }

        #endregion

        protected Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcIMeasure"};
            dataTable.Columns.Add("Измеренное эталонным авмперметром значение тока, А");
            dataTable.Columns.Add("Измеренное блоком питания значение тока, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");
            dataTable.Columns.Add("Результат");

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

        protected override void InitWork()
        {
            DataRow.Clear();
            foreach (var coef in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                operation.InitWork = async () =>
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

                operation.BodyWork = () =>
                {
                    try
                    {
                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                        var resist = Bp.VoltMax / Bp.CurrMax - 3;
                        Load.Resistance.SetResistanceRange(resist).Resistance.Set(resist);
                        Load.SetOutputState(MainN3300.State.On);

                        Bp.InitDevice();
                        Bp.SetStateVolt(Bp.VoltMax).SetStateCurr(Bp.CurrMax);
                        Bp.OnOutput();

                        var setPoint = coef * Bp.CurrMax;
                        //ставим точку напряжения
                        Bp.SetStateCurr(setPoint);
                        Thread.Sleep(1000);
                        //измеряем ток
                        var resultN3300 = Load.Current.MeasureCurrent;
                        MathStatistics.Round(ref resultN3300, 3);

                        var resultBpCurr = Bp.GetMeasureCurr();
                        MathStatistics.Round(ref resultBpCurr, 3);

                        operation.Expected = resultN3300;
                        operation.Getting = resultBpCurr;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);

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
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer =
                            UserItemOperation.ServicePack.MessageBox.Show(operation +
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
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.TolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);
            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public abstract class Oper8DciUnstable : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region Fields

        protected B571Pro Bp;
        protected MainN3300 Load;

        #endregion

        protected Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcIUnstable"};
            dataTable.Columns.Add("Рассчитанное значение нестабильности (I_МАКС - I_МИН)/2, А");
            dataTable.Columns.Add("Допустимое значение, А");
            dataTable.Columns.Add("Результат");

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
                        dataRow[1] = dds?.Error + " А";
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

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperationVerefication<decimal>();
            operation.InitWork = async () =>
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

            operation.BodyWork = () =>
            {
                try
                {
                    Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                    var point = Bp.VoltMax * MyPoint[2] / Bp.CurrMax;
                    Load.Resistance.SetResistanceRange(point).Resistance.Set(point);
                    Load.SetOutputState(MainN3300.State.On);

                    ////инициализация блока питания
                    Bp.InitDevice();
                    Bp.SetStateCurr(Bp.CurrMax*(decimal)0.7);
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.OnOutput();

                    Bp.SetStateCurr(Bp.CurrMax * (decimal) 0.8);
                    Bp.SetStateCurr(Bp.CurrMax * (decimal) 0.9);
                    Bp.SetStateCurr(Bp.CurrMax );

                    if (Bp.UserType.Equals("Б5-71/2-ПРО"))
                    {
                        Bp.OffOutput();
                        Bp.OnOutput();
                    }

                    var currUnstableList = new List<decimal>();

                    foreach (var coef in MyPoint)
                    {
                        var resistance = coef * Bp.VoltMax / Bp.CurrMax;
                        Load.Resistance.SetResistanceRange(resistance).Resistance.Set(resistance);
                        Thread.Sleep(3500);
                        currUnstableList.Add(Load.Current.MeasureCurrent);
                    }

                    Bp.OffOutput();

                    var resultCurrUnstable = (currUnstableList.Max() - currUnstableList.Min()) / 2;
                    MathStatistics.Round(ref resultCurrUnstable, 3);

                    operation.Expected = 0;
                    operation.Getting = resultCurrUnstable;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                             (operation.Getting >= operation.LowerTolerance);
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
            operation.CompliteWork = () =>
            {
                if (!operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox.Show(operation +
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
                            : (BasicOperationVerefication<decimal>) operation.Clone());
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceCurrentUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public abstract class Oper9DciPulsation : ParagraphBase, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion

        protected Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций постоянного тока";
            DataRow = new List<IBasicOperation<decimal>>();

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "FillTabBmDcIPulsation"};

            dataTable.Columns.Add("Измеренное значение пульсаций, мА");
            dataTable.Columns.Add("Допустимое значение пульсаций, мА");
            dataTable.Columns.Add("Результат");

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
                        dataRow[1] = dds?.Error + "мА";
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

        protected override void InitWork()
        {
            DataRow.Clear();
            var operation = new BasicOperationVerefication<decimal>();

            operation.InitWork = async () =>
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
                        var point = (decimal) 0.9 * Bp.VoltMax / Bp.CurrMax;
                        Load.Resistance.SetResistanceRange(point).Resistance.Set(point);
                        Load.SetOutputState(MainN3300.State.On);

                        //инициализация блока питания
                        Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax * (decimal) 0.7);
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();

                        Bp.SetStateCurr(Bp.CurrMax * (decimal) 0.8);
                        Bp.SetStateCurr(Bp.CurrMax * (decimal) 0.9);
                        Bp.SetStateCurr(Bp.CurrMax);
                    });

                    while (Mult.IsTerminal)
                        UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                                      " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                                                      "Указание оператору", MessageButton.OK,
                                                                      MessageIcon.Information,
                                                                      MessageResult.OK);

                    //нужно дать время В3-57
                    Thread.Sleep(5000);

                    UserItemOperation.ServicePack.MessageBox.Show(
                                                                  "Установите на В3-57 подходящий предел измерения напряжения",
                                                                  "Указание оператору", MessageButton.OK,
                                                                  MessageIcon.Information,
                                                                  MessageResult.OK);

                    var windows = (WindowService) UserItemOperation.ServicePack.FreeWindow;
                    var vm = new SelectRangeViewModel();
                    windows.ViewLocator = new ViewLocator(Assembly.GetExecutingAssembly());
                    windows.Title = "Выбор предела измерения В3-57";
                    windows.MaxHeight = 200;
                    windows.MaxWidth = 350;
                    windows.Show("SelectRangeView", vm);

                    var a = vm.SelectRange;

                    Mult.Dc.Voltage.Range.Set(100);
                    decimal currPuls34401 = -1;
                    while (currPuls34401 <= 0)
                    {
                        currPuls34401 = (decimal) Mult.GetMeasValue();
                        if (currPuls34401 > 0) break;

                        var answer = UserItemOperation.ServicePack.MessageBox.Show(
                                                                                   "Установите на В3-57 подходящий предел измерения напряжения",
                                                                                   "Указание оператору",
                                                                                   MessageButton.OKCancel,
                                                                                   MessageIcon.Information,
                                                                                   MessageResult.OK);
                        if (answer == MessageResult.Cancel)
                        {
                            UserItemOperation.ServicePack.MessageBox.Show(
                                                                          "Операция измерения пульсаций прервана, измерения не выполнены.",
                                                                          "Указание оператору", MessageButton.OK,
                                                                          MessageIcon.Information,
                                                                          MessageResult.OK);

                            return;
                        }
                    }

                    var currPulsV357 = MathStatistics.Mapping(currPuls34401, 0, (decimal) 0.99, 0, a.Value);
                    //по закону ома считаем сопротивление
                    var measResist = Bp.GetMeasureVolt() / Bp.GetMeasureCurr();
                    // считаем пульсации
                    currPulsV357 = currPulsV357 / measResist;
                    MathStatistics.Round(ref currPulsV357, Bp.TolleranceCurrentPuls.ToString());

                    UserItemOperation.ServicePack.MessageBox.Show(
                                                                  "Установите на В3-57 МАКСИМАЛЬНЫЙ предел измерения напряжения",
                                                                  "Указание оператору", MessageButton.OK,
                                                                  MessageIcon.Information,
                                                                  MessageResult.OK);

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = currPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () => (operation.Getting <= operation.UpperTolerance) &
                                             (operation.Getting >= operation.LowerTolerance);
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

            operation.CompliteWork = () =>
            {
                if (operation.IsGood == null) return Task.FromResult(true);
                if (!operation.IsGood())
                {
                    var answer =
                        UserItemOperation.ServicePack.MessageBox.Show(operation +
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

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceCurrentPuls;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }
    }

    internal static class ShemeTemplate
    {
        public static readonly ShemeImage TemplateSheme;

        static ShemeTemplate()
        {
            TemplateSheme = new ShemeImage
            {
                Description = "Измерительная схема",
                Number = 1,
                FileName = @"B5-71-4-PRO_N3303_34401_v3-57.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }
}