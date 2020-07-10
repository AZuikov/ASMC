// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using System.Threading;
using MathNet.Numerics.Statistics;

namespace ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope
{
    public class Calibr_9500B : IeeeBase
    {
        /// <summary>
        /// Множитель
        /// </summary>
        public enum Multipliers
        {
            Mega,
            Mili,
            Nano,
            Kilo,
            Micro,
            Si
        }
        public enum Chanel
        {
            CH1,
            CH2,
            CH3,
            CH4,
            CH5
        }
        public enum State
        {
            ON,
            OFF
        }

        public Calibr_9500B() : base()
        {

        }
        public Calibr_9500B(string connect) : this()
        {
            Stringconection = connect;
        }
        /// <summary>
        /// Управление переключением выхода: включено/выключено 
        /// </summary>
        public static string Output(State st)
        {
            return "OUTP " + st.ToString();
        }
        /// <summary>
        /// Данная подсистема используется для конфигурирования выходных каналов, которые используются как выходы сигнала и запуска. 
        /// </summary>
        public class ROUTe
        {
            public class Single
            {
                /// <summary>
                /// Данная команда используется для определения канала, связанного с сигнальным выходом. <cpd> не включает выход, а только выбирает используемый сигнальный канал.
                /// </summary>
                /// <param name="ch">The ch.</param>
                /// <returns></returns>
                public static string SetChanel(Chanel ch)
                {
                    return "ROUT:SIGN " + ch.ToString();
                }
                /// <summary>
                /// Данная команда выбирает между 50 Ом и 1 МОм импедансом осциллографа, в соответствии с уровнями для выбранного сигнального канала.
                /// </summary>
                /// <param name="imp">The imp.</param>
                /// <returns></returns>
                public static string SetImpedans(Impedans imp)
                {
                    return "ROUT:SIGN:IMP " +(int)imp;
                }
                /// <summary>
                /// Выходной импеданс
                /// </summary>
                public enum Impedans
                {
                    /// <summary>
                    /// 50 ОМ
                    /// </summary>
                    Res_50 = 50,
                    /// <summary>
                    /// 1 МОм
                    /// </summary>
                    Res_1M = 1000
                }
            }
        }
        /// <summary>
        /// Дополнительный функцианал
        /// </summary>
        public class AUXFunctional
        {
            /// <summary>
            /// Тип измерения
            /// </summary>
            public enum TypeMes
            {
                /// <summary>
                /// Сопротивление
                /// </summary>
                RES,
                /// <summary>
                /// Емкость
                /// </summary>
                CAP
            }
            /// <summary>
            /// Данная команда используется для выбора функции измерения входного сопротивления или входной емкости испытуемого осциллографа
            /// </summary>
            /// <param name="tm"></param>
            /// <returns></returns>
            public static string SetMeasure(TypeMes tm)
            {
                return "CONF:" + tm.ToString();
            }
            /// <summary>
            /// Запрос значения сопротивления или емкости
            /// </summary>
            public const string QuaryValue = "READ?";
        }
        /// <summary>
        /// Настройка выхода
        /// </summary>
        public class Source
        {
           public enum Shap
            {
                /// <summary>
                /// Определяет, что последующий выбор напряжения (VOLT) или тока (CURR) будет иметь только постоянную составляющую (DC). 
                /// </summary>
                DC,
                /// <summary>
                /// Определяет, что последующий выбор напряжения (VOLT) или тока (CURR) связан с сигналом прямоугольной формы
                /// </summary>
                SQU,
                /// <summary>
                /// Выбирает функцию перепада импульса. Форма сигнала выбирается отдельной командой.
                /// </summary>
                EDG,
                /// <summary>
                /// Выбирает форму сигнала временного маркера. 
                /// </summary>
                MARK,
                /// <summary>
                /// Выбирает нормированный синусоидальный сигнал. 
                /// </summary>
                SIN,
                /// <summary>
                /// Выбирает энергию импульса, используемого для тестирования защиты от перегрузки осциллографа. 
                /// </summary>
                OPULS,
                /// <summary>
                /// Выбирает форму полного видеосигнала. 
                /// </summary>
                TEL,
                /// <summary>
                /// Выбирает замкнутое или разомкнутое состояние активной головки для определения тока утечки осциллографа. 
                /// </summary>
                LAE,
                /// <summary>
                /// Выбирает функцию пилообразного сигнала
                /// </summary>
                RAMP,
                /// <summary>
                /// Выбирает функцию выравнивания задержки
                /// </summary>
                SKEW,
                /// <summary>
                /// Выбирает функцию длительности импульса 
                /// </summary>
                PWID,
                /// <summary>
                /// Выбирает входной сигнал по дополнительному входу (Auxiliary). 
                /// </summary>
                EXT
            }
            /// <summary>
            /// Данная команда определяет основной требуемый сигнальный выход, т.е. выбирает функцию источника калибратора 9500B. 
            /// </summary>
            public static string SetFunc(Shap sp)
            {
                return "SOUR:FUNC " + sp.ToString();
            }
            /// <summary>
            /// Настройки выходного параметра
            /// </summary>
            public class Parametr
            {
                /// <summary>
                /// Настройка постоянного напряжения
                /// </summary>
                public class DC
                {
                    /// <summary>
                    /// Данная команда устанавливает выход напряжения постоянного тока (DCV) в ноль (0 В), если выбрано «ON» и вернет предыдущее значение, если выбрано «OFF». Изменение функция отключает состояние заземления. С выходом, установленным в состояние «ON», выводится ошибка конфликта параметров настройки, если выбрана не функция DC. 
                    /// </summary>
                    /// <param name="st">The st.</param>
                    /// <returns></returns>
                    public static string SetGND(State st)
                    {
                        return "SOUR:PAR:DC:GRO "+st.ToString();
                    }
                    /// <summary>
                    /// Данная команда позволяет установить постоянный сигнал на нескольких каналах (если устанавливается в «ON») и отключить многоканальность, если выбрано «OFF» 
                    /// </summary>
                    /// <param name="st">The st.</param>
                    /// <returns></returns>
                    public static string MCH(State st)
                    {
                        return "SOUR:PAR:DC:MCH " + st.ToString();
                    }
                }
                /// <summary>
                /// Настройка Меандра
                /// </summary>
                public class SQU
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public enum Polar
                    {
                        /// <summary>
                        ///  устанавливает выходной прямоугольный сигнал положительной полярности относительно заземления
                        /// </summary>
                        POS,
                        /// <summary>
                        /// устанавливает выходной прямоугольный сигнал отрицательной полярности относительно заземления
                        /// </summary>
                        NEG,
                        /// <summary>
                        /// устанавливает выходной прямоугольный сигнал симметрично относительно земли
                        /// </summary>
                        SYMM
                    }
                    /// <summary>
                    /// Данная команда выбирает полярность прямоугольного сигнала: выше, ниже или симметрично относительно «0» вольт
                    /// </summary>
                    /// <param name="pr">The pr.</param>
                    /// <returns></returns>
                    public static string SetPolar(Polar pr)
                    {
                        return "SOUR:PAR:SQU:POL " + pr.ToString();
                    }
                    /// <summary>
                    /// Данная команда устанавливает выходной сигнал прямоугольной формы в ноль(0 В), если выбрано «ON» и вернет предыдущее выходное значение, если выбрано «OFF».                     
                    /// </summary>
                    /// <param name="st">The st.</param>
                    /// <returns></returns>
                    public static string SetGND(State st)
                    {
                        return "SOUR:PAR:SQU:GRO " + st.ToString();
                    }
                }
                /// <summary>
                /// настройка импульса перепада
                /// </summary>
                public class EDGE
                {
                    /// <summary>
                    /// Скорость наростания
                    /// </summary>
                    public enum SpeedEdge
                    {
                        /// <summary>
                        /// 100нс
                        /// </summary>
                        Low_100n=700,
                        /// <summary>
                        /// 500 пс
                        /// </summary>
                        Mid_500p= 500,
                        /// <summary>
                        /// 150 пс
                        /// </summary>
                        Fast_150p=100
                    }
                    /// <summary>
                    /// Данная команда выбирает скорость (скорость нарастания выходного напряжения) функции Edge
                    /// </summary>
                    /// <param name="Se"></param>
                    /// <returns></returns>
                    public static string SetSpeed(SpeedEdge Se)
                    {
                        return "SOUR:PAR:EDGE:SPE " + (double)Se * Math.Pow(10, -12);
                    }
                    public enum Derection
                    {
                        /// <summary>
                        ///  устанавливает положительный перепад
                        /// </summary>
                        RIS,
                        /// <summary>
                        /// устанавливает отрицательный перепад
                        /// </summary>
                        FALL
                    }
                    /// <summary>
                    /// Выбирает направление перепада, который следует за запуском
                    /// </summary>
                    /// <param name="Se"></param>
                    /// <returns></returns>
                    public static string SetDerection(Derection Der)
                    {
                        return "SOUR:PAR:EDGE:TRAN " + Der;
                    }
                }

            }
            /// <summary>
            /// Установка напряжения
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="mult">The mult.</param>
            /// <returns></returns>
            public static string SetVoltage(double value, Multipliers mult= Multipliers.Si)
            {
                return "SOUR:VOLT " + Convert(value, mult);
            }
            /// <summary>
            /// Установка частоты
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="mult">The mult.</param>
            /// <returns></returns>
            public static string SetFreq(double value, Multipliers mult = Multipliers.Kilo)
            {
                return "SOUR:FREQ " + Convert(value, mult);
            }
            /// <summary>
            /// Установка периода
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="mult">The mult.</param>
            /// <returns></returns>
            public static string SetPeriod(double value, Multipliers mult)
            {
                return "SOUR:PER " + Convert(value, mult);
            }
        }
        /// <summary>
        /// Пребразует данные в нужных единицах
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="Mult">The mult.</param>
        /// <returns></returns>
        public double DataPreparationAndConvert(string date, Multipliers Mult = Multipliers.Si)
        {
            string[] Value = date.Split(',');
            double[] a = new double[Value.Length];
            for (int i = 0; i < Value.Length; i++)
            {
                a[i] = Convert(Value[i], Mult);
            }
            return Statistics.Mean(a) < 0 ? Statistics.RootMeanSquare(a) * -1 : Statistics.RootMeanSquare(a);
        }
       
