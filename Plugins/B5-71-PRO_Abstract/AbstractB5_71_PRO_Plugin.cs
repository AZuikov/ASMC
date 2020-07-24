using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AP.Math;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using DevExpress.Mvvm;

namespace B5_71_PRO_Abstract
{
    /// <summary>
    /// В этом пространчтве имен будет реализован общий алгоритм поверки блоков питания без жесткой привязки к модели
    /// устройства
    /// </summary>
    public abstract class AbstractB571ProPlugin : AbstractProgram
    {
        #region Property

        public OperationBase AbstraktOperation { get; protected set; }

        #endregion

        protected AbstractB571ProPlugin()
        {
            Grsi = "42467-09";
            Accuracy = "Напряжение ПГ ±(0,002 * U + 0.1); ток ПГ ±(0,01 * I + 0,05)";
        }
    }

    public class Operation : OperationBase
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.

        public Operation()
        {
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }

    public class UseDevices : IDevice
    {
        public bool IsCanStringConnect { get; set; } = true;
        public string Description { get; set; }
        public string[] Name { get; set; }
        public string SelectedName { get; set; }
        public string StringConnect { get; set; }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool? IsConnect { get; } = true;
    }

    public abstract class OpertionFirsVerf : IUserItemOperation
    {
        public IDevice[] TestDevices { get; set; }
        public IUserItemOperationBase[] UserItemOperation { get; set; }
        public string[] Accessories { get; protected set; }
        public string[] AddresDivece { get; set; }
        public IDevice[] ControlDevices { get; set; }

        /// <summary>
        /// Проверяет всели эталоны подключены
        /// </summary>
        public void RefreshDevice()
        {
            AddresDivece = new IeeeBase().GetAllDevace.ToArray();
        }

        public void FindDivice()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Внешний осмотр СИ
    /// </summary>
    public abstract class Oper0VisualTest : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
        //public override async Task StartWork(CancellationToken token)
        //{
        //    BasicOperation<bool> bo = new BasicOperation<bool>();
        //    bo.Expected = true;
        //    bo.IsGood = s => { return bo.Getting == true ? true : false; };

        //    DataRow.Add(bo);
        //}


        protected Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<bool>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        #endregion

