using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AP.Math;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;
using DevExpress.Mvvm;


//TODO:       Имя последовательного порта прописано жестко!!!!     Необходимо реализовать его настройку из ВНЕ - через интрефейс ASMC
// ReSharper disable once CheckNamespace
namespace B5_71_1_PRO
{
    // ReSharper disable once InconsistentNaming
    public class B5_71_1PRO : IProgram
    {
        public B5_71_1PRO()
        {
            AbstraktOperation = new Operation();
            Type = "Б5-71/1-ПРО";
            Grsi = "42467-09";
            Range = "0 - 30 В; 0 - 10 А";
            Accuracy = "Напряжение ПГ ±(0,002 * U + 0.1); ток ПГ ±(0,01 * I + 0,05)";
        }

        public string Type { get; }
        public string Grsi { get; }
        public string Range { get; }
        public string Accuracy { get; }
        public IMessageBoxService TaskMessageService {
            get { return AbstraktOperation.TaskMessageService; }
            set { AbstraktOperation.TaskMessageService = value; }
        }
        public AbstraktOperation AbstraktOperation { get; }
    }

    public class Operation : AbstraktOperation
    {
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.

        public Operation() 
        {
            //это операция первичной поверки
            UserItemOperationPrimaryVerf = new OpertionFirsVerf();
            //здесь периодическая поверка, но набор операций такой же
            UserItemOperationPeriodicVerf = UserItemOperationPrimaryVerf;
        }
    }


    public class UseDevices : IDevice
    {
        public string Description { get; set; }
        public string[] Name { get; set; }
        public string SelectedName { get; set; }
        public string StringConnect { get; set; }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool? IsConnect { get; }
    }


    public class OpertionFirsVerf : IUserItemOperation
    {
        /// <summary>
        /// Операции поверки. Для первичной и периодической одинаковые.
        /// </summary>
        public OpertionFirsVerf()
        {
            //Необходимые эталоны
            ControlDevices = new IDevice[]
            {
                new UseDevices {Name = new[] {"N3300A"}, Description = "Электронная нагрузка"},
                new UseDevices {Name = new[] {"34401A"}, Description = "Мультиметр"},
                new UseDevices {Name = new[] {"В3-57"}, Description = "Микровольтметр"}
            };

            TestDevices = new IDevice[]{new UseDevices{Name = new []{"Б5-71/1-ПРО"}, Description = "источник питания" } };

            //Необходимые аксесуары
            Accessories = new[]
            {
                "Нагрузка электронная Keysight N3300A с модулем N3306A",
                "Мультиметр цифровой Agilent/Keysight 34401A",
                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Преобразователь интерфесов USB - RS-232 + нуль-модемный кабель",
                "Кабель banana - banana 6 шт.",
                "Кабель BNC - banan для В3-57"
            };

            //Перечень операций поверки
            UserItemOperation = new IUserItemOperationBase[]
            {
                //new Oper0VisualTest(this),
                //new Oper1Oprobovanie(this),
                //new Oper2DcvOutput(this),
                //new Oper3DcvMeasure(this),
                //new Oper4VoltUnstable(this),
                //new Oper6DciOutput(this),
                //new Oper7DciMeasure(this),
                //new Oper8DciUnstable(this),
                new Oper5VoltPulsation(this),
                //new Oper9DciPulsation(this)
            };
        }

        public IDevice[] ControlDevices { get; }
        public IDevice[] TestDevices { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }
        public string[] Accessories { get; }
        public string[] AddresDivece { get; set; }

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
    public class Oper0VisualTest : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
    

        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат внешнего осмотра");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<bool>;
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

        public override async Task StartWork(CancellationTokenSource token)
        {
           
            var bo = new BasicOperation<bool> {Expected = true};
            bo.IsGood = () => bo.Getting;
            bo.InitWork = () =>
            {
                this.MessageBoxService.Show("Начало операции", "Начало операции1", MessageButton.OK, MessageIcon.Information,
                    MessageResult.No);
            };
            bo.CompliteWork = () =>
            {
                this.MessageBoxService.Show("Конец операции", "Конец операции1", MessageButton.OK, MessageIcon.Information,
                    MessageResult.No);
                return true;
            };
            bo.BodyWork = () => { Thread.Sleep(3000); };
            await bo.WorkAsync(token);
            DataRow.Add(bo);
        }

        public Oper0VisualTest(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }
    }

    /// <summary>
    /// Проведение опробования
    /// </summary>
    public class Oper1Oprobovanie : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
      
        #region Methods

        protected override DataTable FillData()
        {
            var data = new DataTable();
            data.Columns.Add("Результат опробования");
            var dataRow = data.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<bool>;
            dataRow[0] = dds.Getting;
            data.Rows.Add(dataRow);
            return data;
        }

        #endregion

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override async Task StartWork(CancellationTokenSource token)
        {
            //var bo = new BasicOperation<bool> { Expected = true };
            //bo.IsGood = () => bo.Getting;

            //DataRow.Add(bo);
            var bo = new BasicOperation<bool> { Expected = true };
            bo.IsGood = () => bo.Getting;
            bo.InitWork = () =>
            {
                this.MessageBoxService.Show("Начало операции", "Начало операции2", MessageButton.OK, MessageIcon.Information,
                    MessageResult.No);
            };
            bo.CompliteWork = () =>
            {
                this.MessageBoxService.Show("Конец операции", "Конец операции2", MessageButton.OK, MessageIcon.Information,
                    MessageResult.No);
                return true;
            };
            bo.BodyWork = () => { Thread.Sleep(3000); };
            await bo.WorkAsync(token);
            DataRow.Add(bo);
        }

      

