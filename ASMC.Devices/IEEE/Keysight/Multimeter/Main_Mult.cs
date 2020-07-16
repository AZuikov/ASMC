// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using MathNet.Numerics.Statistics;

namespace ASMC.Devices.IEEE.Keysight.Multimeter
{
    public class Main_Mult: IeeeBase
    {
        /// <summary>
        /// Запрос на получение значения (U, I, R, F), для преобразования использовать ConvertDate
        /// </summary>
        public const string QueryValue = "READ?";
        /// <summary>
        /// Запрос о включенных выходах. Ответ FRON - передняя панель, 
        /// </summary>
        private const string QueryTerminal = "ROUT:TERM?";
        public bool TerminalSet()
        {
            string result;
            Open();
            WriteLine(QueryTerminal);
            result = ReadString();
            Close();
            if (string.Compare(result, "FRON", true)==0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected static string secured_code_calibr;
        public Main_Mult():base()
        {

        }
        /// <summary>
        /// Узать какие входы подключены
        /// </summary>
        /// <returns>true - передняя панель, false - задняя</returns>
        public bool GetTerminalConnect()
        {
            string result;
            WriteLine(QueryTerminal);
            result=ReadString();
            switch (result)
            {
                case "FRON":
                    return true;
                case "REAR":
                    return false;
            }
            return false;
        }
        public class Calibr
        {
            public class Secured
            {
                /// <summary>
                /// Запросить статус безапасности
                /// </summary>
                public static string ChecState = "CAL:SEC:STAT?";
                /// <summary>
                /// Статус
                /// </summary>
                public enum State
                {
                    /// <summary>
                    /// Включить
                    /// </summary>
                    ON,
                    /// <summary>
                    /// Выключить
                    /// </summary>
                    OFF
                }
                /// <summary>
                /// Автовыбор предела
                /// </summary>
                public string ON = "CAL:SEC:STAT ON, " + secured_code_calibr;
                /// <summary>
                /// Автовыбор предела
                /// </summary>
                public string OFF = "CAL:SEC:STAT OFF, " + secured_code_calibr;
                /// <summary>
                /// Снятие и установка защиты
                /// </summary>
                /// <param name="state">Состояние защиты</param>
                /// <returns></returns>
                public static string InputCode(State state)
                {
                    return "CAL:SEC:STAT " + (state == 0 ? "ON, " : "OFF, ") + secured_code_calibr;
                }
                /// <summary>
                /// Снятие и установка защиты
                /// </summary>
                /// <param name="code">Код защиты</param>
                /// <param name="state">Состояние защиты</param>
                /// <returns></returns>
                public static string InputCode(string code, State state)
                {
                    return "CAL:SEC:STAT " + (state == 0 ? "ON, " : "OFF, ") + code;
                }
               

            }
            /// <summary>
            /// Типы калибровок
            /// </summary>
            public struct TypeCalValue
            {
                /// <summary>
                /// Калибровка напряжения
                /// </summary>
                public const string Volt = "V";
                /// <summary>
                /// Калибровка по току
                /// </summary>
                public const string Current = "";
            }
            /// <summary>
            /// Установить калибровочное зачение
            /// </summary>
            /// <param name="value">значение</param>
            /// <param name="TypeCalValue">Тип калибровки</param>
            /// <param name="multipliers">Множитель</param>
            /// <returns></returns>
            public static string SetCalValue(double value, string TypeCalValue, string multipliers)
            {
                return "CAL:VAL " + value + multipliers.Insert(multipliers.Length, TypeCalValue);
            }
            /// <summary>
            /// Запрос на установки калибровки
            /// </summary>
            /// <returns></returns>
            public static string QueryCal()
            {
                return "CAL?";
            }
        }
        /// <summary>
        /// Содержит перечень всех доступных множителей
        /// </summary>
        public class Multipliers
        {
            /// <summary>
            /// The mega соответствует множетелю Мега
            /// </summary>
            public const string mega = " MA";
            /// <summary>
            /// The mili соответствует множетелю мили
            /// </summary>
            public const string mili = " M";
            /// <summary>
            /// The kilo соответствует множетелю кило
            /// </summary>
            public const string kilo = " K";
            /// <summary>
            /// The micro соответствует множетелю микро
            /// </summary>
            public const string micro = " U";
            /// <summary>
            /// The nano соответствует множетелю нано
            /// </summary>
            public const string nano = " N";
            /// <summary>
            /// The si без множителя, в соответствии с СИ
            /// </summary>
            public const string SI = "";
        }
        public class DC
        {
            public class Voltage
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Sets the specified value.
                    /// </summary>
                    /// <param name="value">The value.</param>
                    /// <param name="multipliers">Задает множитель <see cref="Multipliers"/> class.</param>
                    /// <returns></returns>
                    public static string Set(double value, string multipliers = "")
                    {
                        return "CONF:VOLT:DC: " + value.ToString(CultureInfo.GetCultureInfo("en-US")) + multipliers;
                    }
                    /// <summary>
                    /// Команда автоматического выбора придела
                    /// </summary>
                    public const string Auto = "CONF:VOLT:DC AUTO";
                    /// <summary>
                    /// Команда 
                    /// </summary>
                    public const string V1000 = "CONF:VOLT:DC 1000";
                    /// <summary>
                    /// Предел 100 В
                    /// </summary>
                    public const string V100 = "CONF:VOLT:DC 100";
                    /// <summary>
                    /// Предел 10 В
                    /// </summary>
                    public const string V10 = "CONF:VOLT:DC 10";
                    /// <summary>
                    /// Предел 1 В
                    /// </summary>COLT
                    public const string V1 = "CONF:VOLT:DC 1";
                    /// <summary>
                    /// Предел 100 мВ
                    /// </summary>
                    public const string MV100 = "CONF:VOLT:DC 100 MV";
                }


            }
            public class Current
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое знаечние</param>
                    /// <returns>Сформированую команду</returns>
                    public static string Set(double value)
                    {
                        return "CONF:CURR:DC RANG " + value.ToString(CultureInfo.GetCultureInfo("en-US"));
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:CURR:DC:RANG AUTO ON";

                    /// <summary>
                    /// Предел 10 А
                    /// </summary>
                    public const string A10 = "CONF:CURR:DC 10";
                    /// <summary>
                    /// Предел 3 А
                    /// </summary>
                    public const string A3 = "CONF:CURR:DC 3";
                    /// <summary>
                    /// Предел 1 А
                    /// </summary>
                    public const string A1 = "CONF:CURR:DC 1";
                    /// <summary>
                    /// Предел 100 мА
                    /// </summary>COLT
                    public const string M100 = "CONF:CURR:DC 100 MA";
                    /// <summary>
                    /// Предел 1 мА
                    /// </summary>
                    public const string M1 = "CONF:CURR:DC 1 MA";
                    /// <summary>
                    /// Предел 10 мА
                    /// </summary>
                    public const string M10 = "CONF:CURR:DC 10 MA";
                    /// <summary>
                    /// Предел 100 мкА
                    /// </summary>
                    public const string U100 = "CONF:CURR:DC 10 UA";
                }


            }
        }
        public class AC
        {
            /// <summary>
            /// Измерение переменного напряжения
            /// </summary>
            public class Voltage
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое знаечние</param>
                    /// <returns>Сформированую команду</returns>
                    public static string Set(double value)
                    {
                        return "CONF:VOLT:AC " + value.ToString(CultureInfo.GetCultureInfo("en-US"));
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:VOLT:AC AUTO";
                    /// <summary>
                    /// Предел 750 В
                    /// </summary>
                    public const string V750 = "CONF:VOLT:AC 750";
                    /// <summary>
                    /// Предел 100 В
                    /// </summary>
                    public const string V100 = "CONF:VOLT:AC 100";
                    /// <summary>
                    /// Предел 10 В
                    /// </summary>
                    public const string V10 = "CONF:VOLT:AC 10";
                    /// <summary>
                    /// Предел 1 В
                    /// </summary>
                    public const string V1 = "CONF:VOLT:AC 1";
                    /// <summary>
                    /// Предел 100 мВ
                    /// </summary>
                    public const string MV100 = "CONF:VOLT:AC 100 MV";
                }
                /// <summary>
                /// Фильтр
                /// </summary>
                public class Filtr
                {
                    /// <summary>
                    /// Установить фильтр 3 Гц
                    /// </summary>
                    public const string Filtr_3 = "SENS:VOLT:AC:BAND 3";
                    /// <summary>
                    /// Установить фильтр 20 Гц, по умолчанию
                    /// </summary>
                    public const string Filtr_20_Def = "SENS:VOLT:AC:BAND 20";
                    /// <summary>
                    /// Установить фильтр 200 Гц
                    /// </summary>
                    public const string Filtr_200 = "SENS:VOLT:AC:BAND 200";
                }
            }
            /// <summary>
            /// Содержит команды отвечающие за настройку измерения силы тока
            /// </summary>
            public class Current
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое знаечние</param>
                    /// <returns>Сформированую команду</returns>
                    public static string Set(double value)
                    {
                        return "CONF:CURR:AC:RANG " + value.ToString(CultureInfo.GetCultureInfo("en-US"));
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:CURR:DC:RANG AUTO ON";
                    /// <summary>
                    /// Предел 10 А
                    /// </summary>
                    public const string A10 = "CONF:CURR:AC 10";
                    /// <summary>
                    /// Предел 3 А
                    /// </summary>
                    public const string A3 = "CONF:CURR:AC 3";
                    /// <summary>
                    /// Предел 1 А
                    /// </summary>
                    public const string A1 = "CONF:CURR:AC 1";
                    /// <summary>
                    /// Предел 100 мА
                    /// </summary>COLT
                    public const string M100 = "CONF:CURR:AC 100 MA";
                    /// <summary>
                    /// Предел 1 мА
                    /// </summary>
                    public const string M1 = "CONF:CURR:AC 1 MA";
                    /// <summary>
                    /// Предел 10 мА
                    /// </summary>
                    public const string M10 = "CONF:CURR:AC 10 MA";
                    /// <summary>
                    /// Предел 100 мкА
                    /// </summary>
                    public const string U100 = "CONF:CURR:AC 100 UA";
                }
                /// <summary>
                /// Фильтр
                /// </summary>
                public class Filtr
                {
                    /// <summary>
                    /// Установить фильтр 3 Гц
                    /// </summary>
                    public const string Filtr_3 = "SENS:CURR:AC:BAND 3";
                    /// <summary>
                    /// Установить фильтр 20 Гц, по умолчанию
                    /// </summary>
                    public const string Filtr_20_Def = "SENS:CURR:AC:BAND 20";
                    /// <summary>
                    /// Установить фильтр 200 Гц
                    /// </summary>
                    public const string Filtr_200 = "SENS:CURR:AC:BAND 200";
                }
            }
        }
        /// <summary>
        /// Содержит команды отвечающие за настройку измерения частоты
        /// </summary>
        public class Frequency
        {
            /// <summary>
            /// Содержит перечнь комант отвечающих за выбор предела
            /// </summary>
            public class Range
            {
                /// <summary>
                /// Автовыбор предела включение измерения частоты
                /// </summary>
                public const string AutoAndOn = "CONF:FREQ";
                /// <summary>
                /// Предел 1 В
                /// </summary>
                public const string V1 = "SENS:FREQ:VOLT:RANG 1";
                /// <summary>
                /// Предел 10 В
                /// </summary>
                public const string V10 = "SENS:FREQ:VOLT:RANG 10";
                /// <summary>
                /// Предел 100 В
                /// </summary>
                public const string V100 = "SENS:FREQ:VOLT:RANG 100";
                /// <summary>
                /// Предел 1000 В
                /// </summary>
                public const string V1000 = "SENS:FREQ:VOLT:RANG 1000";
                /// <summary>
                /// Предел 100 мВ
                /// </summary>
                public const string MV100 = "SENS:FREQ:VOLT:RANG 0.1";
            }
        }
        /// <summary>
        /// Содержит команды отвечающие за настройку измерения сопротивления
        /// </summary>
        public class Resistance
        {
            /// <summary>
            /// Обычный режим измерения сопротиления
            /// </summary>
            public class Normal
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:RES";
                    /// <summary>
                    /// Предел 100 Ом
                    /// </summary>
                    public const string R100 = "CONF:RES 100";
                    /// <summary>
                    /// Предел 1 кОм
                    /// </summary>
                    public const string RK1 = "CONF:RES 1 KOhm";
                    /// <summary>
                    /// Предел 10 кОм
                    /// </summary>
                    public const string RK10 = "CONF:RES 10 KOhm";
                    /// <summary>
                    /// Предел 100 кОм
                    /// </summary>
                    public const string RK100 = "CONF:RES 100 KOhm";
                    /// <summary>
                    /// Предел 1 MОм
                    /// </summary>
                    public const string RM1 = "CONF:RES 1 MOhm";
                    /// <summary>
                    /// Предел 10 MОм
                    /// </summary>
                    public const string RM10 = "CONF:RES 10 MOhm";
                    /// <summary>
                    /// Предел 100 MОм
                    /// </summary>
                    public const string RM100 = "CONF:RES 100 MOhm";
                    /// <summary>
                    /// Предел 1 ГОм
                    /// </summary>
                    public const string RM1000 = "CONF:RES 1 GOhm";
                }
            }
            /// <summary>
            /// 4х проводное измерение
            /// </summary>
            public class TrueOhm
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public class Range
                {
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "CONF:FRES";
                    /// <summary>
                    /// Предел 100 Ом
                    /// </summary>
                    public const string R100 = "CONF:FRES 100";
                    /// <summary>
                    /// Предел 1 кОм
                    /// </summary>
                    public const string RK1 = "CONF:FRES 1 KOhm";
                    /// <summary>
                    /// Предел 10 кОм
                    /// </summary>
                    public const string RK10 = "CONF:FRES 10 KOhm";
                    /// <summary>
                    /// Предел 100 кОм
                    /// </summary>
                    public const string RK100 = "CONF:FRES 100 KOhm";
                    /// <summary>
                    /// Предел 1 MОм
                    /// </summary>
                    public const string RM1 = "CONF:FRES 1 MOhm";
                    /// <summary>
                    /// Предел 10 MОм
                    /// </summary>
                    public const string RM10 = "CONF:FRES 10 MOhm";
                    /// <summary>
                    /// Предел 100 MОм
                    /// </summary>
                    public const string RM100 = "CONF:FRES 100 MOhm";
                    /// <summary>
                    /// Предел 1 ГОм
                    /// </summary>
                    public const string RM1000 = "CONF:FRES 1 GOhm";
                }

            }
        }
        /// <summary>
        /// Содержит команды для настроек измерения емкости
        /// </summary>
        public class Capacitance
        {
            /// <summary>
            /// Содержит набор команд для установки необходимого диапазона
            /// </summary>
            public class Range
            {

                /// <summary>
                /// Автовыбор предела включение измерения частоты
                /// </summary>
                public const string AutoAndOn = "CONF:CAP";
                /// <summary>
                /// Предел 1 нФ
                /// </summary>
                public const string nF1 = "CONF:CAP 1 NF";
                /// <summary>
                /// Предел 10 нФ
                /// </summary>
                public const string nF10 = "CONF:CAP 10 NF";
                /// <summary>
                /// Предел 100 нФ
                /// </summary>
                public const string nF100 = "CONF:CAP 100 NF";
                /// <summary>
                /// Предел 1 мкФ
                /// </summary>
                public const string uF1 = "CONF:CAP 1 UF";
                /// <summary>
                /// Предел 10 мкФ
                /// </summary>
                public const string uF10 = "CONF:CAP 10 UF";
                /// <summary>
                /// Предел 100 мкФ
                /// </summary>
                public const string uF100 = "CONF:CAP 100 UF";
            }
        }
        /// <summary>
        /// Прробразут данные в нужных единицах
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="Mult">The mult.</param>
        /// <returns></returns>
        public double DataPreparationAndConvert(string date, string Mult = "")
        {
            string[] Value = date.Split(',');
            double[] a = new double[Value.Length];
            for (int i = 0; i < Value.Length; i++)
            {
                a[i] = Convert(Value[i], Mult);
            }
            return Statistics.Mean(a) < 0 ? Statistics.RootMeanSquare(a) * -1 : Statistics.RootMeanSquare(a);

        }
        private double Convert(string date, string Mult = "")
        {

            double[] dDate = new double[2];
            string[] Value = date.Replace(".", ",").Split('E');
            dDate[0] = System.Convert.ToDouble(Value[0]);
            dDate[1] = System.Convert.ToDouble(Value[1]);
            switch (Mult)
            {
                case " M":
                    return (dDate[0] * Math.Pow(10, dDate[1])) * 1000;
                case " N":
                    return (dDate[0] * Math.Pow(10, dDate[1])) * 1000000000;
                case " U":
                    return (dDate[0] * Math.Pow(10, dDate[1])) * 1000000;
                case " K":
                    return (dDate[0] * Math.Pow(10, dDate[1])) / 1000;
                case " MA":
                    return (dDate[0] * Math.Pow(10, dDate[1])) / 1000000;
                default:
                    return (dDate[0] * Math.Pow(10, dDate[1]));
            }
        }
    }
}
