// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Main_Claibr: IeeeBase
    {
        public Main_Claibr():base()
        {

        }
       
        /// <summary>
        /// Содержит доступные множители
        /// </summary>
        public struct Multipliers
        {
            /// <summary>
            /// The mega
            /// </summary>
            public const string mega = " MA";
            /// <summary>
            /// The mili
            /// </summary>
            public const string mili = " M";
            /// <summary>
            /// The nano
            /// </summary>
            public const string nano = " N";
            /// <summary>
            /// The kilo
            /// </summary>
            public const string kilo = " K";
            /// <summary>
            /// The micro
            /// </summary>
            public const string micro = " U";
            /// <summary>
            /// The si
            /// </summary>
            public const string SI = "";
        }
        /// <summary>
        /// Содержит команды позволяющие задовать настройки выхода
        /// </summary>
        public struct Out
        {
            /// <summary>
            /// Содержит команды позволяющие устанавливать значения на выходе
            /// </summary>
            public struct Set
            {
                /// <summary>
                /// Содержит команды позволяющие устанавливать напржение на выходе
                /// </summary>
                public struct Voltage
                {
                    /// <summary>
                    /// Содержит команды позволяющие устанавливть на выходе переменное напряжение
                    /// </summary>
                    public struct DC
                    {
                        /// <summary>
                        /// Генерирует команду становки постоянного напряжения с указаным значением
                        /// </summary>
                        /// <param name="value">The value со</param>
                        /// <param name="Multipliers">Множитель устанавливаемой еденицы <смотреть cref="Multipliers"/> struct.</param>
                        /// <returns>Сформированую команду</returns>
                        public static string SetValue(double value, string Multipliers = "")
                        {
                            return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + Multipliers + "V" + ", 0 HZ";
                        }

                    }
                    /// <summary>
                    /// Содержит набор команд по установке переменного напряжения
                    /// </summary>
                    public struct AC
                    {
                        /// <summary>
                        /// Генерирует команду становки переменного напряжения с указаным значением
                        /// </summary>
                        /// <param name="voltage">The voltage.</param>
                        /// <param name="hertz">The hertz.</param>
                        /// <param name="MultipliersVolt">Множитель устанавливаемой еденицы напряжения <смотреть cref="Multipliers"/> struct.</param>
                        /// <param name="MultipliersHerts">Множитель устанавливаемой еденицы частоты <смотреть cref="Multipliers"/> struct.</param>
                        /// <returns>Сформированую команду</returns>
                        public static string SetValue(double voltage, double hertz, string MultipliersVolt, string MultipliersHerts)
                        {
                            return "OUT " + voltage.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MultipliersVolt + "V" + ", " + hertz.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MultipliersHerts + "HZ";
                        }
                        /// <summary>
                        /// Содержит команды установки формы генерируемого напряжения
                        /// </summary>
                        public struct Wave
                        {
                            /// <summary>
                            /// Команда генерации меандра
                            /// </summary>
                            public const string SQUARE = "WAVE SQUARE";
                            /// <summary>
                            /// Команда генерации синусойды
                            /// </summary>
                            public const string SINE = "WAVE SINE";
                            public const string TRI = "WAVE TRI";
                            public const string TRUNCS = "WAVE TRUNCS,";
                        }
                    }

                }
                /// <summary>
                /// Содержит набор команд по установке тока
                /// </summary>
                public struct Current
                {
                    /// <summary>
                    /// Содержит набор команд по установке постоянного тока
                    /// </summary>
                    public struct DC
                    {
                        /// <summary>
                        /// Генерирует команду установки постоянного тока указной величины
                        /// </summary>
                        /// <param name="value">Устанавливаемое значение.</param>
                        /// <param name="Multipliers">Множитель устанавливаемой еденицы <смотреть cref="Multipliers"/> struct.</param>
                        /// <returns>Сформированую команду</returns>
                        public static string SetValue(double value, string Multipliers = "")
                        {
                            return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + Multipliers + "A" + ", 0 HZ";
                        }

                    }
                    /// <summary>
                    /// Содержит набор команд по установке переменного тока
                    /// </summary>
                    public struct AC
                    {
                        /// <summary>
                        /// Генерирует команду установки переменного тока указной величины и частоты
                        /// </summary>
                        /// <param name="voltage">Устанавливаемое значение напряжения</param>
                        /// <param name="hertz">Устанавливаемое значение частоты</param>
                        /// <param name="MultipliersVolt">Множитель устанавливаемой еденицы напряжения <смотреть cref="Multipliers"/> struct.</param>
                        /// <param name="MultipliersHerts">Множитель устанавливаемой еденицы частоты <смотреть cref="Multipliers"/> struct.</param>
                        /// <returns>Сформированую команду</returns>
                        public static string SetValue(double voltage, double hertz, string MultipliersVolt, string MultipliersHerts)
                        {
                            return "OUT " + voltage.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MultipliersVolt + "A" + ", " + hertz.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + MultipliersHerts + "HZ";
                        }
                    }
                }
                /// <summary>
                /// Содержит набор команд по установке сопротивления
                /// </summary>
                public struct Resistance
                {
                    /// <summary>
                    /// Установить сопротивление
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <param name="Multipliers">Множитель</param>
                    /// <returns></returns>
                    public static string SetValue(double value, string Multipliers = "")
                    {
                        return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + Multipliers + "OHM";
                    }
                }
                /// <summary>
                /// Активирует или дезактивирует компенсацию импеданса при 2-проводном или 4-проводном подключении. Компенсация в режиме воспроизведения сопротивления доступна для сопротивлений величиной менее 110 кΩ. Компенсация в режиме воспроизведения емкости доступна для емкостей величиной не менее 110 нФ. 
                /// </summary>
                public struct ZCOMP
                {
                    /// <summary>
                    /// компенсация 2х проводная
                    /// </summary>
                    public const string Wire2 = "ZCOMP WIRE2";
                    /// <summary>
                    /// компенсация 4х проводная
                    /// </summary>
                    public const string Wire4 = "ZCOMP WIRE4";
                    /// <summary>
                    /// Отключает компенсацию
                    /// </summary>
                    public const string WireNONE = "ZCOMP NONE";
                }
                /// <summary>
                /// Содержит набор команд по установке емкости
                /// </summary>
                public struct Capacitance
                {
                    /// <summary>
                    /// Установить емкость
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <param name="Multipliers">Множитель</param>
                    /// <returns></returns>
                    public static string SetValue(double value, string Multipliers = "")
                    {
                        return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + Multipliers + "F";
                    }
                }
                /// <summary>
                /// Содержит набор команд по установке температуры
                /// </summary>
                public struct Temperature
                {
                    /// <summary>
                    /// Содержит достумные виды преобразователей
                    /// </summary>
                    public struct TypeTermocuple
                    {
                        public const string B = "B";
                        public const string C = "C";
                        public const string E = "E";
                        public const string J = "J";
                        public const string K = "K";
                        public const string N = "N";
                        public const string R = "R";
                        public const string S = "S";
                        public const string T = "T";
                        public const string LinOut10mV = "X";
                        public const string Himidity = "Y";
                        public const string LinOut1mV = "Z";
                    }
                    /// <summary>
                    /// Установка выбранноего типа преобразователя
                    /// </summary>
                    /// <param name="type">The type.</param>
                    /// <returns></returns>
                    public static string SetTermocuple(string type)
                    {
                        return "TC_TYPE " + type;
                    }
                    /// <summary>
                    /// Установить знаечние температуры
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <returns></returns>
                    public static string SetValue(double value)
                    {
                        return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "CEL";
                    }
                }
            }
            /// <summary>
            /// СОдержит доступные состояние выхода
            /// </summary>
            public struct State
            {
                /// <summary>
                /// Включить выход
                /// </summary>
                public const string ON = "OPER";
                /// <summary>
                /// Выключить выход
                /// </summary>
                public const string OFF = "STBY";
            }
            
            /// <summary>
            /// Замыкает или размыкает внутренний контакт между защитным заземлением и заземлением корпуса (шасси). 
            /// </summary>
            public struct EARTH
             {  /// <summary>
                /// подключить клемму передней панели LO к заземлению шасси
                /// </summary>
                public const string ON = "EARTH TIED";
                /// <summary>
                /// отсоединить клемму передней панели LO от заземления шасси
                /// </summary>
                public const string OFF = "EARTH OPEN";
             }

        }
        /// <summary>
        /// Преобразует полученные данные с калибратора в значение
        /// </summary>
        /// <param name="date">Значение с калибратора</param>
        /// <param name="Mult">Множитель с которым надо вернуть значение</param>
        /// <param name="MultFrec">The mult frec.</param>
        /// <returns>
        /// Возвращает массив со значением, смещением, частотой
        /// </returns>
        public static double[] ConvertDate(string date, string Mult = "", string MultFrec = "")
        {
            string local;
            string[] dValue;
            string[] Value;
            double[] dDate = new double[2];
            double[] Data = new double[5];
            dValue = date.Split(',');
            for (int i = 0; i < dValue.Length; i++)
            {
                try
                {

                    Value = dValue[i].Split('E');
                    Value[0] = Value[0].Replace(".", ",");
                    dDate[0] = Convert.ToDouble(Value[0]);
                    dDate[1] = Convert.ToDouble(Value[1]);
                    if (i == 4)
                    {
                        local = MultFrec;
                    }
                    else
                    {
                        local = Mult;
                    }
                    switch (local)
                    {
                        case " M":
                            Data[i] = (dDate[0] * Math.Pow(10, dDate[1])) * 1000;
                            break;
                        case " U":
                            Data[i] = (dDate[0] * Math.Pow(10, dDate[1])) * 1000000;
                            break;
                        case " N":
                            Data[i] = (dDate[0] * Math.Pow(10, dDate[1])) * Math.Pow(10, 9);
                            break;
                        case " K":
                            Data[i] = (dDate[0] * Math.Pow(10, dDate[1])) / 1000;
                            break;
                        case " MA":
                            Data[i] = (dDate[0] * Math.Pow(10, dDate[1])) / 1000000;
                            break;
                        default:
                            Data[i] = (dDate[0] * Math.Pow(10, dDate[1]));
                            break;
                    }
                }
                catch (Exception)
                {

                }

            }
            return Data;
        }
    }
}
