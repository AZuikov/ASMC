using System;
using System.IO.Ports;
using System.Threading;
using AP.Reports.Utils;
//using System.Globalization;
//using System.Text.RegularExpressions;


namespace ASMC.Devices.Port.Profigrupp
{

    public enum VoltMultipliers
    {
        [StringValue(" V")]
        Volt,
        [StringValue(" mV")]
        Milli,
        [StringValue(" microV")]
        Micro,
        [StringValue("")]
        SI
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
        SI
    }


    public abstract class B5_71_PRO :  AbstractB5_71_PRO

    {
        /// <summary>
        /// Производится подключение к блоку питания. Переводит прибор в режим дистанционного управления.
        /// </summary>
        /// <param name="PortName">Имя последовательного порта, к которому подключен блок питания.</param>
        /// <returns>Если при подключении к блоку питания получен верный ответ функция вернет true. В противном случае вернет false.</returns>
        public bool InitDevice(String PortName)
        {
            Init(PortName);

            //перезагружаем блок питания
            this.Reset();

            if (!ComPort.Open())
            {
                ComPort.Close();
                return false;
            }

           
            //Переводим в режим дистанционного управления. При этом работают клавиши на панели прибора.
            ComPort.WriteLine("cps_int_ext");
            Thread.Sleep(500);
            //В ответ на команду перевода врежим дистанционного управления прибор должен прислать сообщение "EIC"
            string answer = ComPort.ReadLine();
            Thread.Sleep(500);

            ComPort.Close();

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
            if (!ComPort.Open()) return null;

            ComPort.Write("M");
            string answer = ComPort.ReadLine();
            var StrArr = answer.Split('M');
            
            ComPort.Close();
            return StrArr;
        }

        /// <summary>
        /// Вовзращает измеренное источником питания значения тока в цепи.
        /// </summary>
        /// <returns></returns>
        public override decimal GetMeasureCurr()
        {
            return Decimal.Parse(GetMeasVal()[1])/1000;

        }

        /// <summary>
        /// Вовзращает измеренное источником питания напряжение на выходных клеммах.
        /// </summary>
        /// <returns></returns>
        public override decimal GetMeasureVolt()
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
                if (!ComPort.Open()) return null;

                ComPort.WriteLine("R");
                string answer = ComPort.ReadLine();
                var ArrStr = answer.Split('R');

                ComPort.Close();

            return ArrStr;
        }

        /// <summary>
        /// Вовзращает величину уставки по току источника питания.
        /// </summary>
        /// <returns></returns>
        public override decimal GetStateCurr()
        {
            return Decimal.Parse(GetStateVal()[1])/1000;
        }


        /// <summary>
        /// Вовзращает величину уставки напряжения источника питания.
        /// </summary>
        /// <returns></returns>
        public override decimal GetStateVolt()
        {
            return Decimal.Parse(GetStateVal()[2])/1000;
        }

