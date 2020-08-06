using AP.Math;
using ASMC.Core.Helps;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using DevExpress.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    public abstract class OpertionFirsVerf : ASMC.Data.Model.Operation
    {
        /// <inheritdoc />
        public override void FindDivice()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void RefreshDevice()
        {
            AddresDevice = IeeeBase.AllStringConnect;
        }

        protected OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
            this.DocumentName = "Б5-71_1";
        }
    }

    /// <summary>
    /// Внешний осмотр СИ
    /// </summary>
    public abstract class Oper0VisualTest : ParagraphBase, IUserItemOperation<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        

        protected override void InitWork()
        {
            var operation = new BasicOperation<bool>();
            operation.Expected = true;
            operation.IsGood = () => operation.Getting == operation.Expected;
            operation.InitWork = () =>
            {
                var service = this.UserItemOperation.ServicePack.QuestionText;
                service.Title = "Внешний осмотр";
                service.Show();
                operation.Getting = true;
                return Task.CompletedTask;
            };

            operation.CompliteWork = () => Task.FromResult(operation.IsGood());
            DataRow.Add(operation);
        }

        protected Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperation<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        #endregion Methods

        public List<IBasicOperation<bool>> DataRow { get; set; }

        /// <inheritdoc />
        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        /// <inheritdoc />
        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow)
                await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Проведение опробования
    /// </summary>
    public abstract class Oper1Oprobovanie : ParagraphBase, IUserItemOperation<bool>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected Oper1Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат опробования");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        #endregion Methods

        public override async Task StartWork(CancellationToken token)
        {
            var bo = new BasicOperation<bool> { Expected = true };
            bo.IsGood = () => bo.Getting;
            bo.InitWork = () =>
            {
                this.UserItemOperation.ServicePack.MessageBox.Show("Начало операции", "Начало операции2", MessageButton.OK, MessageIcon.Information,
                                       MessageResult.No);
                return Task.CompletedTask;
            };
            bo.CompliteWork = () =>
            {
                this.UserItemOperation.ServicePack.MessageBox.Show("Конец операции", "Конец операции2", MessageButton.OK, MessageIcon.Information,
                                       MessageResult.No);
                return Task.FromResult(true);
            };
            bo.BodyWork = () => { Thread.Sleep(100); };
            await bo.WorkAsync(token);
            DataRow.Add(bo);
        }

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }

    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public abstract class Oper2DcvOutput : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        private static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion Property

        protected Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table2" };
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
                dataRow[0] = dds.Expected + " В";
                dataRow[1] = dds.Getting + " В";
                dataRow[2] = dds.LowerTolerance + " В";
                dataRow[3] = dds.UpperTolerance + " В";
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
                            this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                     " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                                     "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                     MessageResult.OK);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
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
                        var result = Mult.Dc.Voltage.Range.Set((double)setPoint).GetMeasValue();
                        MathStatistics.Round(ref result, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = setPoint;
                        operation.Getting = (decimal)result;
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
                    }
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer = ShowTolleranceDialog(operation);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>)operation.Clone());
            };
        }

        private decimal ErrorCalculation(decimal inA)
        {
            inA = Bp.TolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion Methods

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public abstract class Oper3DcvMeasure : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };

        #region Property

        //порт нужно спрашивать у интерфейса
        protected B571Pro Bp { get; set; }

        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion Property

        protected Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table3" };
            dataTable.Columns.Add("Измеренное эталонным мультиметром значение, В");
            dataTable.Columns.Add("Измеренное источником питания значение, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");
            dataTable.Columns.Add("Результат");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Expected + " В";
                dataRow[1] = dds.Getting + " В";
                dataRow[2] = dds.LowerTolerance + " В";
                dataRow[3] = dds.UpperTolerance + " В";
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
                            this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                                               " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                MessageResult.OK);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
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
                        var resultMult = Mult.Dc.Voltage.Range.Set((double)setPoint).GetMeasValue();
                        var resultBp = Bp.GetMeasureVolt();

                        MathStatistics.Round(ref resultMult, 3);
                        MathStatistics.Round(ref resultBp, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = (decimal)resultMult;
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
                    }
                };

                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer = ShowTolleranceDialog(operation);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                    ? operation
                    : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.TolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion Methods

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public abstract class Oper4VoltUnstable : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrСoefVoltUnstable = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion Property

        protected Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table4" };
            dataTable.Columns.Add("Рассчитанное значение нестабильности (U_МАКС - U_МИН)/2, В");
            dataTable.Columns.Add("Допустимое значение, В");
            dataTable.Columns.Add("Результат");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting + " В";
            dataRow[1] = dds.Error + " В";
            dataRow[2] = dds.IsGood() ? "Годен" : "Брак";

            dataTable.Rows.Add(dataRow);

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
                        this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                                           " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                            "Указание оператору", MessageButton.OK, MessageIcon.Information,
                            MessageResult.OK);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
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

                    decimal pointResistance = Bp.VoltMax / (Bp.CurrMax * ArrСoefVoltUnstable[2]);
                    MathStatistics.Round(ref pointResistance, 3);

                    Load.Resistance.SetResistanceRange(pointResistance).
                        Resistance.Set(pointResistance)
                        .SetOutputState(MainN3300.State.On);

                    //сюда запишем результаты
                    var voltUnstableList = new List<decimal>();

                    Bp.OnOutput();

                    foreach (var coef in ArrСoefVoltUnstable)
                    {
                        var resistance = Bp.VoltMax / (coef * Bp.CurrMax);
                        Load.Resistance.SetResistanceRange(resistance).Resistance.Set(resistance); //ставим сопротивление

                        // время выдержки
                        Thread.Sleep(1000);
                        // записываем результаты
                        voltUnstableList.Add((decimal)Mult.Dc.Voltage.Range.Set((double)Bp.VoltMax).GetMeasValue());
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
                }
            };
            operation.CompliteWork = () =>
            {
                if (!operation.IsGood())
                {
                    var answer = ShowTolleranceDialog(operation);

                    if (answer == MessageResult.No) return Task.FromResult(true);
                }

                return Task.FromResult(operation.IsGood());
            };
            DataRow.Add(DataRow.IndexOf(operation) == -1
                ? operation
                : (BasicOperationVerefication<decimal>)operation.Clone());
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceVoltageUnstability;
        }

        #endregion Methods

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public abstract class Oper5VoltPulsation : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrResistanceVoltUnstable = { (decimal)20.27, (decimal)37.5, (decimal)187.5 };

        protected Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций по напряжению";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table5" };
            dataTable.Columns.Add("Измеренное значение пульсаций, мВ");
            dataTable.Columns.Add("Допустимое значение пульсаций, мВ");
            dataTable.Columns.Add("Результат");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;

            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting + " мВ";
            dataRow[1] = dds.Error + " мВ";
            dataRow[2] = dds.IsGood() ? "Годен" : "Брак";
            dataTable.Rows.Add(dataRow);

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

                          var point = Bp.VoltMax / ((decimal)0.9 * Bp.CurrMax);
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
                        this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                                           " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                            "Указание оператору", MessageButton.OK, MessageIcon.Information,
                            MessageResult.OK);

                    this.UserItemOperation.ServicePack.MessageBox.Show("Установите на В3-57 подходящий предел измерения напряжения",
                        "Указание оператору", MessageButton.OK, MessageIcon.Information,
                        MessageResult.OK);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            };

            operation.BodyWork = () =>
            {
                try
                {
                    Thread.Sleep(5000);

                    Mult.Dc.Voltage.Range.Set(100);
                    var voltPulsV357 = (decimal)Mult.GetMeasValue();
                    voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
                    voltPulsV357 = MathStatistics.Mapping(voltPulsV357, 0, (decimal)0.99, 0, 3);
                    MathStatistics.Round(ref voltPulsV357, Bp.TolleranceVoltPuls.ToString());

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = voltPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () =>
                        (operation.Expected >= operation.LowerTolerance) &
                        (operation.Expected < operation.UpperTolerance);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            };
            operation.CompliteWork = () =>
            {
                if (!operation.IsGood())
                {
                    var answer = ShowTolleranceDialog(operation);

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

        #endregion Methods

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }

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
    public abstract class Oper6DciOutput : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }

        #endregion Property

        protected Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table6" };
            dataTable.Columns.Add("Установленное значение тока, А");
            dataTable.Columns.Add("Измеренное значение, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");
            dataTable.Columns.Add("Результат");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Expected + " А";
                dataRow[1] = dds.Getting + " А";
                dataRow[2] = dds.LowerTolerance + " А";
                dataRow[3] = dds.UpperTolerance + " А";
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
                                throw new ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
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

                        var result = Load.Current.MeasCurrent;

                        MathStatistics.Round(ref result, 3);

                        operation.Expected = setPoint;
                        operation.Getting = result;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);
                        operation.CompliteWork = () => Task.FromResult(operation.IsGood());

                        Bp.OffOutput();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer = ShowTolleranceDialog(operation);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                    ? operation
                    : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.TolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion Methods

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public abstract class Oper7DciMeasure : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };

        #region Fields

        protected B571Pro Bp;

        #endregion Fields

        #region Property

        protected MainN3300 Load { get; set; }

        #endregion Property

        protected Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table7" };
            dataTable.Columns.Add("Измеренное эталонным авмперметром значение тока, А");
            dataTable.Columns.Add("Измеренное блоком питания значение тока, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");
            dataTable.Columns.Add("Результат");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Expected + " А";
                dataRow[1] = dds.Getting + " А";
                dataRow[2] = dds.LowerTolerance + " А";
                dataRow[3] = dds.UpperTolerance + " А";
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
                        var resultN3300 = Load.Current.MeasCurrent;
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
                    }
                };
                operation.CompliteWork = () =>
                {
                    if (!operation.IsGood())
                    {
                        var answer = ShowTolleranceDialog(operation);

                        if (answer == MessageResult.No) return Task.FromResult(true);
                    }

                    return Task.FromResult(operation.IsGood());
                };
                DataRow.Add(DataRow.IndexOf(operation) == -1
                    ? operation
                    : (BasicOperationVerefication<decimal>)operation.Clone());
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.TolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);
            return inA;
        }

        #endregion Methods

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public abstract class Oper8DciUnstable : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };

        #region Fields

        protected B571Pro Bp;
        protected MainN3300 Load;

        #endregion Fields

        protected Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table8" };
            dataTable.Columns.Add("Рассчитанное значение нестабильности (I_МАКС - I_МИН)/2, А");
            dataTable.Columns.Add("Допустимое значение, А");
            dataTable.Columns.Add("Результат");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting + " А";
            dataRow[1] = dds.Error + " А";
            dataRow[2] = dds.IsGood() ? "Годен" : "Брак";

            dataTable.Rows.Add(dataRow);

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
                    Bp.SetStateCurr(Bp.CurrMax);
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.OnOutput();

                    var currUnstableList = new List<decimal>();

                    foreach (var coef in MyPoint)
                    {
                        var resistance = coef * Bp.VoltMax / Bp.CurrMax;
                        Load.Resistance.SetResistanceRange(resistance).Resistance.Set(resistance);
                        Thread.Sleep(1000);
                        currUnstableList.Add(Load.Current.MeasCurrent);
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
                }
            };
            operation.CompliteWork = () =>
            {
                if (!operation.IsGood())
                {
                    var answer = ShowTolleranceDialog(operation);

                    if (answer == MessageResult.No) return Task.FromResult(true);
                }

                return Task.FromResult(operation.IsGood());
            };
            DataRow.Add(DataRow.IndexOf(operation) == -1
                ? operation
                : (BasicOperationVerefication<decimal>)operation.Clone());
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceCurrentUnstability;
        }

        #endregion Methods

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public abstract class Oper9DciPulsation : TolleranceDialog, IUserItemOperation<decimal>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion Property

        protected Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций постоянного тока";
            DataRow = new List<IBasicOperation<decimal>>();

            Sheme = ShemeTemplate.TemplateSheme;
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable { TableName = "table9" };

            dataTable.Columns.Add("Измеренное значение пульсаций, мА");
            dataTable.Columns.Add("Допустимое значение пульсаций, мА");
            dataTable.Columns.Add("Результат");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = dds.Getting + " мА";
            dataRow[1] = dds.Error + " мА";
            dataRow[2] = dds.IsGood() ? "Годен" : "Брак";

            dataTable.Rows.Add(dataRow);

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
                        var point = (decimal)0.9 * Bp.VoltMax / Bp.CurrMax;
                        Load.Resistance.SetResistanceRange(point).Resistance.Set(point);
                        Load.SetOutputState(MainN3300.State.On);

                            //инициализация блока питания
                            Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax);
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();
                    });

                    while (Mult.IsTerminal)
                        this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                                           " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                            "Указание оператору", MessageButton.OK, MessageIcon.Information,
                            MessageResult.OK);

                    this.UserItemOperation.ServicePack.MessageBox.Show(
                        "Установите на В3-57 подходящий предел измерения напряжения",
                        "Указание оператору", MessageButton.OK, MessageIcon.Information,
                        MessageResult.OK);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            };

            operation.BodyWork = () =>
            {
                try
                {
                    //нужно дать время В3-57
                    Thread.Sleep(5000);
                    Mult.Dc.Voltage.Range.Set(100);
                    var currPuls34401 = (decimal)Mult.GetMeasValue();
                    var currPulsV357 = MathStatistics.Mapping(currPuls34401, 0, (decimal)0.99, 0, 3);
                    //по закону ома считаем сопротивление
                    var measResist = Bp.GetMeasureVolt() / Bp.GetMeasureCurr();
                    // считаем пульсации
                    currPulsV357 = currPulsV357 / measResist;
                    MathStatistics.Round(ref currPulsV357, Bp.TolleranceCurrentPuls.ToString());

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = currPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                             (operation.Getting >= operation.LowerTolerance);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            };
            operation.CompliteWork = () =>
            {
                if (!operation.IsGood())
                {
                    var answer = ShowTolleranceDialog(operation);

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

        #endregion Methods

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
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
                FileName = @"B5-71-1_2-PRO_N3306_34401_v3-57.jpg",
                ExtendedDescription = "Соберите измерительную схему, согласно рисунку"
            };
        }
    }
}