        private double Convert(string date, Multipliers Mult)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            double _return = 0;
            double[] dDate = new double[2];
            string[] Value = date.Split('E');
            dDate[0] = System.Convert.ToDouble(Value[0]);
            dDate[1] = System.Convert.ToDouble(Value[1]);
            switch (Mult)
            {
                case Multipliers.Mega:
                    _return = (dDate[0] * Math.Pow(10, dDate[1])) * 1E-6;
                    break;
                case Multipliers.Mili:
                    _return = (dDate[0] * Math.Pow(10, dDate[1])) * 1E3;
                    break;
                case Multipliers.Nano:
                    _return = (dDate[0] * Math.Pow(10, dDate[1])) * 1E9;
                    break;
                case Multipliers.Kilo:
                    _return = (dDate[0] * Math.Pow(10, dDate[1])) * 1E-3;
                    break;
                case Multipliers.Micro:
                    _return = (dDate[0] * Math.Pow(10, dDate[1])) * 1E6;
                    break;
                case Multipliers.Si:
                    _return = (dDate[0] * Math.Pow(10, dDate[1]));
                    break;
            }
            return _return;
        }
        private static string Convert(double date, Multipliers Mult)
        {
            double _data = 0;
            switch (Mult)
            {
                case Multipliers.Mega:
                    _data = date * 1E6;
                    break;
                case Multipliers.Mili:
                    _data = date * 1E-3;
                    break;
                case Multipliers.Nano:
                    _data = date * 1E-9;
                    break;
                case Multipliers.Kilo:
                    _data = date * 1E3;
                    break;
                case Multipliers.Micro:
                    _data = date * 1E-6;
                    break;
                case Multipliers.Si:
                    _data = date;
                    break;
            }
            return _data.ToString();
        }
    }
}
