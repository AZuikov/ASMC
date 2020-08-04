using System;
using System.IO.Ports;
using System.Threading;
using AP.Utils.Data;

//using System.Globalization;
//using System.Text.RegularExpressions;


namespace ASMC.Devices.Port.Profigrupp
{
    
    public abstract class B571Pro : ComPort
    {
        /// <summary>
        /// Установка напряжения или тока источника питания
        /// </summary>
        public enum EMode
        {
            [StringValue("U")] Vol,
            [StringValue("I")] Curr
        }

        /// <summary>
        /// Перечень режимов работы источника питания.
        /// </summary>
        public enum RemoteState
        {
            [StringValue("cont_ps_ext")] OnlyPcMode, //Работа тольк от ПК с блокировкой кнопок cont_ps_ext
            [StringValue("cps_int_ext")] PcAndFrontPanel, //Управление от ПК и с клавиатуры прибора cps_int_ext
            [StringValue("cont_ps_int")] OnlyFrontPanel, //Выключает управление от ПК, только управление с клавиатуры прибора cont_ps_int 
            [StringValue("c_reset_ext")] ResetBp //команда перезагрузки блока питания
        }


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

            value *= (decimal)EnumExtensions.GetDoubleValue(mult);
            return (decimal)0.002 * value + (decimal)0.1;
        }

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению тока источника питания.
        /// </summary>
        /// <returns>Погрешность в амперах</returns>
        public decimal TolleranceFormulaCurrent(decimal value, Multipliers mult = Multipliers.None)
        {
            value *= (decimal)EnumExtensions.GetDoubleValue(mult);
            return (decimal)0.01 * value + (decimal)0.05;
        }



        /// <summary>
        /// Производится подключение к блоку питания. Переводит прибор в режим дистанционного управления.
        /// </summary>
        /// <returns>Если при подключении к блоку питания получен верный ответ функция вернет true. В противном случае вернет false.</returns>
        public B571Pro InitDevice()
        {
            this.BaudRate = SpeedRate.R9600;
            this.Parity = Parity.None;
            this.DataBit = DBit.Bit8;
            this.StopBit = StopBits.One;
            this.EndLineTerm = "\r";

            
            //перезагружаем блок питания
            this.Reset();

            //Переводим в режим дистанционного управления. При этом работают клавиши на панели прибора.
            //В ответ на команду перевода врежим дистанционного управления прибор должен прислать сообщение "EIC"
            string answer = this.QueryLine(EnumExtensions.GetStringValue(RemoteState.PcAndFrontPanel));
            
            if (String.Equals(answer, "EIC"))
                return this;

            throw new Exception("Инициализация блока не прошла успешно.");

        }

        /// <summary>
        /// Возвращает массив из трех строк. !!! Первый элемент пустой!!! . 
        /// Второй элемент это значение тока в мА. 
        /// Третий элемент это значение напряжение в мВ.
        /// </summary>
        /// <returns></returns>
        private string[] GetMeasVal()
        {

            string answer = this.QueryLine("M");
            var strArr = answer.Split('M');
            
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
               
                string answer = this.QueryLine("R");
                var arrStr = answer.Split('R');
            
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
        /// Отправляет на прибор команду перезагрузки
        /// </summary>
        public  void Reset()
        {
            this.WriteLine(EnumExtensions.GetStringValue(RemoteState.ResetBp));
            Thread.Sleep(2000);
        }

       


        /// <summary>
        /// Позволяет задать уставку по току для источника питания.
        /// </summary>
        /// <param name="inCurr">Ток который необходимо задать.</param>
        /// <param name="inUnitCurrMultipliers">Единицы измерения тока (милли, кило, амперы).</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public  B571Pro SetStateCurr(decimal inCurr, Multipliers mult = Multipliers.None)
        {

            decimal inCurrToDevice = inCurr * (decimal)(EnumExtensions.GetDoubleValue(mult) * 1E4);

            string resultStr = AP.Math.MathStatistics.Round(ref inCurrToDevice, 0, true);

            if (resultStr.Length > 6) throw new ArgumentOutOfRangeException();

            for (; resultStr.Length < 6;)
                resultStr = resultStr.Insert(0, "0");
            
            string answer = this.QueryLine("I" + resultStr);
            Thread.Sleep(2000);
            if (string.Equals(answer, "I")) return this;

            throw new ArgumentException($"Неверное значение уставки по току: {inCurr} => {inCurrToDevice}");

        }

        /// <summary>
        /// Позволяет задать уставку по напряжению.
        /// </summary>
        /// <param name="inVolt">Значение напряжения.</param>
        /// <param name="inUnitVoltMultipliers">Единицы измерения напряжения (милли, кило, вольты)</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public  B571Pro SetStateVolt(decimal inVolt, Multipliers mult = Multipliers.None)
        {
           
            //блок питания понимает значения только в милливольтах
           decimal inVoltToDevice = inVolt * (decimal) (EnumExtensions.GetDoubleValue(mult) * 1E3);
           
            string resultStr = AP.Math.MathStatistics.Round(ref inVoltToDevice, 0, true);

            if (resultStr.Length > 6) throw new ArgumentOutOfRangeException();

            for (; resultStr.Length < 6;)
                resultStr = resultStr.Insert(0, "0");
            
            string answer = this.QueryLine("U" + resultStr);
            Thread.Sleep(2000);

            if (string.Equals(answer, "U")) return this;

            throw new ArgumentException($"Неверное значение уставки по напряжению: {inVolt} => {inVoltToDevice}");
        }

        /// <summary>
        /// Включает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public B571Pro OnOutput()
        {
           string answer = this.QueryLine("Y");
           
            if (string.Equals(answer, "Y")) return this;

            throw  new Exception("Блок питания не отвечает на команду включения выхода");
        }

        /// <summary>
        /// Выключает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public B571Pro OffOutput()
        {
           
            string answer = this.QueryLine("N");
            
            if (string.Equals(answer, "N")) return this;

            throw new Exception("Блок питания не отвечает на команду выключения выхода");
        }


        protected B571Pro(string portName) : base(portName)
        {
            

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