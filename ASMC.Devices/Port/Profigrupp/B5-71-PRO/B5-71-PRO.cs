using System;
using System.IO.Ports;
using System.Threading;
using AP.Reports.Utils;
//using System.Globalization;
//using System.Text.RegularExpressions;


namespace ASMC.Devices.Port.Profigrupp
{

    public enum bpRemoteState
    {
    [StringValue("cont_ps_ext")] OnlyPcMode, //Работа тольк от ПК с блокировкой кнопок cont_ps_ext
        [StringValue("cps_int_ext")] PcAndFrontPanel, //Управление от ПК и с клавиатуры прибора cps_int_ext
        [StringValue("cont_ps_int")] OnlyFrontPanel //Выключает управление от ПК, только управление с клавиатуры прибора cont_ps_int 
    }

public enum CurrMultipliers
    {
        [StringValue(" A")]
        Amp,
        [StringValue(" mA")]
        Milli,
        [StringValue(" microA")]
        Micro,
        [StringValue("")]
        Si
    }


    public abstract class B571Pro : ComPort

    {

        /// <summary>
        /// Максимальное значение напряжения источника питания
        /// </summary>
        public decimal VoltMax { get; protected set; }

        /// <summary>
        /// Максимальное значение тока источника питания
        /// </summary>
        public decimal CurrMax { get; protected set; }

        /// <summary>
        /// Погрешность нестабильности выходного напряжения.
        /// </summary>
        private decimal tolVoltUnstable;
        public decimal TolleranceVoltageUnstability
        {
            get { return tolVoltUnstable;}
            protected set
            {
                tolVoltUnstable = TollUnstable(VoltMax, (decimal)0.02); 
            }
        }

        /// <summary>
        /// Погрешность нетсабильности выходного тока.
        /// </summary>

        private decimal tolCurrUnstable;
        public decimal TolleranceCurrentUnstability
        {
            get { return tolCurrUnstable;}
            protected set
            {
                tolCurrUnstable = TollUnstable(CurrMax, (decimal)0.05); 
            }
        }

        /// <summary>
        /// формулы расчета погрешности нестабильности тока/напряжения
        /// </summary>
        /// <param name="bpInArg">Максимальный ток/напряжение для данной модели блока питания</param>
        /// <param name="bpTolArg">Коэффициент из формулы расчета, различный для тока и напряжения</param>
        /// <returns></returns>
        private decimal TollUnstable(decimal bpInArg, decimal bpTolArg)
        {
            return (decimal) 0.001 * bpInArg + bpTolArg;
        }

        /// <summary>
        /// Допустимый уровень пульсаций по напряжению
        /// </summary>

        public decimal TolleranceVoltPuls { get; protected set; } = 2;


        /// <summary>
        /// Допустимый уровень пульсаций по току
        /// </summary>
        public decimal TolleranceCurrentPuls { get; protected set; } = 5;

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению напряжения источника питания.
        /// </summary>
        /// <param name="value">Поверяемая точка. В вольтах</param>
        /// <param name = "mult"></param>
        /// <returns>Погрешность в вольтах.</returns>
        public decimal TolleranceFormulaVolt(decimal value, Multipliers mult = Multipliers.None)
        {

            value *= (decimal)mult.GetDoubleValue();
            return (decimal)0.002 * value + (decimal)0.1;
        }

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению тока источника питания.
        /// </summary>
        /// <returns>Погрешность в амперах</returns>
        public decimal TolleranceFormulaCurrent(decimal value, Multipliers mult = Multipliers.None)
        {
            value *= (decimal)mult.GetDoubleValue();
            return (decimal)0.01 * value + (decimal)0.05;
        }



        /// <summary>
        /// Производится подключение к блоку питания. Переводит прибор в режим дистанционного управления.
        /// </summary>
        /// <returns>Если при подключении к блоку питания получен верный ответ функция вернет true. В противном случае вернет false.</returns>
        public bool InitDevice()
        {
            Init();

            //перезагружаем блок питания
            this.Reset();

            if (!this.Open())
            {
                this.Close();
                return false;
            }


            //Переводим в режим дистанционного управления. При этом работают клавиши на панели прибора.
            this.WriteLine("cps_int_ext");
            Thread.Sleep(500);
            //В ответ на команду перевода врежим дистанционного управления прибор должен прислать сообщение "EIC"
            string answer = this.ReadLine();
            Thread.Sleep(500);

            this.Close();

            if (String.Equals(answer, "EIC"))
                return true;
            else
                return false;

        }