        /// <summary>
        /// Задает параметры подключения к последовательному порту. Задает символ конца строки для блока питания.
        /// </summary>
        /// <param name="portName">имя последовательного порта, к которому подключен блок питания</param>
        public override void Init(string portName)
        {
            ComPort = new ComPort(portName, ComPort.SpeedRate.R9600, Parity.None, ComPort.DataBit.Bit8, StopBits.One);
            ComPort.EndLineTerm = "\r";
        }

       
        /// <summary>
        /// Отправляет на прибор команду перезагрузки
        /// </summary>
        public override void Reset()
        {
                       

            if (!ComPort.Open()) return;
            ComPort.WriteLine("c_reset_ext");
            Thread.Sleep(3000);
            ComPort.Close();
            
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
                case VoltMultipliers.SI:
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
           

            return resultStr;
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
                case CurrMultipliers.SI:
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
        public override bool SetStateCurr(decimal inCurr, CurrMultipliers inUnitCurrMultipliers = CurrMultipliers.SI)
        {
            //нужно выбросить исключение
           if (!ComPort.Open()) return  false;

            ComPort.WriteLine("I" + CurrMultipliersConvert(inCurr, inUnitCurrMultipliers));
            Thread.Sleep(2500);
            string answer = ComPort.ReadLine();
            ComPort.Close();

            if (string.Equals(answer, "I")) return true;

            return false;

        }

        /// <summary>
        /// Позволяет задать уставку по напряжению.
        /// </summary>
        /// <param name="inVolt">Значение напряжения.</param>
        /// <param name="inUnitVoltMultipliers">Единицы измерения</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public override bool SetStateVolt(decimal inVolt, VoltMultipliers inUnitVoltMultipliers = VoltMultipliers.SI)
        {
            //нужно выбросить исключение
            if (!ComPort.Open()) return false;
            ComPort.WriteLine("U" + VoltMultipliersConvert(inVolt, inUnitVoltMultipliers));
            Thread.Sleep(2500);
            string answer = ComPort.ReadLine();
            ComPort.Close();

            if (string.Equals(answer, "U")) return true;

            return false;
        }

        /// <summary>
        /// Включает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public override bool OnOutput()
        {
            //нужно выбросить исключение
            if (!ComPort.Open()) return false;
            ComPort.WriteLine("Y");
            Thread.Sleep(700);
            string answer = ComPort.ReadLine();
            ComPort.Close();

            if (string.Equals(answer, "Y")) return true;

            return false;
        }

        /// <summary>
        /// Выключает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public override bool OffOutput()
        {
            //нужно выбросить исключение
            if (!ComPort.Open()) return false;
            ComPort.WriteLine("N");
            Thread.Sleep(700);
            string answer = ComPort.ReadLine();
            ComPort.Close();

            if (string.Equals(answer, "N")) return true;

            return false;
        }


        protected B5_71_PRO(string PortName) : base(PortName)
        {
            ComPort = new ComPort(PortName, ComPort.SpeedRate.R9600, Parity.None, ComPort.DataBit.Bit8, StopBits.One);
            ComPort.EndLineTerm = "\r";
        }

        protected B5_71_PRO(string PortName, SpeedRate bautRate) : base(PortName, bautRate)
        {
            ComPort = new ComPort(PortName, bautRate, Parity.None, ComPort.DataBit.Bit8, StopBits.One);
            ComPort.EndLineTerm = "\r";
        }

        protected B5_71_PRO(string PortName, SpeedRate bautRate, Parity parity) : base(PortName, bautRate, parity)
        {
            ComPort = new ComPort(PortName, bautRate, parity, ComPort.DataBit.Bit8, StopBits.One);
            ComPort.EndLineTerm = "\r";
        }

        protected B5_71_PRO(string PortName, SpeedRate bautRate, Parity parity, DataBit databit) : base(PortName, bautRate, parity, databit)
        {
            ComPort = new ComPort(PortName, bautRate, parity, databit, StopBits.One);
            ComPort.EndLineTerm = "\r";
        }

        protected B5_71_PRO(string PortName, SpeedRate bautRate, Parity parity, DataBit databit, StopBits stopbits) : base(PortName, bautRate, parity, databit, stopbits)
        {
            ComPort = new ComPort(PortName, bautRate, parity, databit, stopbits);
            ComPort.EndLineTerm = "\r";
        }

        protected B5_71_PRO()
        {
        }
    }

    public abstract class AbstractB5_71_PRO :ComPort
    {
        protected ComPort ComPort;

        /// <summary>
        /// Максимальное значение напряжения источника питания
        /// </summary>
        public decimal VoltMax { get; protected set; }

        /// <summary>
        /// Максимальное значение тока источника питания
        /// </summary>
        public decimal CurrMax { get; protected set; }

        /// <summary>
        /// Погрешность нетсабильности выходного напряжения.
        /// </summary>
        public decimal tolleranceVoltageUnstability { get; protected set; }

        /// <summary>
        /// Погрешность нетсабильности выходного тока.
        /// </summary>
        public decimal tolleranceCurrentUnstability { get; protected set; }

        /// <summary>
        /// Допустимый уровень пульсаций по напряжению
        /// </summary>
        public decimal tolleranceVoltPuls { get; protected set; }

