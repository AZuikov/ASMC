// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using AP.Reports.Utils;

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
        public enum Multipliers
        {
            /// <summary>
            /// The mega
            /// </summary>
           [StringValue(" MA")] mega,
            /// <summary>
            /// The mili
            /// </summary>
            [StringValue(" M")] mili,
            /// <summary>
            /// The nano
            /// </summary>
            [StringValue(" N")] nano,
            /// <summary>
            /// The kilo
            /// </summary>
           [StringValue(" K")] kilo,
            /// <summary>
            /// The micro
            /// </summary>
           [StringValue(" U")] micro,
            /// <summary>
            /// The si
            /// </summary>
           [StringValue("")] SI 
        }

        /// <summary>
        /// Множители (приставки) для обозначения частоты
        /// </summary>
        public enum HerzMultiplers
        {
            [StringValue(" M")] mili,
            [StringValue("")] SI,
            [StringValue(" K")] kilo,
            [StringValue(" MA")] mega,
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
                        public static string SetValue(decimal value, Multipliers inMultipliers)
                        {
                            return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inMultipliers + "V" + ", 0 HZ";
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
                        /// <param name="inMultipliers">Множитель устанавливаемой еденицы напряжения <смотреть cref="Multipliers"/> struct.</param>
                        /// <param name="MultipliersHerts">Множитель устанавливаемой еденицы частоты <смотреть cref="Multipliers"/> struct.</param>
                        /// <returns>Сформированую команду</returns>

                        //todo: множитель для частоты нужно уточнить в документации и сделать перечисление
                        public static string SetValue(decimal voltage, decimal hertz, Multipliers inMultipliers, HerzMultiplers inHerzMultiplers)
                        {
                            return "OUT " + voltage.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inMultipliers + "V" + ", " + hertz.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inHerzMultiplers + "HZ";
                        }
                        /// <summary>
                        /// Содержит команды установки формы генерируемого напряжения
                        /// </summary>
                        public enum Wave
                        {
                            /// <summary>
                            /// Команда генерации меандра
                            /// </summary>
                            [StringValue("WAVE SQUARE")] SQUARE ,
                            /// <summary>
                            /// Команда генерации синусойды
                            /// </summary>
                            [StringValue("WAVE SINE")] SINE,
                            [StringValue("WAVE TRI")] TRI,
                            //тут точно нужна запятая в значении???
                            [StringValue("WAVE TRUNCS,")] TRUNCS 
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
                        public static string SetValue(double value, Multipliers inMultipliers)
                        {
                            return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inMultipliers + "A" + ", 0 HZ";
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
                        public static string SetValue(double voltage, double hertz, Multipliers inMultipliers, HerzMultiplers inHerzMultiplers)
                        {
                            return "OUT " + voltage.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inMultipliers + "A" + ", " + hertz.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inHerzMultiplers + "HZ";
                        }
                    }
                }
                /// <summary>
                /// Содержит набор команд по установке сопротивления
                /// </summary>
                public struct Resistance
                {
                    public enum ZCOMP
                    {
                        /// <summary>
                        /// компенсация 2х проводная
                        /// </summary>
                        [StringValue("ZCOMP WIRE2")] Wire2,
                        /// <summary>
                        /// компенсация 4х проводная
                        /// </summary>
                        [StringValue("ZCOMP WIRE4")] Wire4,
                        /// <summary>
                        /// Отключает компенсацию
                        /// </summary>
                        [StringValue("ZCOMP NONE")] WireNONE
                    }
                    /// <summary>
                    /// Установить сопротивление
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <param name="Multipliers">Множитель</param>
                    /// <returns></returns>
                    public static string SetValue(double value, Multipliers inMultipliers)
                    {
                        return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inMultipliers + "OHM";
                    }
                }
                /// <summary>
                /// Активирует или дезактивирует компенсацию импеданса при 2-проводном или 4-проводном подключении. Компенсация в режиме воспроизведения сопротивления доступна для сопротивлений величиной менее 110 кΩ. Компенсация в режиме воспроизведения емкости доступна для емкостей величиной не менее 110 нФ. 
                /// </summary>
                
               
                /// <summary>
                /// Содержит набор команд по установке емкости
                /// </summary>
                public struct Capacitance
                {
                    public enum ZCOMP
                    {
                        /// <summary>
                        /// компенсация 2х проводная
                        /// </summary>
                        [StringValue("ZCOMP WIRE2")] Wire2,
                        /// <summary>
                        /// Отключает компенсацию
                        /// </summary>
                        [StringValue("ZCOMP NONE")] WireNONE
                    }
                    /// <summary>
                    /// Установить емкость
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <param name="Multipliers">Множитель</param>
                    /// <returns></returns>
                    public static string SetValue(double value,  Multipliers inMultipliers)
                    {
                        return "OUT " + value.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US")) + inMultipliers + "F";
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
                    public enum TypeTermocuple
                    {
                        [StringValue("B")] B,
                        [StringValue("C")] C,
                        [StringValue("E")] E,
                        [StringValue("J")] J,
                        [StringValue("K")] K,
                        [StringValue("N")] N,
                        [StringValue("R")] R,
                        [StringValue("S")] S,
                        [StringValue("T")] T,
                        [StringValue("X")] LinOut10mV,
                        [StringValue("Y")] Himidity,
                        [StringValue("Z")] LinOut1mV 
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
            public enum State
            {
                /// <summary>
                /// Включить выход
                /// </summary>
                [StringValue("OPER")] ON,
                /// <summary>
                /// Выключить выход
                /// </summary>
                [StringValue("STBY")] OFF
            }
            
            /// <summary>
            /// Замыкает или размыкает внутренний контакт между защитным заземлением и заземлением корпуса (шасси). 
            /// </summary>
            public enum EARTH
             {  /// <summary>
                /// подключить клемму передней панели LO к заземлению шасси
                /// </summary>
                [StringValue("EARTH TIED")] ON,
                /// <summary>
                /// отсоединить клемму передней панели LO от заземления шасси
                /// </summary>
                [StringValue("EARTH OPEN")]OFF

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