        /// <summary>
        /// Возвращает массив из трех строк. !!! Первый элемент пустой!!! . 
        /// Второй элемент это значение тока в мА. 
        /// Третий элемент это значение напряжение в мВ.
        /// </summary>
        /// <returns></returns>
        private string[] GetMeasVal()
        {
           
            //если порт не открыт нужно бросить исключение
            if (!this.Open()) return null;

            this.Write("M");
            string answer = this.ReadLine();
            var strArr = answer.Split('M');

            this.Close();
            return strArr;
        }

        /// <summary>
        /// Вовзращает измеренное источником питания значения тока в цепи.
        /// </summary>
        /// <returns></returns>
        public  decimal GetMeasureCurr()
        {
            return Decimal.Parse(GetMeasVal()[1])/1000;

        }

        /// <summary>
        /// Вовзращает измеренное источником питания напряжение на выходных клеммах.
        /// </summary>
        /// <returns></returns>
        public  decimal GetMeasureVolt()
        {
            return Decimal.Parse(GetMeasVal()[2])/1000;
        }

        /// <summary>
        /// Возвращает массив строк, содержащий уставки тока и напряжения источника питания.
        /// </summary>
        /// <returns></returns>
        private string[] GetStateVal()
        {
                
                //если порт не открыт нужно бросить исключение
                if (!this.Open()) return null;

            this.WriteLine("R");
                string answer = this.ReadLine();
                var arrStr = answer.Split('R');

            this.Close();

            return arrStr;
        }

        /// <summary>
        /// Вовзращает величину уставки по току источника питания.
        /// </summary>
        /// <returns></returns>
        public  decimal GetStateCurr()
        {
            return Decimal.Parse(GetStateVal()[1])/1000;
        }


        /// <summary>
        /// Вовзращает величину уставки напряжения источника питания.
        /// </summary>
        /// <returns></returns>
        public  decimal GetStateVolt()
        {
            return Decimal.Parse(GetStateVal()[2])/1000;
        }

        /// <summary>
        /// Задает параметры подключения к последовательному порту. Задает символ конца строки для блока питания.
        /// </summary>
        public  void Init()
        {
            this.BaudRate = SpeedRate.R9600;
            this.Parity = Parity.None;
            this.DataBit = DBit.Bit8;
            this.StopBit = StopBits.One;
            this.EndLineTerm = "\r";
        }

   


        /// <summary>
        /// Отправляет на прибор команду перезагрузки
        /// </summary>
        public  void Reset()
        {

            if (!this.Open()) return;
            this.WriteLine("c_reset_ext");
            Thread.Sleep(3000);
            this.Close();
            
        }

        /// <summary>
        /// Конвертирует число (напряжение) в строку, необходимую для отправки на прибор. 
        /// </summary>
        /// <param name="inVar">Величина напряжения.</param>
        /// <param name="inVoltMultipliers">Единица напряжения - вольты, милливольты, микровольты.</param>
        /// <returns>Значение напряжения в строке, которое можно отправлять на прибор.</returns>
        private string VoltMultipliersConvert(decimal inVar,VoltMultipliers inVoltMultipliers)
        {
            //можно ввести вольты
            //можно ввести милливольты
            //микровольты!!!!

            switch (inVoltMultipliers)
            {
                case VoltMultipliers.Si:
                    inVar *= 1000;
                    break;
                case VoltMultipliers.Micro:
                    inVar /= 1000;
                    break;
                
            }
            
            string resultStr = AP.Math.MathStatistics.Round(ref inVar, 0, true);
            
            //максимально до 17 знаков, разделитель точка
            //inVar.ToString("G17", new CultureInfo("en-US"));

           
            if (resultStr.Length > 6) throw new ArgumentOutOfRangeException();
            
                for (; resultStr.Length < 6; )
                    resultStr = resultStr.Insert(0, "0");
           

            return resultStr.Replace(',','.');
        }

