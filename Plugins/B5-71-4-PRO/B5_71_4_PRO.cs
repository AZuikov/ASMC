using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ASMC.Data.Model;
using ASMC.Data.Model.Interface;
using ASMC.Devices.IEEE;
using ASMC.Devices.IEEE.Keysight.ElectronicLoad;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Port.Profigrupp;

// !!!!!!!! Внимание !!!!!!!!!
//  Имя последовательного порта прописано жестко!!!!
// Необходимо реализовать его настройку из ВНЕ - через интрефейс ASMC

namespace B5_71_4_PRO
{
    public class B5_71_4PRO : IProgram
    {
        public B5_71_4PRO()
        {
            AbstraktOperation = new Operation();
            Type = "Б5-71/4-ПРО";
            Grsi = "42467-09";
            Range = "0 - 75 В; 0 - 4 А";
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
        //public IDevice[] Device { get; }
        public IDevice[] TestDevices { get; }
        public IUserItemOperationBase[] UserItemOperation { get; }
        public string[] Accessories { get; }
        public string[] AddresDivece { get; set; }
        public IDevice[] ControlDevices { get; }

        /// <summary>
        /// Операции поверки. Для первичной и периодической одинаковые.
        /// </summary>
        public OpertionFirsVerf()
        {
            //Необходимые эталоны
            TestDevices = new[]
            {
                new UseDevices { Name = new []{"N3300A"},  Description = "Электронная нагрузка"},
                new UseDevices{ Name = new []{"34401A"},  Description = "Мультиметр"},
                new UseDevices{ Name = new []{"В3-57"}, Description = "Микровольтметр"}

            };

            ControlDevices = new IDevice[]{new UseDevices {Name = new[] {"Б5-71/4-ПРО"}, Description = "источник питания" }};

            //Необходимые аксесуары
            Accessories = new[]
            {
                "Нагрузка электронная Keysight N3300A с модулем n3303a",
                "Мультиметр цифровой Agilent/Keysight 34401A",
                "Преобразователь интерфесов National Instruments GPIB-USB",
                "Преобразователь интерфесов USB - RS-232 + нуль-модемный кабель",
                "Кабель banana - banana 6 шт.",
                "Кабель BNC - banan для В3-57"
            };

            //Перечень операций поверки
            UserItemOperation = new IUserItemOperationBase[]{
                new Oper0VisualTest(),
                new Oper1Oprobovanie(),
                new Oper2DcvOutput(),
                new Oper3DcvMeasure(),
                new Oper4VoltUnstable(),
                new Oper6DciOutput(),
                new Oper7DciMeasure(),
                new Oper8DciUnstable(),
                new Oper5VoltPulsation(),
                new Oper9DciPulsation()
            };

        }

        /// <summary>
        /// Проверяет всели эталоны подключены
        /// </summary>
        public void RefreshDevice()
        {
            AddresDivece = new IeeeBase().GetAllDevace().ToArray();
        }
    }


    /// <summary>
    /// Внешний осмотр СИ
    /// </summary>
    public class Oper0VisualTest : AbstractUserItemOperationBase, IUserItemOperation<bool>
    {
        public List<IBasicOperation<bool>> DataRow { get; set; }

        public Oper0VisualTest()
        {
            Name = "Внешний осмотр";
            DataRow = new List<IBasicOperation<bool>>();
        }
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
        private B571Pro4 BP;
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
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
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
            while (m34401.GetTerminalConnect() == false)
            {
                MessageBox.Show("На панели прибора " + m34401.GetDeviceType() + " нажмите клавишу REAR,\nчтобы включить ПЕРЕДНИЙ клеммный терминал.");
            }
            m34401.Close();

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.OffOutput();
            n3303a.Close();
            //-------------------------------------------------


            BP = new B571Pro4(portName);

            //инициализация блока питания
            BP.InitDevice(portName);

            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            foreach (decimal coef in MyPoint)
            {
                decimal setPoint = coef * BP.VoltMax;
                //ставим точку напряжения
                BP.SetStateVolt(setPoint);
                Thread.Sleep(5000);
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
                BufOperation.IsGood = s => { return (BufOperation.Getting < BufOperation.UpperTolerance) & 
                                                    (BufOperation.Getting > BufOperation.LowerTolerance) ? true : false; };
                
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

            return inA;

        }

        /// <summary>
        /// Формирует итоговую таблицу дял режима воспроизведения напряжения
        /// </summary>
        /// <returns>Таблица с результатами измерений</returns>
        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
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


    }