        public List<IBasicOperation<bool>> DataRow { get; set; }

        public Oper1Oprobovanie(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }
    }


    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public class Oper2DcvOutput : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};
        public static readonly decimal[] MyPointCurr = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion
                
        #region Methods

        /// <summary>
        /// Формирует итоговую таблицу дял режима воспроизведения напряжения
        /// </summary>
        /// <returns>Таблица с результатами измерений</returns>
        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table2";
            dataTable.Columns.Add("Установленное значение напряжения, В");
            dataTable.Columns.Add("Измеренное значение, В");
            dataTable.Columns.Add("Абсолютная погрешность, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                dataRow[0] = dds.Expected;
                dataRow[1] = dds.Getting;
                dataRow[2] = dds.Error;
                dataRow[3] = dds.LowerTolerance;
                dataRow[4] = dds.UpperTolerance;
                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = _bp.tolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            var mult = new Mult_34401A();
            _bp = new B571Pro1();

            var load = new N3306A();
            load.Open();
            load.FindThisModule();
            load.Close();
            //если модуль нагрузки найти не удалось
            if (load.GetChanelNumb()<=0)
                throw new ArgumentException($"Модуль нагрузки {load.GetModuleModel()} не установлен в базовый блок нагрузки");


            

            foreach (var point in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                mult.StringConnection = "GPIB0::22::INSTR";
                load.StringConnection = "GPIB0::23::INSTR";
                //mult.Stringconection = GetStringConnect(mult.GetDeviceType());
                //load.Stringconection = GetStringConnect(load.GetDeviceType());
              
                try
                {
                    operation.InitWork = () =>
                    {
                        MessageBoxService.Show("Нагрузка",
                            $"Воспроизведение напряжения", MessageButton.OK, MessageIcon.Information, MessageResult.OK);
                        /*схема*/
                    };
                    operation.BodyWork = Test;
                    void Test()
                    {
                        mult.Open();
                        load.Open();
                        load.SetWorkingChanel();
                        load.OffOutput();

                        _bp.InitDevice("com3");
                        var setPoint = point *_bp.VoltMax;
                        //ставим точку напряжения
                        _bp.SetStateVolt(setPoint);
                        _bp.SetStateCurr(_bp.CurrMax);
                        _bp.OnOutput();
                        
                        //измеряем напряжение
                       mult.WriteLine(Main_Mult.DC.Voltage.Range.V100);
                        mult.WriteLine(Main_Mult.QueryValue);
                        var result = mult.DataPreparationAndConvert(mult.ReadString());
                       

                        MathStatistics.Round(ref result, 3);

                        var absTol = setPoint - (decimal)result;
                        MathStatistics.Round(ref absTol, 3);

                        var dopusk = _bp.tolleranceFormulaVolt(setPoint);
                        MathStatistics.Round(ref dopusk, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = setPoint;
                        operation.Getting = (decimal)result;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                (operation.Getting > operation.LowerTolerance);
                        operation.CompliteWork = () => operation.IsGood();
                        
                        DataRow.Add(operation);
                        _bp.OffOutput();
                        load.OffOutput();
                    }   
                    await operation.WorkAsync(token);
                 
                }
                finally
                {
                    
                    mult.Close();
                    load.Close();
                    _bp.OffOutput();
                    _bp.Close();

                }

            }

         
         


            #region MyRegion
            ////------- Создаем подключение к мультиметру
            //var m34401 = new Mult_34401A();
            //m34401.Devace();
            //m34401.Open();
            //while(m34401.GetTerminalConnect() == false)
            //    MessageBox.Show("На панели прибора " + m34401.GetDeviceType() +
            //                    " нажмите клавишу REAR,\nчтобы включить ПЕРЕДНИЙ клеммный терминал.");
            //m34401.Close();

            ////------- Создаем подключение к нагрузке
            //var n3306A = new N3306A(1);
            //n3306A.Devace();
            //n3306A.Open();
            ////массив всех установленных модулей
            ////var installedMod = n3306A.GetInstalledModulesName();
            ////Берем канал который нам нужен
            ////var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');

            ////if(!currModel[1].Equals(n3306A.GetModuleModel()))
            ////    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");
               
            //n3306A.SetWorkingChanel();
            //n3306A.OffOutput();
            //n3306A.Close();
            ////-------------------------------------------------


            //_bp = new B571Pro1(_portName);

            ////инициализация блока питания
            //_bp.InitDevice(_portName);
            //_bp.SetStateCurr(_bp.CurrMax);
            //_bp.SetStateVolt(_bp.VoltMax);
            //_bp.OnOutput();

            //foreach(var coef in MyPoint)
            //{
            //    var setPoint = coef * _bp.VoltMax;
            //    //ставим точку напряжения
            //    _bp.SetStateVolt(setPoint);
            //    Thread.Sleep(5000);
            //    //измеряем напряжение
            //    m34401.Open();
            //    m34401.WriteLine(Main_Mult.DC.Voltage.Range.V100);
            //    m34401.WriteLine(Main_Mult.QueryValue);


            //    var result = m34401.DataPreparationAndConvert(m34401.ReadString());
            //    m34401.Close();

            //    MathStatistics.Round(ref result, 3);

            //    var absTol = setPoint - (decimal)result;
            //    MathStatistics.Round(ref absTol, 3);

            //    var dopusk = _bp.tolleranceFormulaVolt(setPoint);
            //    MathStatistics.Round(ref dopusk, 3);

            //    //забиваем результаты конкретного измерения для последующей передачи их в протокол
            //    var bufOperation = new BasicOperationVerefication<decimal>
            //    {
            //        Expected = setPoint,
            //        Getting = (decimal)result,
            //        ErrorCalculation = ErrorCalculation
            //    };

            //    bufOperation.LowerTolerance = bufOperation.Expected - bufOperation.Error;
            //    bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            //    bufOperation.IsGood=() => (bufOperation.Getting < bufOperation.UpperTolerance) &
            //                               (bufOperation.Getting > bufOperation.LowerTolerance);
            //    DataRow.Add(bufOperation);
            //}


            //// -------- закрываем все объекты
            //_bp.OffOutput();
            //_bp.Close();


            #endregion

        }

    

        public Oper2DcvOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            
            Name = "Определение погрешности установки выходного напряжения";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }


    /// <summary>
    /// Измерение постоянного напряжения
    /// </summary>
    public class Oper3DcvMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};
        public static readonly decimal[] MyPointCurr = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion

     

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table3";
            dataTable.Columns.Add("Измеренное эталонным мультиметром значение, В");
            dataTable.Columns.Add("Измеренное источником питания значение, В");
            dataTable.Columns.Add("Абсолютная погрешность, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                dataRow[0] = dds.Expected;
                dataRow[1] = dds.Getting;
                dataRow[2] = dds.Error;
                dataRow[3] = dds.LowerTolerance;
                dataRow[4] = dds.UpperTolerance;
                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = _bp.tolleranceFormulaVolt(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            _bp = new B571Pro1();
            var mult = new Mult_34401A();

            var load = new N3306A();
            load.Open();
            load.FindThisModule();
            load.Close();
            //если модуль нагрузки найти не удалось
            if (load.GetChanelNumb() <= 0)
                throw new ArgumentException($"Модуль нагрузки {load.GetModuleModel()} не установлен в базовый блок нагрузки");


            string GetStringConnect(string nameDevice, IeeeBase devType)
            {
                var connect = this.UserItemOperation.ControlDevices
                    .Where(q => string.Equals(q.SelectedName, devType.GetDeviceType)).Select(q => q.StringConnect)
                    .ToString();
                if (string.IsNullOrEmpty(connect))
                    throw new ArgumentException($@"Строка подключения не указана для {nameDevice}");
                return connect;
            }

            foreach (var point in MyPoint)
            {
                var operation = new BasicOperationVerefication<decimal>();
                mult.StringConnection = "GPIB0::22::INSTR";
                load.StringConnection = "GPIB0::23::INSTR";
                //mult.Stringconection = GetStringConnect(mult.GetDeviceType());
                //load.Stringconection = GetStringConnect(load.GetDeviceType());

                try
                {
                    operation.InitWork = () =>
                    {
                        MessageBoxService.Show("Нагрузка",
                            $"Измерение напряжения", MessageButton.OK, MessageIcon.Information, MessageResult.OK);
                        /*схема*/
                    };
                    operation.BodyWork = Test;
                    void Test()
                    {
                        mult.Open();
                        load.Open();
                        load.SetWorkingChanel();
                        load.OffOutput();

                        _bp.InitDevice("com3");
                        var setPoint = point * _bp.VoltMax;
                        //ставим точку напряжения
                        _bp.SetStateVolt(setPoint);
                        _bp.SetStateCurr(_bp.CurrMax);
                        _bp.OnOutput();

                        //измеряем напряжение
                        mult.WriteLine(Main_Mult.DC.Voltage.Range.V100);
                        mult.WriteLine(Main_Mult.QueryValue);
                        var resultMult = mult.DataPreparationAndConvert(mult.ReadString());
                        var resultBp = _bp.GetMeasureVolt();

                        MathStatistics.Round(ref resultMult, 3);
                        MathStatistics.Round(ref resultBp, 3);

                        //var absTol = setPoint - (decimal)result;
                        //MathStatistics.Round(ref absTol, 3);

                        var dopusk = _bp.tolleranceFormulaVolt(setPoint);
                        MathStatistics.Round(ref dopusk, 3);

                        //забиваем результаты конкретного измерения для последующей передачи их в протокол

                        operation.Expected = (decimal)resultMult;
                        operation.Getting = (decimal)resultBp;
                        operation.ErrorCalculation = ErrorCalculation;
                        operation.LowerTolerance = operation.Expected - operation.Error;
                        operation.UpperTolerance = operation.Expected + operation.Error;
                        operation.IsGood = () => (operation.Getting < operation.UpperTolerance) &
                                                (operation.Getting > operation.LowerTolerance);
                        operation.CompliteWork = () => operation.IsGood();

                        DataRow.Add(operation);
                        _bp.OffOutput();
                        load.OffOutput();
                    }
                    await operation.WorkAsync(token);

                }
                finally
                {

                    mult.Close();
                    load.Close();
                    _bp.OffOutput();
                    _bp.Close();

                }

            }


            #region OldCodeDcvMeasure
            ////------- Создаем подключение к мультиметру
            //var m34401 = new Mult_34401A();
            //m34401.Devace();
            //m34401.Open();
            //while(m34401.GetTerminalConnect() == false)
            //    MessageBox.Show("На панели прибора " + m34401.GetDeviceType() +
            //                    " нажмите клавишу REAR,\nчтобы включить ПЕРЕДНИЙ клеммный терминал.");
            //m34401.Close();

            ////------- Создаем подключение к нагрузке
            //var n3306A = new N3306A(1);
            //n3306A.Devace();
            //n3306A.Open();
            ////массив всех установленных модулей
            //var installedMod = n3306A.GetInstalledModulesName();
            ////Берем канал который нам нужен
            ////var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            ////if(!currModel[1].Equals(n3306A.GetModuleModel()))
            ////    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            //n3306A.SetWorkingChanel();
            //n3306A.OffOutput();
            //n3306A.Close();
            ////-------------------------------------------------

            //_bp = new B571Pro1(_portName);

            ////инициализация блока питания
            //_bp.InitDevice(_portName);

            //_bp.SetStateCurr(_bp.CurrMax);
            //_bp.SetStateVolt(_bp.VoltMax);
            //_bp.OnOutput();

            //foreach(var coef in MyPoint)
            //{
            //    var setPoint = coef * _bp.VoltMax;
            //    //ставим точку напряжения
            //    _bp.SetStateVolt(setPoint);

            //    //измеряем напряжение
            //    Thread.Sleep(7000);
            //    m34401.Open();
            //    m34401.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
            //    m34401.WriteLine(Main_Mult.QueryValue);
            //    var result = (decimal)m34401.DataPreparationAndConvert(m34401.ReadString());
            //    MathStatistics.Round(ref result, 3);
            //    m34401.Close();

            //    var resultMeasBp = _bp.GetMeasureVolt();
            //    MathStatistics.Round(ref resultMeasBp, 2);

            //    var absTol = result - resultMeasBp;
            //    MathStatistics.Round(ref absTol, 3);


            //    //забиваем результаты конкретного измерения для последующей передачи их в протокол
            //    var bufOperation = new BasicOperationVerefication<decimal>
            //    {
            //        Expected = result,
            //        Getting = resultMeasBp,
            //        ErrorCalculation = ErrorCalculation
            //    };

            //    bufOperation.LowerTolerance = bufOperation.Expected - bufOperation.Error;
            //    bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            //    bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
            //                               (bufOperation.Getting > bufOperation.LowerTolerance);
            //    DataRow.Add(bufOperation);
            //}

            //_bp.OffOutput();
            //_bp.Close();

            #endregion


        }

       

        public Oper3DcvMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного напряжения";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class Oper4VoltUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrResistanceVoltUnstable = {(decimal) 3.4, 6, 30};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion


        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table4";
            dataTable.Columns.Add("Рассчитанное значение нестабильности (U_МАКС - U_МИН)/2, В");
            dataTable.Columns.Add("Минимальное допустимое значение, В");
            dataTable.Columns.Add("Максимальное допустимое значение, В");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = dds.Getting;
            dataRow[1] = dds.LowerTolerance;
            dataRow[2] = dds.UpperTolerance;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return _bp.tolleranceVoltageUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            _bp = new B571Pro1();
            var mult = new Mult_34401A();

            var load = new N3306A();
            load.Open();
            load.FindThisModule();
            load.Close();
            //если модуль нагрузки найти не удалось
            if (load.GetChanelNumb() <= 0)
                throw new ArgumentException($"Модуль нагрузки {load.GetModuleModel()} не установлен в базовый блок нагрузки");


            string GetStringConnect(string nameDevice, IeeeBase devType)
            {
                var connect = this.UserItemOperation.ControlDevices
                    .Where(q => string.Equals(q.SelectedName, devType.GetDeviceType)).Select(q => q.StringConnect)
                    .ToString();
                if (string.IsNullOrEmpty(connect))
                    throw new ArgumentException($@"Строка подключения не указана для {nameDevice}");
                return connect;
            }


            var operation = new BasicOperationVerefication<decimal>();
                mult.StringConnection = "GPIB0::22::INSTR";
                load.StringConnection = "GPIB0::23::INSTR";
                //mult.Stringconection = GetStringConnect(mult.GetDeviceType());
                //load.Stringconection = GetStringConnect(load.GetDeviceType());

                try
                {
                    operation.InitWork = () =>
                    {
                        MessageBoxService.Show("Нагрузка",
                            $"Нестабильность по напряжению", MessageButton.OK, MessageIcon.Information, MessageResult.OK);
                        /*схема*/
                    };
                    operation.BodyWork = Test;
                    void Test()
                    {
                        mult.Open();
                        load.Open();
                        load.SetWorkingChanel();
                        load.OffOutput();

                        _bp.InitDevice("com3");
                        _bp.SetStateCurr(_bp.CurrMax);
                       

                        // ------ настроим нагрузку
                        load.SetWorkingChanel();
                        load.SetResistanceFunc();
                        load.OnOutput();
                        load.Close();

                        //сюда запишем результаты
                        var voltUnstableList = new List<decimal>();

                         _bp.OnOutput();

                        foreach (var resistance in ArrResistanceVoltUnstable)
                            {
                        load.Open();
                        load.SetResistanceRange(resistance);
                        load.SetResistance(resistance); //ставим сопротивление
                        load.Close();
                        // время выдержки
                        Thread.Sleep(1000);
                        //измерения
                        mult.Open();
                        mult.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
                        mult.WriteLine(Main_Mult.QueryValue);
                        // записываем результаты
                        voltUnstableList.Add((decimal)mult.DataPreparationAndConvert(mult.ReadString()));
                                mult.Close();
                    }

                    //считаем
                    var resultVoltUnstable = (voltUnstableList.Max() - voltUnstableList.Min()) / 2;
                    MathStatistics.Round(ref resultVoltUnstable, 3);

                    //забиваем результаты конкретного измерения для последующей передачи их в протокол
                    var bufOperation = new BasicOperationVerefication<decimal>
                    {
                        Expected = 0,
                        Getting = resultVoltUnstable,
                        ErrorCalculation = ErrorCalculation,
                        LowerTolerance = 0
                    };

                    bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
                    bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
                                               (bufOperation.Getting >= bufOperation.LowerTolerance);
                    DataRow.Add(bufOperation);


                }
                    await operation.WorkAsync(token);

            }
                finally
                {
                    _bp.OffOutput();
                    _bp.Close();
                    load.Close();
                    mult.Close();

                }
            

            #region OldCodeVoltUnstable
            ////------- Создаем подключение к мультиметру
            //var m34401 = new Mult_34401A();
            //m34401.Devace();
            //m34401.Open();
            //while (m34401.GetTerminalConnect() == false)
            //    MessageBox.Show("На панели прибора " + m34401.GetDeviceType() +
            //                    " нажмите клавишу REAR,\nчтобы включить ПЕРЕДНИЙ клеммный терминал.");
            //m34401.Close();

            ////------- Создаем подключение к нагрузке
            //var n3306A = new N3306A(1);
            //n3306A.Devace();
            //n3306A.Open();
            ////массив всех установленных модулей
            //var installedMod = n3306A.GetInstalledModulesName();
            ////Берем канал который нам нужен
            ////var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            ////if(!currModel[1].Equals(n3306A.GetModuleModel()))
            ////    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            //n3306A.SetWorkingChanel();
            //n3306A.OffOutput();
            //n3306A.Close();
            ////-------------------------------------------------

            //_bp = new B571Pro1(_portName);
            ////инициализация блока питания
            //_bp.InitDevice(_portName);
            //_bp.SetStateCurr(_bp.CurrMax);
            //_bp.SetStateVolt(_bp.VoltMax);
            //_bp.OnOutput();

            //// ------ настроим нагрузку
            //n3306A.Open();
            //n3306A.SetWorkingChanel();
            //n3306A.SetResistanceFunc();
            //n3306A.OnOutput();
            //n3306A.Close();

            ////сюда запишем результаты
            //var voltUnstableList = new List<decimal>();


            //foreach (var resistance in ArrResistanceVoltUnstable)
            //{
            //    n3306A.Open();
            //    n3306A.SetResistanceRange(resistance);
            //    n3306A.SetResistance(resistance); //ставим сопротивление
            //    n3306A.Close();
            //    // время выдержки
            //    Thread.Sleep(7000);
            //    //измерения
            //    m34401.Open();
            //    m34401.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
            //    m34401.WriteLine(Main_Mult.QueryValue);
            //    // записываем результаты
            //    voltUnstableList.Add((decimal)m34401.DataPreparationAndConvert(m34401.ReadString()));
            //    m34401.Close();
            //}

            //_bp.OffOutput();
            //_bp.Close();

            ////считаем нестабильность
            //var resultVoltUnstable = (voltUnstableList.Max() - voltUnstableList.Min()) / 2;
            //MathStatistics.Round(ref resultVoltUnstable, 3);

            ////забиваем результаты конкретного измерения для последующей передачи их в протокол
            //var bufOperation = new BasicOperationVerefication<decimal>
            //{
            //    Expected = 0,
            //    Getting = resultVoltUnstable,
            //    ErrorCalculation = ErrorCalculation,
            //    LowerTolerance = 0
            //};

            //bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            //bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
            //                           (bufOperation.Getting >= bufOperation.LowerTolerance);
            //DataRow.Add(bufOperation);


            #endregion

        }


        public Oper4VoltUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного напряжения";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public class Oper5VoltPulsation : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //это точки для нагрузки в Омах
        public static readonly decimal[] ArrResistanceVoltUnstable = {(decimal) 3.4, 6, 30};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion


        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table5";
            dataTable.Columns.Add("Величина напряжения на выходе источника питания, В");
            dataTable.Columns.Add("Измеренное значение пульсаций, мВ");
            dataTable.Columns.Add("Допустимое значение пульсаций, мВ");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = _bp.VoltMax;
            dataRow[1] = dds.Getting;
            dataRow[2] = dds.Error;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return _bp.tolleranceVoltPuls;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            _bp = new B571Pro1();
            var mult = new Mult_34401A();

            var load = new N3306A();
            load.Open();
            load.FindThisModule();
            load.Close();
            //если модуль нагрузки найти не удалось
            if (load.GetChanelNumb() <= 0)
                throw new ArgumentException($"Модуль нагрузки {load.GetModuleModel()} не установлен в базовый блок нагрузки");


            string GetStringConnect(string nameDevice, IeeeBase devType)
            {
                var connect = this.UserItemOperation.ControlDevices
                    .Where(q => string.Equals(q.SelectedName, devType.GetDeviceType)).Select(q => q.StringConnect)
                    .ToString();
                if (string.IsNullOrEmpty(connect))
                    throw new ArgumentException($@"Строка подключения не указана для {nameDevice}");
                return connect;
            }

            var operation = new BasicOperationVerefication<decimal>();
            mult.StringConnection = "GPIB0::22::INSTR";
            load.StringConnection = "GPIB0::23::INSTR";
            //mult.Stringconection = GetStringConnect(mult.GetDeviceType());
            //load.Stringconection = GetStringConnect(load.GetDeviceType());
            try
            {
                operation.InitWork = () =>
                {
                    MessageBoxService.Show("Нагрузка",
                        $"Измерение пульсаций", MessageButton.OK, MessageIcon.Information, MessageResult.OK);
                    /*схема*/
                };

                operation.BodyWork = Test;

                void Test()
                {
                    mult.Open();
                    load.Open();
                    load.SetWorkingChanel();
                    load.OffOutput();

                    _bp.InitDevice("com3");
                    _bp.SetStateVolt(_bp.VoltMax);
                    _bp.SetStateCurr(_bp.CurrMax);


                    load.SetWorkingChanel();
                    load.SetResistanceFunc();
                    load.SetResistanceRange(ArrResistanceVoltUnstable[0]);
                    load.SetResistance(ArrResistanceVoltUnstable[0]);
                    load.OnOutput();
                    load.Close();

                    while (mult.GetTerminalConnect())
                        MessageBoxService.Show("Указание оператору",
                            "На панели прибора " + mult.GetDeviceType +
                                                " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.", MessageButton.OK, MessageIcon.Information, MessageResult.OK);

                    MessageBoxService.Show("Указание оператору",
                        $"Установите на В3-57 подходящий предел измерения напряжения", MessageButton.OK, MessageIcon.Information, MessageResult.OK);

                    Thread.Sleep(7000);
                    mult.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
                    mult.WriteLine(Main_Mult.QueryValue);

                    var voltPulsV357 = (decimal)mult.DataPreparationAndConvert(mult.ReadString());
                    voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
                    voltPulsV357 = MathStatistics.Mapping(voltPulsV357, 0, (decimal)0.99, 0, 3);
                    MathStatistics.Round(ref voltPulsV357, 2);

                    operation.Expected = 0;
                    operation.Getting = voltPulsV357;
                    operation.ErrorCalculation = ErrorCalculation;
                    operation.LowerTolerance = 0;

                    DataRow.Add(operation);

                }
                await operation.WorkAsync(token);


            }
            finally
            {
                _bp.OffOutput();
                _bp.Close();
                mult.Close();
                load.Close();
            }

            #region OldCodePulsationVolts

            ////------- Создаем подключение к мультиметру
            //var m34401 = new Mult_34401A();
            //m34401.Devace();
            //m34401.Open();
            //m34401.Close();

            ////------- Создаем подключение к нагрузке
            //var n3306A = new N3306A(1);
            //n3306A.Devace();
            //n3306A.Open();
            ////массив всех установленных модулей
            //var installedMod = n3306A.GetInstalledModulesName();
            ////Берем канал который нам нужен
            ////var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            ////if(!currModel[1].Equals(n3306A.GetModuleModel()))
            ////    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            //n3306A.SetWorkingChanel();
            //n3306A.OffOutput();
            //n3306A.Close();
            ////-------------------------------------------------

            //_bp = new B571Pro1(_portName);
            ////инициализация блока питания
            //_bp.InitDevice(_portName);
            //_bp.SetStateCurr(_bp.CurrMax);
            //_bp.SetStateVolt(_bp.VoltMax);
            //_bp.OnOutput();

            //// ------ настроим нагрузку
            //n3306A.Open();
            //n3306A.SetWorkingChanel();
            //n3306A.SetResistanceFunc();
            //n3306A.SetResistanceRange(ArrResistanceVoltUnstable[0]);
            //n3306A.SetResistance(ArrResistanceVoltUnstable[0]);
            //n3306A.OnOutput();
            //n3306A.Close();

            //m34401.Open();

            //while (m34401.GetTerminalConnect())
            //    MessageBox.Show("На панели прибора " + m34401.GetDeviceType() +
            //                    " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.");

            //MessageBox.Show("Установите на В3-57 подходящий предел измерения напряжения");

            ////нужно дать время для В3-57 для успокоения стрелки
            //Thread.Sleep(7000);
            //m34401.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
            //m34401.WriteLine(Main_Mult.QueryValue);

            //var voltPulsV357 = (decimal)m34401.DataPreparationAndConvert(m34401.ReadString());
            //voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
            //voltPulsV357 = MathStatistics.Mapping(voltPulsV357, 0, (decimal)0.99, 0, 3);
            //MathStatistics.Round(ref voltPulsV357, 2);

            ////выключаем источник питания
            //_bp.OffOutput();
            //_bp.Close();

            //m34401.Close();


            ////забиваем результаты конкретного измерения для последующей передачи их в протокол
            //var bufOperation = new BasicOperationVerefication<decimal>
            //{

            //    Expected = 0,
            //    Getting = voltPulsV357,
            //    ErrorCalculation = ErrorCalculation,
            //    LowerTolerance = 0
            //};

            //bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            //bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
            //                           (bufOperation.Getting >= bufOperation.LowerTolerance);
            //DataRow.Add(bufOperation);

            #endregion

        }


        public Oper5VoltPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций по напряжению";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            /*
             *Еще одна схема, для переключения терминала мультиметра
             *  C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/34401A_V3-57.jpg
             */
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }


    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public class Oper6DciOutput : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};
        public static readonly decimal[] MyPointCurr = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion

       

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table6";
            dataTable.Columns.Add("Установленное значение тока, А");
            dataTable.Columns.Add("Измеренное значение, А");
            dataTable.Columns.Add("Абсолютная погрешность, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Expected;
                dataRow[1] = dds.Getting;
                dataRow[2] = dds.Error;
                dataRow[3] = dds.LowerTolerance;
                dataRow[4] = dds.UpperTolerance;
                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = _bp.tolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);

            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {


            #region OldCodeDciOutput
            //_bp = new B571Pro1(_portName);

            ////------- Создаем подключение к нагрузке
            //var n3306A = new N3306A(1);
            //n3306A.Devace();
            //n3306A.Open();
            ////массив всех установленных модулей
            //var installedMod = n3306A.GetInstalledModulesName();
            ////Берем канал который нам нужен
            ////var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            ////if(!currModel[1].Equals(n3306A.GetModuleModel()))
            ////    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            //n3306A.SetWorkingChanel();
            //n3306A.SetVoltFunc();
            //n3306A.SetVoltLevel((decimal)0.9 * _bp.VoltMax);
            //n3306A.OnOutput();
            //n3306A.Close();
            ////-------------------------------------------------

            ////инициализация блока питания
            //_bp.InitDevice(_portName);
            //_bp.SetStateCurr(_bp.CurrMax);
            //_bp.SetStateVolt(_bp.VoltMax);
            //_bp.OnOutput();

            //foreach (var coef in MyPoint)
            //{
            //    var setPoint = coef * _bp.CurrMax;
            //    //ставим точку напряжения
            //    _bp.SetStateCurr(setPoint);
            //    Thread.Sleep(2000);

            //    //измеряем ток
            //    n3306A.Open();
            //    var result = n3306A.GetMeasCurr();
            //    n3306A.Close();
            //    MathStatistics.Round(ref result, 3);

            //    //забиваем результаты конкретного измерения для последующей передачи их в протокол
            //    var bufOperation = new BasicOperationVerefication<decimal>();

            //    bufOperation.Expected = setPoint;
            //    bufOperation.Getting = result;
            //    bufOperation.ErrorCalculation = ErrorCalculation;
            //    bufOperation.LowerTolerance = bufOperation.Expected - bufOperation.Error;
            //    bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            //    bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
            //                               (bufOperation.Getting > bufOperation.LowerTolerance);
            //    DataRow.Add(bufOperation);
            //}

            //_bp.OffOutput();
            //_bp.Close();


            #endregion


        }

   
        public Oper6DciOutput(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности установки выходного тока";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }


    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public class Oper7DciMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};
        public static readonly decimal[] MyPointCurr = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion

    

        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table7";
            dataTable.Columns.Add("Измеренное эталонным авмперметром значение тока, А");
            dataTable.Columns.Add("Измеренное блоком питания значение тока, А");
            dataTable.Columns.Add("Абсолютная погрешность, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");

            foreach (var row in DataRow)
            {
                var dataRow = dataTable.NewRow();
                var dds = row as BasicOperationVerefication<decimal>;
                // ReSharper disable once PossibleNullReferenceException
                dataRow[0] = dds.Expected;
                dataRow[1] = dds.Getting;
                dataRow[2] = dds.Error;
                dataRow[3] = dds.LowerTolerance;
                dataRow[4] = dds.UpperTolerance;
                dataTable.Rows.Add(dataRow);
            }


            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = _bp.tolleranceFormulaCurrent(inA);
            MathStatistics.Round(ref inA, 3);
            return inA;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            _bp = new B571Pro1(_portName);

            //------- Создаем подключение к нагрузке
            var n3306A = new N3306A(1);
            n3306A.Devace();
            n3306A.Open();
            //массив всех установленных модулей
            var installedMod = n3306A.GetInstalledModulesName();
            //Берем канал который нам нужен
            //var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            //if(!currModel[1].Equals(n3306A.GetModuleModel()))
            //    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3306A.SetWorkingChanel();
            n3306A.SetVoltFunc();
            n3306A.SetVoltLevel((decimal)0.9 * _bp.VoltMax);
            n3306A.OnOutput();
            n3306A.Close();

            //инициализация блока питания
            _bp.InitDevice(_portName);
            _bp.SetStateCurr(_bp.CurrMax);
            _bp.SetStateVolt(_bp.VoltMax);
            _bp.OnOutput();

            //-------------------------------------------------
            foreach(var coef in MyPoint)
            {
                var setPoint = coef * _bp.CurrMax;
                //ставим точку напряжения
                _bp.SetStateCurr(setPoint);

                //измеряем ток
                n3306A.Open();
                Thread.Sleep(2000);
                var resultN3306A = n3306A.GetMeasCurr();
                n3306A.Close();
                MathStatistics.Round(ref resultN3306A, 3);

                var resultBpCurr = _bp.GetMeasureCurr();
                MathStatistics.Round(ref resultBpCurr, 3);

                //забиваем результаты конкретного измерения для последующей передачи их в протокол
                var bufOperation = new BasicOperationVerefication<decimal>();

                bufOperation.Expected = resultN3306A;
                bufOperation.Getting = resultBpCurr;
                bufOperation.ErrorCalculation = ErrorCalculation;
                bufOperation.LowerTolerance = bufOperation.Expected - bufOperation.Error;
                bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
                bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
                                           (bufOperation.Getting > bufOperation.LowerTolerance);
                DataRow.Add(bufOperation);
            }

            _bp.OffOutput();
            _bp.Close();
        }


        public Oper7DciMeasure(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение погрешности измерения выходного тока";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }


    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public class Oper8DciUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        public static readonly decimal[] ArrResistanceCurrUnstable = {(decimal) 2.7, (decimal) 1.5, (decimal) 0.3};

        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = {(decimal) 0.1, (decimal) 0.5, 1};
        public static readonly decimal[] MyPointCurr = {(decimal) 0.1, (decimal) 0.5, (decimal) 0.9};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion


        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table8";
            dataTable.Columns.Add("Рассчитанное значение нестабильности (I_МАКС - I_МИН)/2, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[0] = dds.Getting;
            dataRow[1] = dds.LowerTolerance;
            dataRow[2] = dds.UpperTolerance;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return _bp.tolleranceCurrentUnstability;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            _bp = new B571Pro1(_portName);

            //------- Создаем подключение к нагрузке
            var n3306A = new N3306A(1);
            n3306A.Devace();
            n3306A.Open();
            //массив всех установленных модулей
            var installedMod = n3306A.GetInstalledModulesName();
            //Берем канал который нам нужен
            //var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            //if(!currModel[1].Equals(n3306A.GetModuleModel()))
                //throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            var currUnstableList = new List<decimal>();


            n3306A.SetWorkingChanel();
            n3306A.SetResistanceFunc();
            n3306A.SetResistanceRange(ArrResistanceCurrUnstable[0]);
            n3306A.SetResistance(ArrResistanceCurrUnstable[0]);
            n3306A.OnOutput();
            n3306A.Close();

            //инициализация блока питания
            _bp.InitDevice(_portName);
            _bp.SetStateCurr(_bp.CurrMax);
            _bp.SetStateVolt(_bp.VoltMax);
            _bp.OnOutput();

            foreach(var resistance in ArrResistanceCurrUnstable)
            {
                n3306A.Open();
                n3306A.SetResistanceRange(resistance);
                n3306A.SetResistance(resistance);
                Thread.Sleep(2000);
                currUnstableList.Add(n3306A.GetMeasCurr());
                n3306A.Close();
            }

            _bp.OffOutput();
            _bp.Close();

            var resultCurrUnstable = (currUnstableList.Max() - currUnstableList.Min()) / 2;
            MathStatistics.Round(ref resultCurrUnstable, 3);

            //забиваем результаты конкретного измерения для последующей передачи их в протокол
            var bufOperation = new BasicOperationVerefication<decimal>();

            bufOperation.Expected = 0;
            bufOperation.Getting = resultCurrUnstable;
            bufOperation.ErrorCalculation = ErrorCalculation;
            bufOperation.LowerTolerance = 0;
            bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
                                       (bufOperation.Getting >= bufOperation.LowerTolerance);
            DataRow.Add(bufOperation);
        }

     
        public Oper8DciUnstable(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение нестабильности выходного тока";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }


    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public class Oper9DciPulsation : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //список точек
        public static readonly decimal[] ArrResistanceCurrUnstable = {(decimal) 2.7, (decimal) 1.5, (decimal) 0.3};

        #region  Fields

        private B571Pro1 _bp;

        //порт нужно спрашивать у интерфейса
        private readonly string _portName = "com3";

        #endregion

      
        #region Methods

        protected override DataTable FillData()
        {
            var dataTable = new DataTable();
            dataTable.TableName = "table8";
            dataTable.Columns.Add("Величина тока на выходе источника питания, В");
            dataTable.Columns.Add("Измеренное значение пульсаций, мА");
            dataTable.Columns.Add("Допустимое значение пульсаций, мА");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = _bp.CurrMax;
            // ReSharper disable once PossibleNullReferenceException
            dataRow[1] = dds.Getting;
            dataRow[2] = dds.Error;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return _bp.tolleranceCurrentPuls;
        }

        #endregion

        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public async override Task StartWork(CancellationTokenSource token)
        {
            _bp = new B571Pro1(_portName);

            //------- Создаем подключение к нагрузке
            var n3306A = new N3306A(1);
            n3306A.Devace();
            n3306A.Open();
            //массив всех установленных модулей
            var installedMod = n3306A.GetInstalledModulesName();
            //Берем канал который нам нужен
            //var currModel = installedMod[n3306A.GetChanelNumb() - 1].Split(':');
            //if(!currModel[1].Equals(n3306A.GetModuleModel()))
            //    throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3306A.SetWorkingChanel();
            n3306A.SetResistanceFunc();
            n3306A.SetResistanceRange(ArrResistanceCurrUnstable[0]);
            n3306A.SetResistance(ArrResistanceCurrUnstable[0]);
            n3306A.OnOutput();
            n3306A.Close();

            //инициализация блока питания
            _bp.InitDevice(_portName);
            _bp.SetStateCurr(_bp.CurrMax);
            _bp.SetStateVolt(_bp.VoltMax);
            _bp.OnOutput();

            //------- Создаем подключение к мультиметру
            var m34401 = new Mult_34401A();
            m34401.Devace();
            m34401.Open();

            //Начинаем измерять пульсации

            while(m34401.GetTerminalConnect())
                MessageBox.Show("На панели прибора " + m34401.GetDeviceType +
                                " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.");

            MessageBox.Show("Установите на В3-57 подходящий предел измерения напряжения");
            //нужно дать время В3-57
            Thread.Sleep(5000);
            m34401.WriteLine(Main_Mult.DC.Voltage.Range.Auto);
            m34401.WriteLine(Main_Mult.QueryValue);

            var currPuls34401 = (decimal)m34401.DataPreparationAndConvert(m34401.ReadString());

            var currPulsV357 = MathStatistics.Mapping(currPuls34401, 0, (decimal)0.99, 0, 3);
            //по закону ома считаем сопротивление
            var measResist = _bp.GetMeasureVolt() / _bp.GetMeasureCurr();
            // считаем пульсации
            currPulsV357 = currPulsV357 / measResist;
            MathStatistics.Round(ref currPulsV357, 2);


            m34401.Close();


            _bp.OffOutput();
            _bp.Close();

            //забиваем результаты конкретного измерения для последующей передачи их в протокол
            var bufOperation = new BasicOperationVerefication<decimal>
            {
                Expected = 0,
                Getting = currPulsV357,
                ErrorCalculation = ErrorCalculation,
                LowerTolerance = 0
            };

            bufOperation.UpperTolerance = bufOperation.Expected + bufOperation.Error;
            bufOperation.IsGood = () => (bufOperation.Getting < bufOperation.UpperTolerance) &
                                       (bufOperation.Getting >= bufOperation.LowerTolerance);
            DataRow.Add(bufOperation);
        }


        public Oper9DciPulsation(IUserItemOperation userItemOperation) : base(userItemOperation)
        {
            Name = "Определение уровня пульсаций постоянного тока";

            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            /*
            *Еще одна схема, для переключения терминала мультиметра
            *  C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/34401A_V3-57.jpg
            */
            DataRow = new List<IBasicOperation<decimal>>();
        }
    }
}