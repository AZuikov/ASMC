using System;
using System.IO.Ports;
using System.Threading;
using AP.Math;
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

            [StringValue("cont_ps_int")]
            OnlyFrontPanel, //Выключает управление от ПК, только управление с клавиатуры прибора cont_ps_int 
            [StringValue("c_reset_ext")] ResetBp //команда перезагрузки блока питания
        }

        #region  Fields

        /// <summary>
        /// Погрешность нетсабильности выходного тока.
        /// </summary>
        private decimal tolCurrUnstable;

        /// <summary>
        /// Погрешность нестабильности выходного напряжения.
        /// </summary>
        private decimal tolVoltUnstable;

        #endregion

        #region Property

        /// <summary>
        /// Максимальное значение тока источника питания
        /// </summary>
        public decimal CurrMax { get; protected set; }


        /// <summary>
        /// Допустимый уровень пульсаций по току
        /// </summary>
        public decimal TolleranceCurrentPuls { get; protected set; } = 5;

        public decimal TolleranceCurrentUnstability => TollUnstable(CurrMax, (decimal) 0.05);
        public decimal TolleranceVoltageUnstability => TollUnstable(VoltMax, (decimal) 0.02);

        /// <summary>
        /// Допустимый уровень пульсаций по напряжению
        /// </summary>

        public decimal TolleranceVoltPuls { get; protected set; } = 2;


        /// <summary>
        /// Максимальное значение напряжения источника питания
        /// </summary>
        public decimal VoltMax { get; protected set; }

        #endregion


        protected B571Pro(string portName) : base(portName)
        {
        }

        protected B571Pro()
        {
        }

        #region Methods

        /// <summary>
        /// Вовзращает измеренное источником питания значения тока в цепи.
        /// </summary>
        /// <returns></returns>
        public decimal GetMeasureCurr()
        {
            return decimal.Parse(GetMeasVal()[1]) / 1000;
        }

        /// <summary>
        /// Вовзращает измеренное источником питания напряжение на выходных клеммах.
        /// </summary>
        /// <returns></returns>
        public decimal GetMeasureVolt()
        {
            return decimal.Parse(GetMeasVal()[2]) / 1000;
        }

        /// <summary>
        /// Вовзращает величину уставки по току источника питания.
        /// </summary>
        /// <returns></returns>
        public decimal GetStateCurr()
        {
            return decimal.Parse(GetStateVal()[1]) / 1000;
        }


        /// <summary>
        /// Вовзращает величину уставки напряжения источника питания.
        /// </summary>
        /// <returns></returns>
        public decimal GetStateVolt()
        {
            return decimal.Parse(GetStateVal()[2]) / 1000;
        }


        /// <summary>
        /// Производится подключение к блоку питания. Переводит прибор в режим дистанционного управления.
        /// </summary>
        /// <returns>
        /// Если при подключении к блоку питания получен верный ответ функция вернет true. В противном случае вернет
        /// false.
        /// </returns>
        public B571Pro InitDevice()
        {
            BaudRate = SpeedRate.R9600;
            Parity = Parity.None;
            DataBit = DBit.Bit8;
            StopBit = StopBits.One;
            EndLineTerm = "\r";


            //перезагружаем блок питания
            Reset();

            //Переводим в режим дистанционного управления. При этом работают клавиши на панели прибора.
            //В ответ на команду перевода врежим дистанционного управления прибор должен прислать сообщение "EIC"
            var answer = QueryLine(RemoteState.PcAndFrontPanel.GetStringValue());

            if (string.Equals(answer, "EIC"))
                return this;

            throw new Exception("Инициализация блока не прошла успешно.");
        }

        /// <summary>
        /// Выключает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public B571Pro OffOutput()
        {
            var answer = QueryLine("N");

            if (string.Equals(answer, "N")) return this;

            throw new Exception("Блок питания не отвечает на команду выключения выхода");
        }

        /// <summary>
        /// Включает выход истоника питания.
        /// </summary>
        /// <returns>Если от прибора получен положительный ответ, то вернет true. В противном случае false.</returns>
        public B571Pro OnOutput()
        {
            var answer = QueryLine("Y");

            if (string.Equals(answer, "Y")) return this;

            throw new Exception("Блок питания не отвечает на команду включения выхода");
        }


        /// <summary>
        /// Отправляет на прибор команду перезагрузки
        /// </summary>
        public void Reset()
        {
            WriteLine(RemoteState.ResetBp.GetStringValue());
            Thread.Sleep(2000);
        }


        /// <summary>
        /// Позволяет задать уставку по току для источника питания.
        /// </summary>
        /// <param name = "inCurr">Ток который необходимо задать.</param>
        /// <param name = "inUnitCurrMultipliers">Единицы измерения тока (милли, кило, амперы).</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public B571Pro SetStateCurr(decimal inCurr, Multipliers mult = Devices.Multipliers.None )
        {
            var inCurrToDevice = inCurr * (decimal) (mult.GetDoubleValue() * 1E4);

            var resultStr = MathStatistics.Round(ref inCurrToDevice, 0, true);

            if (resultStr.Length > 6) throw new ArgumentOutOfRangeException();

            for (; resultStr.Length < 6;)
                resultStr = resultStr.Insert(0, "0");

            var answer = QueryLine("I" + resultStr);
            Thread.Sleep(2000);
            if (string.Equals(answer, "I")) return this;

            throw new ArgumentException($"Неверное значение уставки по току: {inCurr} => {inCurrToDevice}");
        }

        /// <summary>
        /// Позволяет задать уставку по напряжению.
        /// </summary>
        /// <param name = "inVolt">Значение напряжения.</param>
        /// <param name = "inUnitVoltMultipliers">Единицы измерения напряжения (милли, кило, вольты)</param>
        /// <returns>Если установка величины прошла успешно возвращает true. В противном случае false.</returns>
        public B571Pro SetStateVolt(decimal inVolt, Multipliers mult = Devices.Multipliers.None)
        {
            //блок питания понимает значения только в милливольтах
            var inVoltToDevice = inVolt * (decimal) (mult.GetDoubleValue() * 1E3);

            var resultStr = MathStatistics.Round(ref inVoltToDevice, 0, true);

            if (resultStr.Length > 6) throw new ArgumentOutOfRangeException();

            for (; resultStr.Length < 6;)
                resultStr = resultStr.Insert(0, "0");

            var answer = QueryLine("U" + resultStr);
            Thread.Sleep(2000);

            if (string.Equals(answer, "U")) return this;

            throw new ArgumentException($"Неверное значение уставки по напряжению: {inVolt} => {inVoltToDevice}");
        }

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению тока источника питания.
        /// </summary>
        /// <returns>Погрешность в амперах</returns>
        public decimal TolleranceFormulaCurrent(decimal value, Multipliers mult = Devices.Multipliers.None)
        {
            value *= (decimal) mult.GetDoubleValue();
            return (decimal) 0.01 * value + (decimal) 0.05;
        }

        /// <summary>
        /// Формула расчета погрешности по воспроизведению и измерению напряжения источника питания.
        /// </summary>
        /// <param name = "value">Поверяемая точка. В вольтах</param>
        /// <param name = "mult"></param>
        /// <returns>Погрешность в вольтах.</returns>
        public decimal TolleranceFormulaVolt(decimal value, Multipliers mult = Devices.Multipliers.None)
        {
            value *= (decimal) mult.GetDoubleValue();
            return (decimal) 0.002 * value + (decimal) 0.1;
        }

        /// <summary>
        /// Возвращает массив из трех строк. !!! Первый элемент пустой!!! .
        /// Второй элемент это значение тока в мА.
        /// Третий элемент это значение напряжение в мВ.
        /// </summary>
        /// <returns></returns>
        private string[] GetMeasVal()
        {
            var answer = QueryLine("M");
            var strArr = answer.Split('M');

            return strArr;
        }

        /// <summary>
        /// Возвращает массив строк, содержащий уставки тока и напряжения источника питания.
        /// </summary>
        /// <returns></returns>
        private string[] GetStateVal()
        {
            var answer = QueryLine("R");
            var arrStr = answer.Split('R');

            return arrStr;
        }


        /// <summary>
        /// формулы расчета погрешности нестабильности тока/напряжения
        /// </summary>
        /// <param name = "bpInArg">Максимальный ток/напряжение для данной модели блока питания</param>
        /// <param name = "bpTolArg">Коэффициент из формулы расчета, различный для тока и напряжения</param>
        /// <returns></returns>
        private decimal TollUnstable(decimal bpInArg, decimal bpTolArg)
        {
            return (decimal) 0.001 * bpInArg + bpTolArg;
        }

        #endregion
    }
}