        /// <summary>
        /// Конвертирует число (ток) в строку, необходимую для отправки на прибор.
        /// </summary>
        /// <param name="inVar">Величина тока.</param>
        /// <param name="inCurrMultipliers"></param>
        /// <returns>Значение тока в строке, которое можно отправлять на прибор.</returns>

        private string CurrMultipliersConvert(decimal inVar, CurrMultipliers inCurrMultipliers)
        {
            //можно ввести вольты
            //можно ввести милливольты
            //микровольты!!!!

            switch (inCurrMultipliers)
            {
                case CurrMultipliers.Si:
                    inVar *= 1000;
                    break;
                case CurrMultipliers.Micro:
                    inVar /= 1000;
                    break;

            }

            string resultStr = AP.Math.MathStatistics.Round(ref inVar, 0, true);

            //максимально до 17 знаков, разделитель точка
            //inVar.ToString("G17", new CultureInfo("en-US"));


            if (resultStr.Length > 5) throw new ArgumentOutOfRangeException();

            for (; resultStr.Length < 5;)
                resultStr = resultStr.Insert(0, "0");


            return resultStr;
        }


        /// <summary>
        /// Позволяет задать уставку по току для источника питания.
        /// </summary>
        /// <param name="inCurr">Ток который необходимо задать.</param>
        /// <param name="inUnitCurrMultipliers">Единицы измерения тока.</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public  bool SetStateCurr(decimal inCurr, CurrMultipliers inUnitCurrMultipliers = CurrMultipliers.Si)
        {
            //нужно выбросить исключение
           if (!this.Open()) return  false;

            this.WriteLine("I" + CurrMultipliersConvert(inCurr, inUnitCurrMultipliers));
            Thread.Sleep(2500);
            string answer = this.ReadLine();
            this.Close();

            if (string.Equals(answer, "I")) return true;

            return false;

        }

        /// <summary>
        /// Позволяет задать уставку по напряжению.
        /// </summary>
        /// <param name="inVolt">Значение напряжения.</param>
        /// <param name="inUnitVoltMultipliers">Единицы измерения</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public  bool SetStateVolt(decimal inVolt, Multipliers mult = Multipliers.None)
        {
            //нужно выбросить исключение
            if (!this.Open()) return false;

            //блок питания понимает значения только в милливольтах
           
            switch (mult)
            {
                case Multipliers.None:
                    inVolt = inVolt * (decimal)Multipliers.Kilo.GetDoubleValue();
                    break;
                case Multipliers.Mili:
                    break;
                case Multipliers.Nano:
                    inVolt = inVolt * (decimal) Multipliers.Kilo.GetDoubleValue();
                    break;
                case Multipliers.Kilo:
                    inVolt = inVolt * (decimal) Multipliers.Mega.GetDoubleValue();
                    break;
                case Multipliers.Mega:
                    inVolt = inVolt * (decimal) Multipliers.Giga.GetDoubleValue();
                    break;

            }
            string resultStr = AP.Math.MathStatistics.Round(ref inVolt, 0, true);

            if (resultStr.Length > 6) throw new ArgumentOutOfRangeException();

            for (; resultStr.Length < 6;)
                resultStr = resultStr.Insert(0, "0");


            this.WriteLine("U" + resultStr);
            Thread.Sleep(2000);
            string answer = this.ReadLine();
            this.Close();

            if (string.Equals(answer, "U")) return true;

            return false;
        }

        /// <summary>
        /// Включает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public bool OnOutput()
        {
            //нужно выбросить исключение
            if (!this.Open()) return false;
            this.WriteLine("Y");
            Thread.Sleep(700);
            string answer = this.ReadLine();
            this.Close();

            if (string.Equals(answer, "Y")) return true;

            return false;
        }