    /// <summary>
    /// Измерение постоянного напряжения 
    /// </summary>
    public class Oper3DcvMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
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
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
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
            while (m34401.GetTerminalConnect() == false)
            {
                MessageBox.Show("На панели прибора " + m34401.GetDeviceType() + " нажмите клавишу REAR,\nчтобы включить ПЕРЕДНИЙ клеммный терминал.");
            }
            m34401.Close();

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.OffOutput();
            n3303a.Close();
            //-------------------------------------------------

            BP = new B571Pro4(portName);

            //инициализация блока питания
            BP.InitDevice(portName);

            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            foreach (decimal coef in MyPoint)
            {
                decimal setPoint = coef * BP.VoltMax;
                //ставим точку напряжения
                BP.SetStateVolt(setPoint);

                //измеряем напряжение
                Thread.Sleep(7000);
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

               //забиваем результаты конкретного измерения для последующей передачи их в протокол
                BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

                BufOperation.Expected = result;
                BufOperation.Getting = resultMeasBp;
                BufOperation.ErrorCalculation = ErrorCalculation;
                BufOperation.LowerTolerance = BufOperation.Expected - BufOperation.Error;
                BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
                BufOperation.IsGood = s => {
                    return (BufOperation.Getting < BufOperation.UpperTolerance) &
                           (BufOperation.Getting > BufOperation.LowerTolerance) ? true : false;
                };
                DataRow.Add(BufOperation);


            }

            BP.OffOutput();
            BP.Close();

        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = BP.tolleranceFormulaVolt(inA);
            AP.Math.MathStatistics.Round(ref inA, 3);

            return inA;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
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


    }

    /// <summary>
    /// Определение нестабильности выходного напряжения
    /// </summary>
    public class Oper4VoltUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
        //это точки для нагрузки в Омах
        public static readonly decimal[] arrResistanceVoltUnstable = { (decimal)20.27, (decimal)37.5, (decimal)187.5 };
        public List<IBasicOperation<decimal>> DataRow { get; set; }