        /// <summary>
        /// Допустимый уровень пульсаций по току
        /// </summary>
        public decimal tolleranceCurrentPuls { get; protected set; }

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению напряжения источника питания.
        /// </summary>
        /// <param name="inVoltPoint">Поверяемая точка. В вольтах</param>
        /// <returns>Погрешность в вольтах.</returns>
        public decimal tolleranceFormulaVolt(decimal inVoltPoint,
            VoltMultipliers inUnitVoltMultipliers = VoltMultipliers.SI)
        {
            switch (inUnitVoltMultipliers)
            {
                case VoltMultipliers.SI:
                case VoltMultipliers.Volt:
                    break;

                case VoltMultipliers.Milli:
                    inVoltPoint /= 1000;
                    break;
                case VoltMultipliers.Micro:
                    inVoltPoint /= 1000000;
                    break;

            }

            return (decimal)0.002 * inVoltPoint + (decimal)0.1;
        }

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению тока источника питания.
        /// </summary>
        /// <param name="inVoltPoint">Поверяемая точка. В амперах</param>
        /// <returns>Погрешность в амперах</returns>
        public decimal tolleranceFormulaCurrent(decimal inCurrPoint,
            CurrMultipliers inUnitCurrMultipliers = CurrMultipliers.SI)
        {
            switch (inUnitCurrMultipliers)
            {
                case CurrMultipliers.SI:
                case CurrMultipliers.Amp:
                    break;
                case CurrMultipliers.Milli:
                    inCurrPoint *= 1000;
                    break;
                case CurrMultipliers.Micro:
                    inCurrPoint *= 1000000;
                    break;
            }

            return (decimal)0.01 * inCurrPoint + (decimal)0.05;
        }

        /// <summary>
        /// Проверка подключения к источнику питания 
        /// </summary>
        /// <param name="portName">Передать имя последовательного порта</param>
       public abstract void Init (string portName);
        /// <summary>
        /// Сброс блока к настройкам по умолчанию.
        /// </summary>
        public abstract void Reset();

        //========================
        //  Режимы функционирования блока
        // 1. Работа тольк от ПК с блокировкой кнопок cont_ps_ext
        // 2. Управление от ПК и с клавиатуры прибора cps_int_ext
        // 3. Выключает управление от ПК, только управление с клавиатуры прибора cont_ps_int 
        //========================


        /// <summary>
        /// Вовращает уставку постоянного тока блока питания.
        /// </summary>
        public abstract decimal GetStateCurr();

        /// <summary>
        /// Задает уставку тока блока питания
        /// </summary>
        /// <param name="inCurr">величина уставки тока</param>
        /// <param name="inUnitCurrMultipliers">единицы измерения тока, по умолчанию амперы</param>
        /// <returns>возвращает true в случае успешной установки значения на приборе, в противном случае false.</returns>
        public abstract bool SetStateCurr(decimal inCurr, CurrMultipliers inUnitCurrMultipliers = CurrMultipliers.SI);


        /// <summary>
        /// Измеренное блоком значение тока в цепи.
        /// </summary>
        public abstract decimal GetMeasureCurr();


        /// <summary>
        /// Возвращает уставку постоянного напряжения блока питания.
        /// </summary>
        public abstract decimal GetStateVolt();

        /// <summary>
        /// Задает уставку напряжения блока питания
        /// </summary>
        /// <param name="inVolt">значения устанавливаемого напряжения</param>
        /// <param name="inUnitVoltMultipliers">единицы измерения устанавливаемого напряжения</param>
        /// <returns>возвращает true в случае успешной установки значения на приборе, в противном случае false.</returns>
        public abstract bool SetStateVolt(decimal inVolt, VoltMultipliers inUnitVoltMultipliers = VoltMultipliers.SI);

        /// <summary>
        /// Измеренное блоком питания постоянное напряжение.
        /// </summary>
        public abstract decimal GetMeasureVolt();

        /// <summary>
        /// Включает выход источника питания.
        /// </summary>
        /// <returns></returns>
        public abstract bool OnOutput();

        /// <summary>
        /// Выключает выход источника питания.
        /// </summary>
        /// <returns></returns>
        public abstract bool OffOutput();

        protected AbstractB5_71_PRO()
        {
        }

        protected AbstractB5_71_PRO(string PortName) : base(PortName)
        {
        }

        protected AbstractB5_71_PRO(string PortName, SpeedRate bautRate) : base(PortName, bautRate)
        {
        }

        protected AbstractB5_71_PRO(string PortName, SpeedRate bautRate, Parity parity) : base(PortName, bautRate, parity)
        {
        }

        protected AbstractB5_71_PRO(string PortName, SpeedRate bautRate, Parity parity, DataBit databit) : base(PortName, bautRate, parity, databit)
        {
        }

        protected AbstractB5_71_PRO(string PortName, SpeedRate bautRate, Parity parity, DataBit databit, StopBits stopbits) : base(PortName, bautRate, parity, databit, stopbits)
        {
        }
    }
}