        public List<IBasicOperation<bool>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var bo = new BasicOperation<bool> {Expected = true};
            bo.IsGood = () => bo.Getting;
            bo.InitWork = () =>
            {
                MessageBoxService.Show("Начало операции", "Начало операции1", MessageButton.OK, MessageIcon.Information,
                                       MessageResult.No);
                return Task.CompletedTask;
            };
            bo.CompliteWork = () =>
            {
                MessageBoxService.Show("Конец операции", "Конец операции1", MessageButton.OK, MessageIcon.Information,
                                       MessageResult.No);
                return Task.FromResult(true);
            };
            bo.BodyWork = () => { Thread.Sleep(100); };
            await bo.WorkAsync(token);
            DataRow.Add(bo);
        }
    }


    /// <summary>
    /// Проведение опробования
    /// </summary>
    public abstract class Oper1Oprobovanie : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
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

        #endregion


        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var bo = new BasicOperation<bool> {Expected = true};
            bo.IsGood = () => bo.Getting;
            bo.InitWork = () =>
            {
                MessageBoxService.Show("Начало операции", "Начало операции2", MessageButton.OK, MessageIcon.Information,
                                       MessageResult.No);
                return Task.CompletedTask;
            };
            bo.CompliteWork = () =>
            {
                MessageBoxService.Show("Конец операции", "Конец операции2", MessageButton.OK, MessageIcon.Information,
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
    public abstract class Oper2DcvOutput : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        private static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        protected B5_71_PRO Bp { get; set; }
        protected Main_N3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion


        protected Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table2"};
            dataTable.Columns.Add("Установленное значение напряжения, В");
            dataTable.Columns.Add("Измеренное значение, В");
            dataTable.Columns.Add("Абсолютная погрешность, В");
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
                dataRow[2] = dds.Error + " В";
                dataRow[3] = dds.LowerTolerance + " В";
                dataRow[4] = dds.UpperTolerance + " В";
                dataRow[5] = dds.IsGood() ? "Годен" : "Брак";
                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA)
        {
            inA = Bp.tolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();
            try
            {
                operation.InitWork = () =>
                {
                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    Mult.Open();
                    while (Mult.GetTerminalConnect() == false)
                        MessageBoxService.Show("На панели прибора " + Mult.GetDeviceType +
                                               " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Mult.Open();
                    Load.Open();
                    Load.SetWorkingChanel();
                    Load.OffOutput();

                    Bp.InitDevice();
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.SetStateCurr(Bp.CurrMax);


                    foreach (var point in MyPoint)
                    {
                        var setPoint = point * Bp.VoltMax;
                        //ставим точку напряжения
                        Bp.SetStateVolt(setPoint);
                        Bp.OnOutput();
                        Thread.Sleep(1000);

                        //измеряем напряжение
                        Mult.WriteLine(Main_Mult.DC.Voltage.Range.V100);
                        Mult.WriteLine(Main_Mult.QueryValue);
                        var result = Mult.DataPreparationAndConvert(Mult.ReadString());
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
                        operation.CompliteWork = () => Task.FromResult(operation.IsGood());

                        var a = (BasicOperationVerefication<decimal>) operation.Clone();

                        DataRow.Add(a);
                    }

                    Bp.OffOutput();
                    Load.OffOutput();
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Mult.Close();
                Load.Close();
                Bp.OffOutput();
                Bp.Close();
            }
        }
    }

    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public abstract class Oper3DcvMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        //порт нужно спрашивать у интерфейса
        protected B5_71_PRO Bp { get; set; }
        protected Main_N3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion


        protected Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table3"};
            dataTable.Columns.Add("Измеренное эталонным мультиметром значение, В");
            dataTable.Columns.Add("Измеренное источником питания значение, В");
            dataTable.Columns.Add("Абсолютная погрешность, В");
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
                dataRow[2] = dds.Error + " В";
                dataRow[3] = dds.LowerTolerance + " В";
                dataRow[4] = dds.UpperTolerance + " В";
                dataRow[5] = dds.IsGood() ? "Годен" : "Брак";


                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.tolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    Mult.Open();
                    while (Mult.GetTerminalConnect() == false)
                        MessageBoxService.Show("На панели прибора " + Mult.GetDeviceType +
                                               " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Mult.Open();
                    Load.Open();
                    Load.SetWorkingChanel();
                    Load.OffOutput();

                    Bp.InitDevice();
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.SetStateCurr(Bp.CurrMax);


                    foreach (var point in MyPoint)
                    {
                        var setPoint = point * Bp.VoltMax;
                        //ставим точку напряжения
                        Bp.SetStateVolt(setPoint);
                        Bp.OnOutput();
                        Thread.Sleep(1000);

                        //измеряем напряжение
                        Mult.WriteLine(Main_Mult.DC.Voltage.Range.V100);
                        Mult.WriteLine(Main_Mult.QueryValue);
                        var resultMult = Mult.DataPreparationAndConvert(Mult.ReadString());
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
                        operation.CompliteWork = () => Task.FromResult(operation.IsGood());


                        DataRow.Add(operation);
                    }


                    Bp.OffOutput();
                    Load.OffOutput();
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Mult.Close();
                Load.Close();
                Bp.OffOutput();
                Bp.Close();
            }
        }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public abstract class Oper4VoltUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах

        public static readonly decimal[] ArrСoefVoltUnstable = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region Property

        //порт нужно спрашивать у интерфейса
        protected B5_71_PRO Bp { get; set; }
        protected Main_N3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion

        protected Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного напряжения";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table4"};
            dataTable.Columns.Add("Рассчитанное значение нестабильности (U_МАКС - U_МИН)/2, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");
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

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.tolleranceVoltageUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    Mult.Open();
                    while (Mult.GetTerminalConnect() == false)
                        MessageBoxService.Show("На панели прибора " + Mult.GetDeviceType +
                                               " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Mult.Open();
                    Load.Open();
                    Load.SetWorkingChanel();
                    Load.OffOutput();

                    Bp.InitDevice();
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.SetStateCurr(Bp.CurrMax);

                    // ------ настроим нагрузку
                    Load.SetWorkingChanel();
                    Load.SetResistanceFunc();

                    Load.SetMaxResistanceRange();
                    Load.SetResistance(Bp.VoltMax / (Bp.CurrMax * ArrСoefVoltUnstable[2]));
                    Load.OnOutput();
                    Load.Close();

                    //сюда запишем результаты
                    var voltUnstableList = new List<decimal>();

                    Bp.OnOutput();

                    foreach (var coef in ArrСoefVoltUnstable)
                    {
                        Load.Open();
                        var resistance = Bp.VoltMax / (coef * Bp.CurrMax);
                        Load.SetResistanceRange(resistance);
                        Load.SetResistance(resistance); //ставим сопротивление

                        // время выдержки
                        Thread.Sleep(1000);
                        //измерения
                        Mult.Open();
                        Mult.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
                        Mult.WriteLine(Main_Mult.QueryValue);
                        // записываем результаты
                        voltUnstableList.Add((decimal) Mult.DataPreparationAndConvert(Mult.ReadString()));
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
                    DataRow.Add(operation);
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Load.Close();
                Mult.Close();
            }
        }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public abstract class Oper5VoltPulsation : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrResistanceVoltUnstable = {(decimal) 20.27, (decimal) 37.5, (decimal) 187.5};

        protected Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций по напряжению";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table5"};
            dataTable.Columns.Add("Величина напряжения на выходе источника питания, В");
            dataTable.Columns.Add("Измеренное значение пульсаций, мВ");
            dataTable.Columns.Add("Допустимое значение пульсаций, мВ");
            dataTable.Columns.Add("Результат");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = Bp.VoltMax + " В";
            // ReSharper disable once PossibleNullReferenceException
            dataRow[1] = dds.Getting + " мВ";
            dataRow[2] = dds.Error + " мВ";
            dataRow[3] = dds.IsGood() ? "Годен" : "Брак";
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.tolleranceVoltPuls;
        }

        #endregion


        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = async () =>
                {
                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);


                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    await Task.Factory.StartNew(() =>
                    {
                        Load.Open();
                        Load.SetWorkingChanel();
                        Load.SetResistanceFunc();
                        var point = Bp.VoltMax / ((decimal) 0.9 * Bp.CurrMax);
                        Load.SetResistanceRange(point);
                        Load.SetResistance(point);
                        Load.OnOutput();
                        Load.Close();

                        Bp.InitDevice();
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.SetStateCurr(Bp.CurrMax);
                        Bp.OnOutput();
                    }, token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);


                    Mult.Open();
                    while (Mult.GetTerminalConnect())
                        MessageBoxService.Show("На панели прибора " + Mult.GetDeviceType +
                                               " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);

                    MessageBoxService.Show("Установите на В3-57 подходящий предел измерения напряжения",
                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                           MessageResult.OK);
                };

                operation.BodyWork = Test;

                void Test()
                {
                    Thread.Sleep(5000);

                    Mult.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
                    Mult.WriteLine(Main_Mult.QueryValue);

                    var voltPulsV357 = (decimal) Mult.DataPreparationAndConvert(Mult.ReadString());
                    voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
                    voltPulsV357 = MathStatistics.Mapping(voltPulsV357, 0, (decimal) 0.99, 0, 3);
                    MathStatistics.Round(ref voltPulsV357, 2);

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = voltPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () =>
                        (operation.Expected >= operation.LowerTolerance) &
                        (operation.Expected < operation.UpperTolerance);

                    operation.CompliteWork = () => Task.FromResult(operation.IsGood());

                    DataRow.Add(operation);
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Mult.Close();
            }
        }

        #region Fileds

        protected B5_71_PRO Bp { get; set; }
        protected Mult_34401A Mult { get; set; }
        protected Main_N3300 Load { get; set; }
        public List<IBasicOperation<decimal>> DataRow { get; set; }

        #endregion
    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public abstract class Oper6DciOutput : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region Property

        protected B5_71_PRO Bp { get; set; }
        protected Main_N3300 Load { get; set; }

        #endregion


        protected Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table6"};
            dataTable.Columns.Add("Установленное значение тока, А");
            dataTable.Columns.Add("Измеренное значение, А");
            dataTable.Columns.Add("Абсолютная погрешность, А");
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
                dataRow[2] = dds.Error + " А";
                dataRow[3] = dds.LowerTolerance + " А";
                dataRow[4] = dds.UpperTolerance + " А";
                dataRow[5] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.tolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();


            try
            {
                operation.InitWork = () =>
                {
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Load.Open();
                    Load.SetWorkingChanel();
                    Load.SetResistanceFunc();
                    Load.SetResistanceRange(Bp.VoltMax / Bp.CurrMax - 3);
                    Load.SetResistance(Bp.VoltMax / Bp.CurrMax - 3);
                    Load.OnOutput();
                    Load.Close();

                    Bp.InitDevice();
                    Bp.SetStateCurr(Bp.CurrMax);
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.OnOutput();

                    foreach (var coef in MyPoint)
                    {
                        var setPoint = coef * Bp.CurrMax;
                        //ставим значение тока
                        Bp.SetStateCurr(setPoint);
                        Thread.Sleep(1000);
                        //измеряем ток
                        Load.Open();
                        var result = Load.GetMeasCurr();

                        MathStatistics.Round(ref result, 3);

                        operation.Expected = setPoint;
                        operation.Getting = result;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);
                        operation.CompliteWork = () => Task.FromResult(operation.IsGood());
                        DataRow.Add(operation);
                    }

                    Bp.OffOutput();
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Load.Close();
            }
        }
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public abstract class Oper7DciMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region  Fields

        protected B5_71_PRO Bp;

        #endregion

        #region Property

        protected Main_N3300 Load { get; set; }

        #endregion

        protected Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table7"};
            dataTable.Columns.Add("Измеренное эталонным авмперметром значение тока, А");
            dataTable.Columns.Add("Измеренное блоком питания значение тока, А");
            dataTable.Columns.Add("Абсолютная погрешность, А");
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
                dataRow[2] = dds.Error + " А";
                dataRow[3] = dds.LowerTolerance + " А";
                dataRow[4] = dds.UpperTolerance + " А";
                dataRow[5] = dds.IsGood() ? "Годен" : "Брак";

                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = Bp.tolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);
            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);


                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Load.Open();
                    Load.SetWorkingChanel();
                    Load.SetResistanceFunc();
                    Load.SetResistanceRange(Bp.VoltMax / Bp.CurrMax - 3);
                    Load.SetResistance(Bp.VoltMax / Bp.CurrMax - 3);
                    Load.OnOutput();
                    Load.Close();

                    Bp.InitDevice();
                    Bp.SetStateCurr(Bp.CurrMax);
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.OnOutput();

                    Load.Open();

                    foreach (var coef in MyPoint)
                    {
                        var setPoint = coef * Bp.CurrMax;
                        //ставим точку напряжения
                        Bp.SetStateCurr(setPoint);
                        Thread.Sleep(1000);
                        //измеряем ток
                        var resultN3300 = Load.GetMeasCurr();

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
                        operation.CompliteWork = () => Task.FromResult(operation.IsGood());
                        DataRow.Add(operation);
                    }


                    Bp.OffOutput();
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Load.Close();
            }
        }
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public abstract class Oper8DciUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        protected B5_71_PRO Bp;
        protected Main_N3300 Load;

        #endregion

        protected Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного тока";
            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table8"};
            dataTable.Columns.Add("Рассчитанное значение нестабильности (I_МАКС - I_МИН)/2, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");
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

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.tolleranceCurrentUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);

                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Load.Open();
                    Load.SetWorkingChanel();
                    Load.SetResistanceFunc();
                    var point = Bp.VoltMax * MyPoint[2] / Bp.CurrMax;
                    Load.SetResistanceRange(point);
                    Load.SetResistance(point);
                    Load.OnOutput();
                    Load.Close();

                    ////инициализация блока питания
                    Bp.InitDevice();
                    Bp.SetStateCurr(Bp.CurrMax);
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.OnOutput();

                    var currUnstableList = new List<decimal>();

                    foreach (var coef in MyPoint)
                    {
                        Load.Open();
                        var resistance = coef * Bp.VoltMax / Bp.CurrMax;
                        Load.SetResistanceRange(resistance);
                        Load.SetResistance(resistance);
                        Thread.Sleep(1000);
                        currUnstableList.Add(Load.GetMeasCurr());
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
                    operation.CompliteWork = () => Task.FromResult(operation.IsGood());
                    DataRow.Add(operation);
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Load.Close();
            }
        }
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public abstract class Oper9DciPulsation : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        #region Property

        protected B5_71_PRO Bp { get; set; }
        protected Main_N3300 Load { get; set; }
        protected Mult_34401A Mult { get; set; }

        #endregion


        protected Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций постоянного тока";

            DataRow = new List<IBasicOperation<decimal>>();
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table8"};
            dataTable.Columns.Add("Величина тока на выходе источника питания, В");
            dataTable.Columns.Add("Измеренное значение пульсаций, мА");
            dataTable.Columns.Add("Допустимое значение пульсаций, мА");
            dataTable.Columns.Add("Результат");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = Bp.CurrMax + " А";
            // ReSharper disable once PossibleNullReferenceException
            dataRow[1] = dds.Getting + " мА";
            dataRow[2] = dds.Error + " мА";
            dataRow[3] = dds.IsGood() ? "Годен" : "Брак";

            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.tolleranceCurrentPuls;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }


        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationToken token)
        {
            var operation = new BasicOperationVerefication<decimal>();
            try
            {
                operation.InitWork = async () =>
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);
                    Mult.StringConnection = GetStringConnect(Mult);

                    Load.Open();
                    Load.FindThisModule();
                    Load.Close();
                    //если модуль нагрузки найти не удалось
                    if (Load.GetChanelNumb <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    void ConfigDeviseAsync()
                    {
                        Load.Open();
                        Load.SetWorkingChanel();
                        Load.SetResistanceFunc();
                        var point = (decimal) 0.9 * Bp.VoltMax / Bp.CurrMax;
                        Load.SetResistanceRange(point);
                        Load.SetResistance(point);
                        Load.OnOutput();
                        Load.Close();

                        //инициализация блока питания
                        Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax);
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();
                    }

                    await Task.Factory.StartNew(ConfigDeviseAsync, token, TaskCreationOptions.AttachedToParent,
                                                TaskScheduler.Current);

                    Mult.Open();
                    while (Mult.GetTerminalConnect())
                        MessageBoxService.Show("На панели прибора " + Mult.GetDeviceType +
                                               " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);

                    MessageBoxService.Show("Установите на В3-57 подходящий предел измерения напряжения",
                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                           MessageResult.OK);
                };

                operation.BodyWork = Test;

                void Test()
                {
                    //нужно дать время В3-57
                    Thread.Sleep(5000);
                    Mult.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
                    Mult.WriteLine(Main_Mult.QueryValue);

                    var currPuls34401 = (decimal) Mult.DataPreparationAndConvert(Mult.ReadString());

                    var currPulsV357 = MathStatistics.Mapping(currPuls34401, 0, (decimal) 0.99, 0, 3);
                    //по закону ома считаем сопротивление
                    var measResist = Bp.GetMeasureVolt() / Bp.GetMeasureCurr();
                    // считаем пульсации
                    currPulsV357 = currPulsV357 / measResist;
                    MathStatistics.Round(ref currPulsV357, 3);

                    Bp.OffOutput();

                    operation.Expected = 0;
                    operation.Getting = currPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;
                    operation.UpperTolerance = operation.Expected + operation.Error;
                    operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                             (operation.Getting >= operation.LowerTolerance);
                    operation.CompliteWork = () => Task.FromResult(operation.IsGood());

                    DataRow.Add(operation);
                }

                await operation.WorkAsync(token);
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Mult.Close();
            }
        }
    }
}