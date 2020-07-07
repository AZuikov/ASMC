using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;

// !!!!!!!! Внимание !!!!!!!!!
//  Имя последовательного порта прописано жестко!!!!
// Необходимо реализовать его настройку из ВНЕ - через интрефейс ASMC


namespace B5_71_PRO
{
    public class B5_71_1PRO : IProrgam
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
        public AbstraktOperation AbstraktOperation { get; }
    }

    public class Operation : AbstraktOperation
    {
      
        //определяет какие типы проверок доступны для СИ: поверка первичная/переодическая, калибровка, adjustment.
        public Operation()
        {
            //это операция первичной поверки
            this.UserItemOperationPrimaryVerf = new OpertionFirsVerf();
            //здесь периодическая поверка, но набор операций такой же
            this.UserItemOperationPeriodicVerf = this.UserItemOperationPrimaryVerf;
            
        }
    }



    public class StandartDevices : IDevice
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
        public IDevice[] Device { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }
        public string[] Accessories { get; }
        public string[] AddresDivece { get; set; }

        /// <summary>
        /// Операции поверки. Для первичной и периодической одинаковые.
        /// </summary>
        public OpertionFirsVerf()
        {
            //Необходимые эталоны
            Device = new []
            {
                new StandartDevices { Name = new []{"N3300A"},  Description = "Электронная нагрузка"},
                new StandartDevices{ Name = new []{"344010A"},  Description = "Мультиметр"}, 
                new StandartDevices{ Name = new []{"В3-57"}, Description = "Микровольтметр"} 

            };

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
            UserItemOperation = new IUserItemOperationBase[]{new Oper1Oprobovanie(),
                new Oper2DcvOutput(),
                new Oper3DcvMeasure(),
                new Oper4VoltUnstable(),
                new Oper5VoltPulsation(), 
                new Oper6DciOutput(), 
                new Oper7DciMeasure(), 
                new Oper8DciUnstable(), 
                new Oper9DciPuls()
            };

        }

        /// <summary>
        /// Проверяет всели эталоны подключены
        /// </summary>
      public void RefreshDevice()
        {
            foreach (var dev in Device)
            {

            }
        }   
    }
    


    /// <summary>
    /// Проведение опробования
    /// </summary>
    public class Oper1Oprobovanie : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
        public Oper1Oprobovanie()
        {
            Name = "Опробование";
            DataRow = new List<IBasicOperation<bool>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            BasicOperation<bool> bo = new BasicOperation<bool>();
            bo.Expected = true;
            bo.IsGood = s => { return bo.Getting == true ? true : false; }; 

            DataRow.Add(bo);
        }

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

        public List<IBasicOperation<bool>> DataRow { get; set; }
    }


    /// <summary>
    /// Воспроизведение постоянного напряжения
    /// </summary>
    public class Oper2DcvOutput : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro1 BP;
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };
        public static readonly decimal[] MyPointCurr = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };

        public Oper2DcvOutput()
        {
            this.Name = "Определение погрешности установки выходного напряжения";
            Sheme = new ShemeImage
            {
            Number = 1,
            Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new  List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            //------- Создаем подключение к мультиметру
            Mult_34401A m34401 = new Mult_34401A();
            m34401.Devace();
            m34401.Connection();
            m34401.Close();
            
            //------- Создаем подключение к нагрузке
            N3306A n3306a = new N3306A(1);
            n3306a.Devace();
            n3306a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3306a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3306a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3306a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3306a.SetWorkingChanel();
            n3306a.OffOutput();
            n3306a.Close();
            //-------------------------------------------------
            


            
            BP = new B571Pro1(portName);

            //инициализация блока питания
            BP.InitDevice(portName);

            BP.SetStateCurr(10);
            BP.SetStateVolt(30);
            BP.OnOutput();
           
              foreach (decimal coef in MyPoint)
                {
                    decimal setPoint = coef * BP.VoltMax;
                    //ставим точку напряжения
                    BP.SetStateVolt(setPoint);
    
                    //измеряем напряжение
                    m34401.Connection();
                    m34401.WriteLine(Mult_34401A.DC.Voltage.Range.V100);
                    m34401.WriteLine(Mult_34401A.QueryValue);
    
    
                    var result = m34401.DataPreparationAndConvert(m34401.ReadString(), Mult_34401A.Multipliers.SI);
                    m34401.Close();
    
                    AP.Math.MathStatistics.Round(ref result, 3);
    
                    var absTol = setPoint - (decimal)result;
                    AP.Math.MathStatistics.Round(ref absTol, 3);
    
                    var dopusk = BP.tolleranceFormulaVolt(setPoint);
                    AP.Math.MathStatistics.Round(ref dopusk, 3);
    
                    //забиваем результаты конкретного измерения для последующей передачи их в протокол
                    BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();
    
                    BufOperation.Expected = setPoint;
                    BufOperation.Getting = (decimal)result;
                    BufOperation.ErrorCalculation = ErrorCalculation;
                    BufOperation.LowerTolerance = BufOperation.Expected - BufOperation.Error;
                    BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
                    DataRow.Add(BufOperation);
                }
                
            
            // -------- закрываем все объекты
           BP.OffOutput();
           BP.Close();

           
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = BP.tolleranceFormulaVolt(inA);
            AP.Math.MathStatistics.Round(ref inA, 3);

            return BP.tolleranceFormulaVolt(inA);

        }

        /// <summary>
        /// Формирует итоговую таблицу дял режима воспроизведения напряжения
        /// </summary>
        /// <returns>Таблица с результатами измерений</returns>
        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Установленное значение напряжения");
            dataTable.Columns.Add("Измеренное значение");
            dataTable.Columns.Add("Абсолютная погрешность");
            dataTable.Columns.Add("Минимальное допустимое значение");
            dataTable.Columns.Add("Максимальное допустимое значение");

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

        
    }


    /// <summary>
    /// Измерение постоянного напряжения 
    /// </summary>
    public class Oper3DcvMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro1 BP;
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };
        public static readonly decimal[] MyPointCurr = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };

        public Oper3DcvMeasure()
        {
            Name = "Определение погрешности измерения выходного напряжения";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            //------- Создаем подключение к мультиметру
            Mult_34401A m34401 = new Mult_34401A();
            m34401.Devace();
            m34401.Connection();
            m34401.Close();

            //------- Создаем подключение к нагрузке
            N3306A n3306a = new N3306A(1);
            n3306a.Devace();
            n3306a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3306a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3306a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3306a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3306a.SetWorkingChanel();
            n3306a.OffOutput();
            n3306a.Close();
            //-------------------------------------------------

            BP = new B571Pro1(portName);

            //инициализация блока питания
            BP.InitDevice(portName);

            BP.SetStateCurr(10);
            BP.SetStateVolt(30);
            BP.OnOutput();

            foreach (decimal coef in MyPoint)
            {
                decimal setPoint = coef * BP.VoltMax;
                //ставим точку напряжения
                BP.SetStateVolt(setPoint);

                //измеряем напряжение

                m34401.Connection();
                m34401.WriteLine(Mult_34401A.DC.Voltage.Range.Auto);
                m34401.WriteLine(Mult_34401A.QueryValue);
                var result = (decimal)m34401.DataPreparationAndConvert(m34401.ReadString(), Mult_34401A.Multipliers.SI);
                AP.Math.MathStatistics.Round(ref result, 3);
                m34401.Close();

                var resultMeasBp = BP.GetMeasureVolt();
                AP.Math.MathStatistics.Round(ref resultMeasBp, 2);

                var absTol = result - (decimal)resultMeasBp;
                AP.Math.MathStatistics.Round(ref absTol, 3);

                var dopusk = BP.tolleranceFormulaVolt(result);
                AP.Math.MathStatistics.Round(ref dopusk, 3);

                //забиваем результаты конкретного измерения для последующей передачи их в протокол
                BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

                BufOperation.Expected = result;
                BufOperation.Getting = resultMeasBp;
                BufOperation.ErrorCalculation = ErrorCalculation;
                BufOperation.LowerTolerance = BufOperation.Expected - BufOperation.Error;
                BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
                DataRow.Add(BufOperation);


            }

            BP.OffOutput();
            BP.Close();
            
        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = BP.tolleranceFormulaVolt(inA);
            AP.Math.MathStatistics.Round(ref inA, 3);

            return BP.tolleranceFormulaVolt(inA);

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Измеренное эталонным мультиметром значение");
            dataTable.Columns.Add("Измеренное источником питания значение");
            dataTable.Columns.Add("Абсолютная погрешность");
            dataTable.Columns.Add("Минимальное допустимое значение");
            dataTable.Columns.Add("Максимальное допустимое значение");

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

        
    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class Oper4VoltUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro1 BP;
        public List<IBasicOperation<decimal>> DataRow { get; set; }


        public Oper4VoltUnstable()
        {
            Name = "Определение нестабильности выходного напряжения";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-1_2-PRO_N3306_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            //------- Создаем подключение к мультиметру
            Mult_34401A m34401 = new Mult_34401A();
            m34401.Devace();
            m34401.Connection();
            m34401.Close();

            //------- Создаем подключение к нагрузке
            N3306A n3306a = new N3306A(1);
            n3306a.Devace();
            n3306a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3306a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3306a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3306a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3306a.SetWorkingChanel();
            n3306a.OffOutput();
            n3306a.Close();
            //-------------------------------------------------

            BP = new B571Pro1(portName);
            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(10);
            BP.SetStateVolt(30);
            BP.OnOutput();
            
            // ------ настроим нагрузку
            n3306a.Connection();
            n3306a.SetWorkingChanel();
            n3306a.SetResistanceFunc();
            n3306a.OnOutput();
            n3306a.Close();
            
            //сюда запишем результаты
            List<decimal> voltUnstableList = new List<decimal>();
            //это точки для нагрузки в Омах
            decimal[] arrResistanceVoltUnstable = { (decimal)3.4, 6, 30 };

            foreach (decimal resistance in arrResistanceVoltUnstable)
            {
                n3306a.Connection();
                n3306a.SetResistance(resistance); //ставим сопротивление
                n3306a.Close();
                // время выдержки
                Thread.Sleep(2000);
                //измерения
                m34401.Connection();
                m34401.WriteLine(Mult_34401A.DC.Voltage.Range.Auto);
                m34401.WriteLine(Mult_34401A.QueryValue);
                // записываем результаты
                voltUnstableList.Add((decimal)m34401.DataPreparationAndConvert(m34401.ReadString(), Mult_34401A.Multipliers.SI));
                m34401.Close();
            }

            BP.OffOutput();
            BP.Close();

            //считаем нестабильность
            decimal resultVoltUnstable = (voltUnstableList.Max() - voltUnstableList.Min()) / 2;
            AP.Math.MathStatistics.Round(ref resultVoltUnstable, 3);

            //забиваем результаты конкретного измерения для последующей передачи их в протокол
            BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

            BufOperation.Expected = 0;
            BufOperation.Getting = resultVoltUnstable;
            BufOperation.ErrorCalculation = ErrorCalculation;
            BufOperation.LowerTolerance = 0;
            BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
            DataRow.Add(BufOperation);


        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return BP.tolleranceVoltageUnstability;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Рассчитанное значение нестабильности (U_МАКС - U_МИН)/2");
            dataTable.Columns.Add("Минимальное допустимое значение");
            dataTable.Columns.Add("Максимальное допустимое значение");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = dds.Getting;
            dataRow[1] = dds.LowerTolerance;
            dataRow[2] = dds.UpperTolerance;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        

       
    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public class Oper5VoltPulsation : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public Oper5VoltPulsation()
        {
            Name = "Определение уровня пульсаций по напряжению";
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<string>> DataRow { get; set; }
    }



    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public class Oper6DciOutput : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public Oper6DciOutput()
        {
            Name = "Определение погрешности установки выходного тока";
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<string>> DataRow { get; set; }
    }


    /// <summary>
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public class Oper7DciMeasure : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public Oper7DciMeasure()
        {
            Name = "Определение погрешности измерения выходного тока";
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<string>> DataRow { get; set; }
    }


    /// <summary>
    /// Определение нестабильности выходного тока
    /// </summary>
    public class Oper8DciUnstable : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public Oper8DciUnstable()
        {
            Name = "Определение нестабильности выходного тока";
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<string>> DataRow { get; set; }
    }



    /// <summary>
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public class Oper9DciPuls : AbstractUserItemOperationBase, IUserItemOperation<string>
    {
        public Oper9DciPuls()
        {
            Name = "Определение уровня пульсаций постоянного тока";
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillData()
        {
            throw new NotImplementedException();
        }

        public List<IBasicOperation<string>> DataRow { get; set; }
    }

}


