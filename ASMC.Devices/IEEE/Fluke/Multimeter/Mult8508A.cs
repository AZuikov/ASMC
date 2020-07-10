// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Threading;

namespace ASMC.Devices.IEEE.Fluke.Multimeter
{
    public class Mult8508A : IeeeBase
    {

        public Mult8508A() : base()
        {
            DeviseType = "t8508A";
        }
        public Mult8508A(string connect) : this()
        {
            Stringconection = connect;
        }

        /// <summary>
        ///  /// Запрос на получение значения (U, I, R)
        /// </summary>
        public string QueryValue()
        {
            WriteLine("RDG?");
            return ReadString();
        }
        /// <summary>
        /// Запрос на получение значения частоты
        /// </summary>
        public void QueryFrequency()
        {
            WriteLine("FREQ?");
        }
        /// <summary>
        /// Содержит доступные множители
        /// </summary>
        public enum Multipliers
        {
            [StringValue(" MA")]
            Mega,
            [StringValue(" M")]
            Mili,
            [StringValue(" N")]
            Nano,
            [StringValue(" K")]
            Kilo,
            [StringValue(" U")]
            Micro,
            [StringValue("")]
            Si
        }
        /// <summary>
        /// Количество значащих знаков
        /// </summary>
        public enum DigValue
        {
            Dig_5 = 5,
            Dig_6 = 6,
            Dig_7 = 7,
            Dig_8 = 8
        }
        /// <summary>
        /// Состояние
        /// </summary>
        public enum State
        {
            [StringValue("ON")]
            ON,
            [StringValue("OFF")]
            Off
        }
        /// <summary>
        /// Здначение допустимые для фильтра
        /// </summary>
        public enum FiltValue
        {
            [StringValue("100HZ")]
            HZ100,
            [StringValue("40HZ")]
            HZ40,
            [StringValue("10HZ")]
            HZ10,
            [StringValue("1HZ")]
            HZ1,

        }
        public struct DC
        {
            public struct Voltage
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {

                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult= Multipliers.Si)
                    {
                        return "DCV " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"))+ MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "DCV AUTO";
                }                
                /// <summary>
                /// Фильтр
                /// </summary>
                /// <param name="st">Включить или выключить</param>
                /// <returns></returns>
                public static string SetFiltr(State st)
                {
                    return "DCV FILT_" + MyEnum.GetStringValue(st);
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "DCV RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Включает или выключает быстрое преобразование
                /// </summary>
                /// <param name="st">Состояние</param>
                /// <returns></returns>
                public static string EnableFastConvert(State st)
                {
                    return "DCV FAST_" + MyEnum.GetStringValue(st);
                }

            }
            public struct Current
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {
                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult = Multipliers.Si)
                    {
                        return "DCI " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "DCI AUTO";
                }
                /// <summary>
                /// Фильтр
                /// </summary>
                /// <param name="st">Включить или выключить</param>
                /// <returns></returns>
                public static string SetFiltr(State st)
                {
                    return "DCI FILT_" + MyEnum.GetStringValue(st);
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "DCI RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Включает или выключает быстрое преобразование
                /// </summary>
                /// <param name="st">Состояние</param>
                /// <returns></returns>
                public static string EnableFastConvert(State st)
                {
                    return "DCI FAST_" + MyEnum.GetStringValue(st);
                }
            }
        }
        public struct AC
        {
            public enum Cut_off_state
            {
                /// <summary>
                /// Режим отсечки DC
                /// </summary>
                [StringValue("ACCP")]
                ON,
                /// <summary>
                /// Для частот ниже 40 Гц
                /// </summary>
                [StringValue(" DCCP")]
                OFF,
            }
            /// <summary>
            /// Измерение переменного напряжения
            /// </summary>
            public struct Voltage
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {

                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult = Multipliers.Si)
                    {
                        return "ACV " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "ACV AUTO";
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "ACV RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Устанавливает фильтр
                /// </summary>
                /// <param name="fv">Значения фильра</param>
                /// <returns></returns>
                public static string SetFiltr(FiltValue fv)
                {
                    return "ACV FILT" + MyEnum.GetStringValue(fv);
                }
                /// <summary>
                /// Режим измерения c отсечкой или без
                /// </summary>
                /// <param name="cos">Включить или выключить отсечку</param>
                /// <returns></returns>
                public static string EnableCutOff(Cut_off_state cos)
                {
                    return "ACV " + MyEnum.GetStringValue(cos);
                }
                /// <summary>
                /// Компенсация от температуры
                /// </summary>
                /// <param name="st">The st.</param>
                /// <returns></returns>
                public static string EnableTfer(State st)
                {
                    return "ACV TFER_" + MyEnum.GetStringValue(st);
                }
            }
            public struct Current
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {

                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult = Multipliers.Si)
                    {
                        return "ACI " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "ACI AUTO";
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "ACI RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Устанавливает фильтр
                /// </summary>
                /// <param name="fv">Значения фильра</param>
                /// <returns></returns>
                public static string SetFiltr(FiltValue fv)
                {
                    return "ACI FILT" + MyEnum.GetStringValue(fv);
                }
                /// <summary>
                /// Режим измерения c отсечкой или без
                /// </summary>
                /// <param name="cos">Включить или выключить отсечку</param>
                /// <returns></returns>
                public static string EnableCutOff(Cut_off_state cos)
                {
                    return "ACI " + MyEnum.GetStringValue(cos);
                }
            }
        }
        public struct Resistance
        {
            /// <summary>
            /// Обычный режим измерения сопротиления
            /// </summary>
            public struct Normal
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {
                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult = Multipliers.Si)
                    {
                        return "OHMS " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "OHMS AUTO";
                }
                /// <summary>
                /// Фильтр
                /// </summary>
                /// <param name="st">Включить или выключить</param>
                /// <returns></returns>
                public static string SetFiltr(State st)
                {
                    return "OHMS FILT_" + MyEnum.GetStringValue(st);
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "OHMS RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Включает или выключает быстрое преобразование
                /// </summary>
                /// <param name="st">Состояние</param>
                /// <returns></returns>
                public static string EnableFastConvert(State st)
                {
                    return "OHMS FAST_" + MyEnum.GetStringValue(st);
                }
            }
            public struct HV
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {
                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult = Multipliers.Si)
                    {
                        return "HIV_OHMS " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "HIV_OHMS AUTO";
                }
                /// <summary>
                /// Фильтр
                /// </summary>
                /// <param name="st">Включить или выключить</param>
                /// <returns></returns>
                public static string SetFiltr(State st)
                {
                    return "HIV_OHMS FILT_" + MyEnum.GetStringValue(st);
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "HIV_OHMS RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Включает или выключает быстрое преобразование
                /// </summary>
                /// <param name="st">Состояние</param>
                /// <returns></returns>
                public static string EnableFastConvert(State st)
                {
                    return "HIV_OHMS FAST_" + MyEnum.GetStringValue(st);
                }                
            }
            public struct TrueOhm
            {
                /// <summary>
                /// Определяет предел
                /// </summary>
                public struct Range
                {
                    /// <summary>
                    ///  Установка предела от ожидаемого значения
                    /// </summary>
                    /// <param name="value">Ожидаемое значение</param>
                    /// <param name="mult">Множитель</param>
                    /// <returns></returns>
                    public static string SetRangeForValue(double value, Multipliers mult = Multipliers.Si)
                    {
                        return "TRUE_OHMS " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MyEnum.GetStringValue(mult);
                    }
                    /// <summary>
                    /// Автовыбор предела
                    /// </summary>
                    public const string Auto = "TRUE_OHMS AUTO";
                }
                /// <summary>
                /// Фильтр
                /// </summary>
                /// <param name="st">Включить или выключить</param>
                /// <returns></returns>
                public static string SetFiltr(State st)
                {
                    return "TRUE_OHMS FILT_" + MyEnum.GetStringValue(st);
                }
                /// <summary>
                /// Устанавливает количество значищих знаков
                /// </summary>
                /// <param name="dv">Количество знаков</param>
                /// <returns></returns>
                public static string SetDig(DigValue dv)
                {
                    return "TRUE_OHMS RESL" + ((int)dv).ToString();
                }
                /// <summary>
                /// Включает или выключает быстрое преобразование
                /// </summary>
                /// <param name="st">Состояние</param>
                /// <returns></returns>
                public static string EnableFastConvert(State st)
                {
                    return "TRUE_OHMS FAST_" + MyEnum.GetStringValue(st);
                }
                /// <summary>
                /// Преобразует полученое значение в число в указаных еденицах
                /// </summary>
                /// <param name="date">Данный для преобразование</param>
                /// <param name="Mult">Множитель, по имолчанию в СИ</param>
                /// <returns>Возвращает значение в число в указаных еденицах</returns>
                public static double ConvertDate(string date, Multipliers Mult = Multipliers.Si)
                {
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    double result=0;
                    double[] dDate = new double[2];
                    string[] Value = date.Split('E');
                    //Value[0] = Value[0].Replace(".", ",");
                    dDate[0] = Convert.ToDouble(Value[0]);
                    dDate[1] = Convert.ToDouble(Value[1]);
                    switch (Mult)
                    {
                        case Multipliers.Mega:
                            result= (dDate[0] * Math.Pow(10, dDate[1])) * 1E-6;
                            break;
                        case Multipliers.Mili:
                            result = (dDate[0] * Math.Pow(10, dDate[1])) * 1E3;
                            break;
                        case Multipliers.Nano:
                            result = (dDate[0] * Math.Pow(10, dDate[1])) * 1E9;
                            break;
                        case Multipliers.Kilo:
                            result = (dDate[0] * Math.Pow(10, dDate[1])) * 1E-3;
                            break;
                        case Multipliers.Micro:
                            result = (dDate[0] * Math.Pow(10, dDate[1])) * 1E6;
                            break;
                        case Multipliers.Si:
                            result = dDate[0] * Math.Pow(10, dDate[1]);
                            break;                        
                    }
                    return result;
                }
            }
        }
        /// <summary>
        /// Преобразует полученое значение в число в указаных еденицах
        /// </summary>
        /// <param name="date">Данный для преобразование</param>
        /// <param name="Mult">Множитель, по имолчанию в СИ</param>
        /// <returns>Возвращает значение в число в указаных еденицах</returns>
        public static double ConvertDate(string date, Multipliers Mult =  Multipliers.Si)
        {
            double[] dDate = new double[2];
            string[] Value = date.Split('E');
            dDate[0] = Convert.ToDouble(Value[0]);
            dDate[1] = Convert.ToDouble(Value[1]);
            double val = 0;

            switch (Mult)
            {
                case Multipliers.Mega:
                    val = (dDate[0] * Math.Pow(10, dDate[1]))* 1E-6;
                    break;
                case Multipliers.Mili:
                    val = (dDate[0] * Math.Pow(10, dDate[1])) * 1E3;
                    break;
                case Multipliers.Nano:
                    val = (dDate[0] * Math.Pow(10, dDate[1])) * 1E9;
                    break;
                case Multipliers.Kilo:
                    val = (dDate[0] * Math.Pow(10, dDate[1])) * 1E-3;
                    break;
                case Multipliers.Micro:
                    val = (dDate[0] * Math.Pow(10, dDate[1])) * 1E6;
                    break;
                case Multipliers.Si:
                    val = (dDate[0] * Math.Pow(10, dDate[1]));
                    break;
            }
            return val;
        }
    }
}