        public Oper4VoltUnstable()
        {
            Name = "Определение нестабильности выходного напряжения";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
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
            while (m34401.GetTerminalConnect() == false)
            {
                MessageBox.Show("На панели прибора " + m34401.GetDeviceType() + " нажмите клавишу REAR,\nчтобы включить ПЕРЕДНИЙ клеммный терминал.");
            }
            m34401.Close();

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.OffOutput();
            n3303a.Close();
            //-------------------------------------------------

           // ------ настроим нагрузку
            n3303a.Connection();
            n3303a.SetWorkingChanel();
            n3303a.SetResistanceFunc();
            //предварительно включаем нагрузку, что бы избежать резкого переходного процесса
            n3303a.SetResistanceRange(200);
            n3303a.SetResistance(200);
            n3303a.OnOutput();
            n3303a.Close();

            BP = new B571Pro4(portName);
            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            


            //сюда запишем результаты
            List<decimal> voltUnstableList = new List<decimal>();
            
            foreach (decimal resistance in arrResistanceVoltUnstable)
            {
                n3303a.Connection();
                n3303a.SetResistanceRange(resistance);
                n3303a.SetResistance(resistance); //ставим сопротивление
                n3303a.Close();
                BP.OnOutput();
                // время выдержки
                Thread.Sleep(3000);
                //измерения
                m34401.Connection();
                m34401.WriteLine(Mult_34401A.DC.Voltage.Range.Auto);
                m34401.WriteLine(Mult_34401A.QueryValue);
                // записываем результаты
                voltUnstableList.Add((decimal)m34401.DataPreparationAndConvert(m34401.ReadString(), Mult_34401A.Multipliers.SI));
                m34401.Close();
                BP.OffOutput();
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
            BufOperation.IsGood = s => {
                return (BufOperation.Getting < BufOperation.UpperTolerance) &
                       (BufOperation.Getting >= BufOperation.LowerTolerance) ? true : false;
            };
            DataRow.Add(BufOperation);


        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return BP.tolleranceVoltageUnstability;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
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




    }

    /// <summary>
    /// Опрделение уровня пульсаций
    /// </summary>
    public class Oper5VoltPulsation : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
        //это точки для нагрузки в Омах
        public static readonly decimal[] arrResistanceVoltUnstable = { (decimal)20.27, (decimal)37.5, (decimal)187.5 };
        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public Oper5VoltPulsation()
        {
            Name = "Определение уровня пульсаций по напряжению";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
            };
            /*
             *Еще одна схема, для переключения терминала мультиметра
             *  C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/34401A_V3-57.jpg
             */
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
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.SetResistanceFunc();
            n3303a.SetResistanceRange(arrResistanceVoltUnstable[0]);
            n3303a.SetResistance(arrResistanceVoltUnstable[0]);
            n3303a.OnOutput();
            n3303a.Close();

            BP = new B571Pro4(portName);
            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            m34401.Connection();

            while (m34401.GetTerminalConnect())
            {
                MessageBox.Show("На панели прибора " + m34401.GetDeviceType() + " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.");
            }

            MessageBox.Show("Установите на В3-57 подходящий предел измерения напряжения");

            //нужно дать время для В3-57 для успокоения стрелки
            Thread.Sleep(7000);
            m34401.WriteLine(Mult_34401A.DC.Voltage.Range.Auto);
            m34401.WriteLine(Mult_34401A.QueryValue);

            decimal voltPulsV357 = (decimal)m34401.DataPreparationAndConvert(m34401.ReadString(), Mult_34401A.Multipliers.SI);
            voltPulsV357 = voltPulsV357 < 0 ? 0 : voltPulsV357;
            voltPulsV357 = AP.Math.MathStatistics.Mapping(voltPulsV357, (decimal)0, (decimal)0.99, (decimal)0, (decimal)3);
            AP.Math.MathStatistics.Round(ref voltPulsV357, 2);

            //выключаем источник питания
            BP.OffOutput();
            BP.Close();

            m34401.Close();


            //забиваем результаты конкретного измерения для последующей передачи их в протокол
            BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

            BufOperation.Expected = 0;
            BufOperation.Getting = voltPulsV357;
            BufOperation.ErrorCalculation = ErrorCalculation;
            BufOperation.LowerTolerance = 0;
            BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
            BufOperation.IsGood = s => {
                return (BufOperation.Getting < BufOperation.UpperTolerance) &
                       (BufOperation.Getting >= BufOperation.LowerTolerance) ? true : false;
            };
            DataRow.Add(BufOperation);


        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return BP.tolleranceVoltPuls;

        }


        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Величина напряжения на выходе источника питания, В");
            dataTable.Columns.Add("Измеренное значение пульсаций, мВ");
            dataTable.Columns.Add("Допустимое значение пульсаций, мВ");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = BP.VoltMax;
            dataRow[1] = dds.Getting;
            dataRow[2] = dds.Error;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }


    }

    /// <summary>
    /// Определение погрешности установки выходного тока
    /// </summary>
    public class Oper6DciOutput : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };
        public static readonly decimal[] MyPointCurr = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };

        public Oper6DciOutput()
        {
            Name = "Определение погрешности установки выходного тока";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            BP = new B571Pro4(portName);

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.SetVoltFunc();
            n3303a.SetVoltLevel((decimal)0.9 * BP.VoltMax);
            n3303a.OnOutput();
            n3303a.Close();
            //-------------------------------------------------

            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            foreach (decimal coef in MyPoint)
            {
                decimal setPoint = coef * BP.CurrMax;
                //ставим точку напряжения
                BP.SetStateCurr(setPoint);
                Thread.Sleep(4000);

                //измеряем ток
                n3303a.Connection();
                var result = n3303a.GetMeasCurr();
                n3303a.Close();
                AP.Math.MathStatistics.Round(ref result, 3);

                //забиваем результаты конкретного измерения для последующей передачи их в протокол
                BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

                BufOperation.Expected = setPoint;
                BufOperation.Getting = (decimal)result;
                BufOperation.ErrorCalculation = ErrorCalculation;
                BufOperation.LowerTolerance = BufOperation.Expected - BufOperation.Error;
                BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
                BufOperation.IsGood = s => {
                    return (BufOperation.Getting < BufOperation.UpperTolerance) &
                           (BufOperation.Getting > BufOperation.LowerTolerance) ? true : false;
                };
                DataRow.Add(BufOperation);

            }

            BP.OffOutput();
            BP.Close();



        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = BP.tolleranceFormulaCurrent(inA);
            AP.Math.MathStatistics.Round(ref inA, 3);

            return inA;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Установленное значение тока, А");
            dataTable.Columns.Add("Измеренное значение, А");
            dataTable.Columns.Add("Абсолютная погрешность, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");

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
    /// Определение погрешности измерения выходного тока
    /// </summary>
    public class Oper7DciMeasure : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };
        public static readonly decimal[] MyPointCurr = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };

        public Oper7DciMeasure()
        {
            Name = "Определение погрешности измерения выходного тока";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            BP = new B571Pro4(portName);

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.SetVoltFunc();
            n3303a.SetVoltLevel((decimal)0.9 * BP.VoltMax);
            n3303a.OnOutput();
            n3303a.Close();

            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            //-------------------------------------------------
            foreach (decimal coef in MyPoint)
            {
                decimal setPoint = coef * BP.CurrMax;
                //ставим точку напряжения
                BP.SetStateCurr(setPoint);

                Thread.Sleep(4000);
                //измеряем ток
                n3303a.Connection();
                var resultn3303a = n3303a.GetMeasCurr();
                n3303a.Close();
                AP.Math.MathStatistics.Round(ref resultn3303a, 3);

                var resultBpCurr = BP.GetMeasureCurr();
                AP.Math.MathStatistics.Round(ref resultBpCurr, 3);

                //забиваем результаты конкретного измерения для последующей передачи их в протокол
                BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

                BufOperation.Expected = resultn3303a;
                BufOperation.Getting = (decimal)resultBpCurr;
                BufOperation.ErrorCalculation = ErrorCalculation;
                BufOperation.LowerTolerance = BufOperation.Expected - BufOperation.Error;
                BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
                BufOperation.IsGood = s => {
                    return (BufOperation.Getting < BufOperation.UpperTolerance) &
                           (BufOperation.Getting > BufOperation.LowerTolerance) ? true : false;
                };
                DataRow.Add(BufOperation);
            }

            BP.OffOutput();
            BP.Close();

        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            inA = BP.tolleranceFormulaCurrent(inA);
            AP.Math.MathStatistics.Round(ref inA, 3);
            return inA;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Измеренное эталонным авмперметром значение тока, А");
            dataTable.Columns.Add("Измеренное блоком питания значение тока, А");
            dataTable.Columns.Add("Абсолютная погрешность, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");

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
    /// Определение нестабильности выходного тока
    /// </summary>
    public class Oper8DciUnstable : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
        public List<IBasicOperation<decimal>> DataRow { get; set; }
        //список точек поверки (процент от максимальных значений блока питания  )
        public static readonly decimal[] MyPoint = { (decimal)0.1, (decimal)0.5, 1 };
        public static readonly decimal[] MyPointCurr = { (decimal)0.1, (decimal)0.5, (decimal)0.9 };
        public static readonly decimal[] arrResistanceCurrUnstable = { (decimal)15, (decimal)8.125, (decimal)1.875 };

        public Oper8DciUnstable()
        {
            Name = "Определение нестабильности выходного тока";
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
            };
            DataRow = new List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            BP = new B571Pro4(portName);

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            List<decimal> currUnstableList = new List<decimal>();


            n3303a.SetWorkingChanel();
            n3303a.SetResistanceFunc();
            n3303a.SetResistanceRange(arrResistanceCurrUnstable[0]);
            n3303a.SetResistance(arrResistanceCurrUnstable[0]);
            n3303a.OnOutput();
            n3303a.Close();

            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            foreach (decimal resistance in arrResistanceCurrUnstable)
            {
                n3303a.Connection();
                n3303a.SetResistanceRange(resistance);
                n3303a.SetResistance(resistance);
                Thread.Sleep(5000);
                currUnstableList.Add(n3303a.GetMeasCurr());
                n3303a.Close();
            }

            BP.OffOutput();
            BP.Close();

            decimal resultCurrUnstable = (currUnstableList.Max() - currUnstableList.Min()) / 2;
            AP.Math.MathStatistics.Round(ref resultCurrUnstable, 3);

            //забиваем результаты конкретного измерения для последующей передачи их в протокол
            BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

            BufOperation.Expected = 0;
            BufOperation.Getting = (decimal)resultCurrUnstable;
            BufOperation.ErrorCalculation = ErrorCalculation;
            BufOperation.LowerTolerance = 0;
            BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
            BufOperation.IsGood = s => {
                return (BufOperation.Getting < BufOperation.UpperTolerance) &
                       (BufOperation.Getting >= BufOperation.LowerTolerance) ? true : false;
            };
            DataRow.Add(BufOperation);



        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {

            return BP.tolleranceCurrentUnstability;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Рассчитанное значение нестабильности (I_МАКС - I_МИН)/2, А");
            dataTable.Columns.Add("Минимальное допустимое значение, А");
            dataTable.Columns.Add("Максимальное допустимое значение, А");

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
    /// Определение уровня пульсаций постоянного тока
    /// </summary>
    public class Oper9DciPulsation : AbstractUserItemOperationBase, IUserItemOperation<decimal>
    {
        //порт нужно спрашивать у интерфейса
        string portName = "com3";
        private B571Pro4 BP;
        //список точек
        public static readonly decimal[] arrResistanceCurrUnstable = { (decimal)15, (decimal)8.125, (decimal)1.875 };
        public List<IBasicOperation<decimal>> DataRow { get; set; }

        public Oper9DciPulsation()
        {
            Name = "Определение уровня пульсаций постоянного тока";
            
            Sheme = new ShemeImage
            {
                Number = 1,
                Path = "C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/B5-71-4-PRO_N3303_34401_v3-57.jpg"
            };
            /*
            *Еще одна схема, для переключения терминала мультиметра
            *  C:/Users/02zaa01/rep/ASMC/Plugins/ShemePicture/34401A_V3-57.jpg
            */
            DataRow = new List<IBasicOperation<decimal>>();
        }

        public override void StartSinglWork(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override void StartWork()
        {
            BP = new B571Pro4(portName);

            //------- Создаем подключение к нагрузке
            N3303A n3303a = new N3303A(2);
            n3303a.Devace();
            n3303a.Connection();
            //массив всех установленных модулей
            string[] InstalledMod = n3303a.GetInstalledModulesName();
            //Берем канал который нам нужен
            string[] currModel = InstalledMod[n3303a.GetChanelNumb() - 1].Split(':');
            if (!currModel[1].Equals(n3303a.GetModuleModel()))
                throw new ArgumentException("Неверно указан номер канала модуля электронной нагрузки.");

            n3303a.SetWorkingChanel();
            n3303a.SetResistanceFunc();
            n3303a.SetResistanceRange(arrResistanceCurrUnstable[0]);
            n3303a.SetResistance(arrResistanceCurrUnstable[0]);
            n3303a.OnOutput();
            n3303a.Close();

            //инициализация блока питания
            BP.InitDevice(portName);
            BP.SetStateCurr(BP.CurrMax);
            BP.SetStateVolt(BP.VoltMax);
            BP.OnOutput();

            //------- Создаем подключение к мультиметру
            Mult_34401A m34401 = new Mult_34401A();
            m34401.Devace();
            m34401.Connection();

            //Начинаем измерять пульсации

            while (m34401.GetTerminalConnect())
            {
                MessageBox.Show("На панели прибора " + m34401.GetDeviceType() + " нажмите клавишу REAR,\nчтобы включить задний клеммный терминал.");
            }

            MessageBox.Show("Установите на В3-57 подходящий предел измерения напряжения");
            //нужно дать время В3-57
            Thread.Sleep(5000);
            m34401.WriteLine(Mult_34401A.DC.Voltage.Range.Auto);
            m34401.WriteLine(Mult_34401A.QueryValue);

            decimal currPuls34401 = (decimal)m34401.DataPreparationAndConvert(m34401.ReadString(), Mult_34401A.Multipliers.SI);

            decimal currPulsV3_57 = AP.Math.MathStatistics.Mapping(currPuls34401, (decimal)0, (decimal)0.99, (decimal)0, (decimal)3);
            //по закону ома считаем сопротивление
            decimal measResist = BP.GetMeasureVolt() / BP.GetMeasureCurr();
            // считаем пульсации
            currPulsV3_57 = currPulsV3_57 / measResist;
            AP.Math.MathStatistics.Round(ref currPulsV3_57, 2);


            m34401.Close();

            BP.OffOutput();
            BP.Close();

            //забиваем результаты конкретного измерения для последующей передачи их в протокол
            BasicOperationVerefication<decimal> BufOperation = new BasicOperationVerefication<decimal>();

            BufOperation.Expected = 0;
            BufOperation.Getting = currPulsV3_57;
            BufOperation.ErrorCalculation = ErrorCalculation;
            BufOperation.LowerTolerance = 0;
            BufOperation.UpperTolerance = BufOperation.Expected + BufOperation.Error;
            BufOperation.IsGood = s => {
                return (BufOperation.Getting < BufOperation.UpperTolerance) &
                       (BufOperation.Getting >= BufOperation.LowerTolerance) ? true : false;
            };
            DataRow.Add(BufOperation);

        }

        private decimal ErrorCalculation(decimal inA, decimal inB)
        {
            return BP.tolleranceCurrentPuls;

        }

        protected override DataTable FillData()
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Величина тока на выходе источника питания, В");
            dataTable.Columns.Add("Измеренное значение пульсаций, мА");
            dataTable.Columns.Add("Допустимое значение пульсаций, мА");

            var dataRow = dataTable.NewRow();
            var dds = DataRow[0] as BasicOperationVerefication<decimal>;
            dataRow[0] = BP.CurrMax;
            dataRow[1] = dds.Getting;
            dataRow[2] = dds.Error;
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }


    }





}
