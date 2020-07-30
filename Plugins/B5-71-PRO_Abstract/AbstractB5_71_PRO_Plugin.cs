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
using NLog;

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
            AddresDivece = IeeeBase.AllStringConnect;
        }
        protected OpertionFirsVerf(ServicePack servicePack) : base(servicePack)
        {
           
        }
    }

    /// <summary>
    /// Внешний осмотр СИ
    /// </summary>
    public abstract class Oper0VisualTest : ParagraphBase, IUserItemOperation<string>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //public override async Task StartWork(CancellationToken token)
        //{
        //    BasicOperation<bool> bo = new BasicOperation<bool>();
        //    bo.Expected = true;
        //    bo.IsGood = s => { return bo.Getting == true ? true : false; };

        //    DataRow.Add(bo);
        //}

        protected override void InitWork()
        {
            var operation = new BasicOperation<string>();

            operation.IsGood = () => operation.Getting.Equals(operation.Expected, StringComparison.CurrentCultureIgnoreCase);
            operation.InitWork = () =>
            {
                var service = this.UserItemOperation.ServicePack.TestingDialog;
                service.Title = "Визуальный осмотр";
                service.Show();
                   return Task.CompletedTask;
                };
                
                operation.CompliteWork = () => Task.FromResult(operation.IsGood());
                DataRow.Add(operation);


        }
        protected Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<string>>();
        }

        #region Methods
        /// <inheritdoc />
        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperation<string>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        #endregion

        public List<IBasicOperation<string>> DataRow { get; set; }
        /// <inheritdoc />
        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if(a != null)
                await a.WorkAsync(token);
        }
        /// <inheritdoc />
        public override async Task StartWork(CancellationToken token)
        {
            InitWork();
            foreach(var dr in DataRow)
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

        #endregion


        public override async Task StartWork(CancellationToken token)
        {
            var bo = new BasicOperation<bool> {Expected = true};
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
            Sheme = new ShemeImage{Description = "Схема", Number = 1, FileName = @"Ошибка связи.JPG", ExtendedDescription = "свободный текст"};
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table2"};
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
            var operation = new BasicOperationVerefication<decimal>();
            
            foreach (var point in MyPoint)
            {
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

                            Mult.Open();
                        });
                      
                        while(!Mult.IsTerminal)
                          this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                                   " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                                   "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                                   MessageResult.OK);
                    }
                    catch(Exception e)
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
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.SetStateCurr(Bp.CurrMax);


                        var setPoint = point * Bp.VoltMax;
                        //ставим точку напряжения
                        Bp.SetStateVolt(setPoint);
                        Bp.OnOutput();
                        Thread.Sleep(1000);

                        //измеряем напряжение
                        Mult.Dc.Voltage.Range.Set(100);
                        var result = Mult.GetMeasValue();
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
                    }
                    finally
                    {
                        Mult.Close();
                        
                        Bp.Close();
                    }
                };
                operation.CompliteWork = () =>  Task.FromResult(operation.IsGood());
                DataRow.Add(DataRow.IndexOf(operation) == -1
                                ? operation
                                : (BasicOperationVerefication<decimal>) operation.Clone());
            
            };

        }

        private decimal ErrorCalculation(decimal inA)
        {
            inA = Bp.TolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

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
    public abstract class Oper3DcvMeasure : ParagraphBase, IUserItemOperation<decimal>
    {
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
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table3"};
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
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    Mult.Open();
                    while (Mult.IsTerminal == false)
                        this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                               " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Mult.Open();
                    
                    Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);

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
                        Mult.Dc.Voltage.Range.Set(100); 
                        var resultMult = Mult.GetMeasValue();
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


                        var a = (BasicOperationVerefication<decimal>) operation.Clone();

                        DataRow.Add(a);
                    }


                    Bp.OffOutput();
                    Load.SetOutputState(MainN3300.State.Off);
                }
            }
            finally
            {
                Mult.Close();
                
                Bp.OffOutput();
                Bp.Close();
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

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public abstract class Oper4VoltUnstable : ParagraphBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах

        public static readonly decimal[] ArrСoefVoltUnstable = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region Property

        //порт нужно спрашивать у интерфейса
        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
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
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    Mult.Open();
                    while (Mult.IsTerminal == false)
                        this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                               " нажмите клавишу REAR,\nчтобы включить передний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                   
                    Load.SetWorkingChanel().SetOutputState(MainN3300.State.Off);
                    Bp.InitDevice();
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.SetStateCurr(Bp.CurrMax);

                    // ------ настроим нагрузку
                    Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);

                    Load.Resistance.SetMaxResistanceRange()
                        .Resistance.Set(Bp.VoltMax / (Bp.CurrMax * ArrСoefVoltUnstable[2]))
                        .SetOutputState(MainN3300.State.On);

                    //сюда запишем результаты
                    var voltUnstableList = new List<decimal>();

                    Bp.OnOutput();

                    foreach (var coef in ArrСoefVoltUnstable)
                    {
                        
                        var resistance = Bp.VoltMax / (coef * Bp.CurrMax);
                        Load.Resistance.SetRange(resistance).Resistance.Set(resistance); //ставим сопротивление

                        // время выдержки
                        Thread.Sleep(1000);
                        // записываем результаты
                        voltUnstableList.Add((decimal)Mult.Dc.Voltage.Range.Set(100).GetMeasValue());
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
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                
                Mult.Close();
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceVoltageUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public abstract class Oper5VoltPulsation : ParagraphBase, IUserItemOperation<decimal>
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
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = async () =>
                {

                    Mult.StringConnection = GetStringConnect(Mult);
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);


                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    await Task.Factory.StartNew(() =>
                    {
                        var point = Bp.VoltMax / ((decimal)0.9 * Bp.CurrMax);
                        Load.SetWorkingChanel()
                            .SetModeWork(MainN3300.ModeWorks.Resistance)
                            .Resistance.SetRange(point)
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
                };

                operation.BodyWork = Test;

                void Test()
                {
                    Thread.Sleep(5000);

                    Mult.Dc.Voltage.Range.Set(100);
                    var voltPulsV357 = (decimal)Mult.GetMeasValue();
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
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Mult.Close();
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceVoltPuls;
        }

        #endregion

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }

        #region Fileds

        protected B571Pro Bp { get; set; }
        protected Mult_34401A Mult { get; set; }
        protected MainN3300 Load { get; set; }
        public List<IBasicOperation<decimal>> DataRow { get; set; }

        #endregion
    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public abstract class Oper6DciOutput : ParagraphBase, IUserItemOperation<decimal>
    {
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
        }

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable {TableName = "table6"};
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
            var operation = new BasicOperationVerefication<decimal>();


            try
            {
                operation.InitWork = () =>
                {
                    Load.StringConnection = GetStringConnect(Load);
                    Bp.StringConnection = GetStringConnect(Bp);

                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {

                    Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                    var resist = Bp.VoltMax / Bp.CurrMax - 3;
                    Load.Resistance.SetRange(resist).Resistance.Set(resist);
                    Load.SetOutputState(MainN3300.State.On);

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
                        
                        var result = Load.Meas.Current;

                        MathStatistics.Round(ref result, 3);

                        operation.Expected = setPoint;
                        operation.Getting = result;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                 (operation.Getting > operation.LowerTolerance);
                        operation.CompliteWork = () => Task.FromResult(operation.IsGood());
                        var a = (BasicOperationVerefication<decimal>) operation.Clone();

                        DataRow.Add(a);
                    }

                    Bp.OffOutput();
                }
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                
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

        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public abstract class Oper7DciMeasure : ParagraphBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};

        #region  Fields

        protected B571Pro Bp;

        #endregion

        #region Property

        protected MainN3300 Load { get; set; }

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
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);


                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {

                    Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                    var resist = Bp.VoltMax / Bp.CurrMax - 3;
                    Load.Resistance.SetRange(resist).Resistance.Set(resist);
                    Load.SetOutputState(MainN3300.State.On);
                    

                    Bp.InitDevice();
                    Bp.SetStateCurr(Bp.CurrMax);
                    Bp.SetStateVolt(Bp.VoltMax);
                    Bp.OnOutput();

                    

                    foreach (var coef in MyPoint)
                    {
                        var setPoint = coef * Bp.CurrMax;
                        //ставим точку напряжения
                        Bp.SetStateCurr(setPoint);
                        Thread.Sleep(1000);
                        //измеряем ток
                        var resultN3300 = Load.Meas.Current;

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
                        var a = (BasicOperationVerefication<decimal>) operation.Clone();

                        DataRow.Add(a);
                    }


                    Bp.OffOutput();
                }
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                
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


        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public abstract class Oper8DciUnstable : ParagraphBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        protected B571Pro Bp;
        protected MainN3300 Load;

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
            var operation = new BasicOperationVerefication<decimal>();

            try
            {
                operation.InitWork = () =>
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);

                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");
                    return Task.CompletedTask;
                };
                operation.BodyWork = Test;

                void Test()
                {
                    Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                    var point = Bp.VoltMax * MyPoint[2] / Bp.CurrMax;
                    Load.Resistance.SetRange(point).Resistance.Set(point);
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
                        Load.Resistance.SetRange(resistance).Resistance.Set(resistance);
                        Thread.Sleep(1000);
                        currUnstableList.Add(Load.Meas.Current);
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
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceCurrentUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }


        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }

    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public abstract class Oper9DciPulsation : ParagraphBase, IUserItemOperation<decimal>
    {
        #region Property

        protected B571Pro Bp { get; set; }
        protected MainN3300 Load { get; set; }
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
            var dataTable = new DataTable {TableName = "table9"};

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
            var operation = new BasicOperationVerefication<decimal>();
            try
            {
                operation.InitWork = async () =>
                {
                    Bp.StringConnection = GetStringConnect(Bp);
                    Load.StringConnection = GetStringConnect(Load);
                    Mult.StringConnection = GetStringConnect(Mult);

                    
                    Load.FindThisModule();
                    
                    //если модуль нагрузки найти не удалось
                    if (Load.ChanelNumber <= 0)
                        throw new
                            ArgumentException($"Модуль нагрузки {Load.GetModuleModel} не установлен в базовый блок нагрузки");

                    void ConfigDeviseAsync()
                    {
                        
                        Load.SetWorkingChanel().SetModeWork(MainN3300.ModeWorks.Resistance);
                        var point = (decimal) 0.9 * Bp.VoltMax / Bp.CurrMax;
                        Load.Resistance.SetRange(point).Resistance.Set(point);
                        Load.SetOutputState(MainN3300.State.On);
                        

                        //инициализация блока питания
                        Bp.InitDevice();
                        Bp.SetStateCurr(Bp.CurrMax);
                        Bp.SetStateVolt(Bp.VoltMax);
                        Bp.OnOutput();
                    }

                    await Task.Factory.StartNew(ConfigDeviseAsync);

                    Mult.Open();
                    while (Mult.IsTerminal)
                        this.UserItemOperation.ServicePack.MessageBox.Show("На панели прибора " + Mult.UserType +
                                               " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.",
                                               "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                               MessageResult.OK);

                    this.UserItemOperation.ServicePack.MessageBox.Show("Установите на В3-57 подходящий предел измерения напряжения",
                                           "Указание оператору", MessageButton.OK, MessageIcon.Information,
                                           MessageResult.OK);
                };

                operation.BodyWork = Test;

                void Test()
                {
                    //нужно дать время В3-57
                    Thread.Sleep(5000);
                    Mult.Dc.Voltage.Range.Set(100);
                    var currPuls34401 = (decimal)Mult.GetMeasValue();
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
            }
            finally
            {
                Bp.OffOutput();
                Bp.Close();
                Mult.Close();
            }
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return Bp.TolleranceCurrentPuls;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }


        public override async Task StartSinglWork(CancellationToken token, Guid guid)
        {
            var a = DataRow.FirstOrDefault(q => Equals(q.Guid, guid));
            if (a != null)
                await a.WorkAsync(token);
        }

        public override async Task StartWork(CancellationToken token)
        {
            foreach (var dr in DataRow) await dr.WorkAsync(token);
        }
    }
}