        /// <summary>
        /// Выключает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public bool OffOutput()
        {
            //нужно выбросить исключение
            if (!this.Open()) return false;
            this.WriteLine("N");
            Thread.Sleep(700);
            string answer = this.ReadLine();
            this.Close();

            if (string.Equals(answer, "N")) return true;

            return false;
        }


        protected B571Pro(string portName) : base(portName)
        {
            //Sp = new Sp(PortName, Sp.SpeedRate.R9600, Parity.None, Sp.DataBit.Bit8, StopBits.One);
            //Sp.EndLineTerm = "\r";

        }

        protected B571Pro()
        {
          
        }
    }




    //public abstract class AbstractB571Pro :ComPort
    //{
 

      
    //    /// <summary>
    //    /// Проверка подключения к источнику питания 
    //    /// </summary>
        
    //   public abstract void Init ();
    //    /// <summary>
    //    /// Сброс блока к настройкам по умолчанию.
    //    /// </summary>
    //    public abstract void Reset();

    //    //========================
    //    //  Режимы функционирования блока
    //    // 1. Работа тольк от ПК с блокировкой кнопок cont_ps_ext
    //    // 2. Управление от ПК и с клавиатуры прибора cps_int_ext
    //    // 3. Выключает управление от ПК, только управление с клавиатуры прибора cont_ps_int 
    //    //========================


    //    /// <summary>
    //    /// Вовращает уставку постоянного тока блока питания.
    //    /// </summary>
    //    public abstract decimal GetStateCurr();

    //    /// <summary>
    //    /// Задает уставку тока блока питания
    //    /// </summary>
    //    /// <param name="inCurr">величина уставки тока</param>
    //    /// <param name="inUnitCurrMultipliers">единицы измерения тока, по умолчанию амперы</param>
    //    /// <returns>возвращает true в случае успешной установки значения на приборе, в противном случае false.</returns>
    //    public abstract bool SetStateCurr(decimal inCurr, CurrMultipliers inUnitCurrMultipliers = CurrMultipliers.Si);


    //    /// <summary>
    //    /// Измеренное блоком значение тока в цепи.
    //    /// </summary>
    //    public abstract decimal GetMeasureCurr();


    //    /// <summary>
    //    /// Возвращает уставку постоянного напряжения блока питания.
    //    /// </summary>
    //    public abstract decimal GetStateVolt();

    //    /// <summary>
    //    /// Задает уставку напряжения блока питания
    //    /// </summary>
    //    /// <param name="inVolt">значения устанавливаемого напряжения</param>
    //    /// <param name="inUnitVoltMultipliers">единицы измерения устанавливаемого напряжения</param>
    //    /// <returns>возвращает true в случае успешной установки значения на приборе, в противном случае false.</returns>
    //    public abstract bool SetStateVolt(decimal inVolt, VoltMultipliers inUnitVoltMultipliers = VoltMultipliers.Si);

    //    /// <summary>
    //    /// Измеренное блоком питания постоянное напряжение.
    //    /// </summary>
    //    public abstract decimal GetMeasureVolt();

    //    /// <summary>
    //    /// Включает выход источника питания.
    //    /// </summary>
    //    /// <returns></returns>
    //    public abstract bool OnOutput();

    //    /// <summary>
    //    /// Выключает выход источника питания.
    //    /// </summary>
    //    /// <returns></returns>
    //    public abstract bool OffOutput();

    //    protected AbstractB571Pro()
    //    {
            
    //    }

    //    protected AbstractB571Pro(string portName) : base(portName)
    //    {
    //    }

    //    protected AbstractB571Pro(string portName, SpeedRate bautRate) : base(portName, bautRate)
    //    {
    //    }

    //    protected AbstractB571Pro(string portName, SpeedRate bautRate, Parity parity) : base(portName, bautRate, parity)
    //    {
    //    }

    //    protected AbstractB571Pro(string portName, SpeedRate bautRate, Parity parity, DBit databit) : base(portName, bautRate, parity, databit)
    //    {
    //    }

    //    protected AbstractB571Pro(string portName, SpeedRate bautRate, Parity parity, DBit databit, StopBits stopbits) : base(portName, bautRate, parity, databit, stopbits)
    //    {
    //    }
    //}
}