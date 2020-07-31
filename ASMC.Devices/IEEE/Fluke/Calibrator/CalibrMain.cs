﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using AP.Reports.Utils;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public abstract class CalibrMain: IeeeBase
    {
        protected CalibrMain()
        {
            Out = new COut(this);
        }

        public COut Out { get; }

        /// <summary>
        /// Содержит команды позволяющие задовать настройки выхода
        /// </summary>
        public class COut
        {
            public ICommand[] HerzRanges { get; protected internal set; }
           
            private readonly CalibrMain _calibrMain;

            public COut(CalibrMain calibrMain)
            {
                this._calibrMain = calibrMain;
                Set = new CSet(calibrMain);
               
            }

            public CalibrMain SetOutput(State state)
            {
                _calibrMain.WriteLine(state.GetStringValue());
                return _calibrMain;
            }
            public CSet Set { get;  }

            /// <summary>
            /// Содержит команды позволяющие устанавливать значения на выходе
            /// </summary>
            public class CSet
            {
                private CalibrMain _calibrMain;

                public CSet(CalibrMain calibrMain)
                {
                    this._calibrMain = calibrMain;
                    Capacitance = new CCapacitance(calibrMain);
                    Resistance = new CResistance(calibrMain);
                    Voltage= new CVoltage(calibrMain);
                    Current = new CCurrent(calibrMain);
                }

                public CVoltage Voltage { get; }

                /// <summary>
                /// Содержит команды позволяющие устанавливать напржение на выходе
                /// </summary>
                public class CVoltage
                {
                   
                    private CalibrMain _calibrMain;

                    public CVoltage(CalibrMain calibrMain)
                    {
                        this._calibrMain = calibrMain;
                        Ac = new CAc(calibrMain);
                        Dc = new CDc(calibrMain);
                    }

                    public CDc Dc { get; }

                    /// <summary>
                    /// Содержит команды позволяющие устанавливть на выходе переменное напряжение
                    /// </summary>
                    public class CDc :HelpIeeeBase
                    {
                        private readonly CalibrMain _calibrMain;

                        public CDc(CalibrMain calibrMain)
                        {
                            this._calibrMain = calibrMain;
                        }

                        /// <summary>
                        /// Генерирует команду становки постоянного напряжения с указаным значением
                        /// </summary>
                        /// <param name="value">The value со</param>
                        /// <param name = "mult"></param>
                        /// <returns>Сформированую команду</returns>
                        public CalibrMain SetValue(decimal value, Multipliers mult = Devices.Multipliers.None)
                        {
                            _calibrMain.WriteLine($@"OUT {JoinValueMult(value, mult)}V, 0{ JoinValueMult((double)0, Devices.Multipliers.None)}HZ");
                            return _calibrMain;
                        }

                    }
                    public CAc Ac
                    {
                        get;
                    }
                    /// <summary>
                    /// Содержит набор команд по установке переменного напряжения
                    /// </summary>
                    public class CAc : HelpIeeeBase
                    {
                        private readonly CalibrMain _calibrMain;

                        public CAc(CalibrMain calibrMain)
                        {
                            this._calibrMain = calibrMain;
                        }

                        /// <summary>
                        /// Генерирует команду становки переменного напряжения с указаным значением
                        /// </summary>
                        /// <param name = "value"></param>
                        /// <param name="hertz">The hertz.</param>
                        /// <param name="voltMult">Множитель устанавливаемой еденицы напряжения <смотреть cref="Multipliers"/> class.</param>
                        /// <param name = "herzMult"></param>
                        /// <returns>Сформированую команду</returns>

                        //todo: множитель для частоты нужно уточнить в документации и сделать перечисление
                        public CalibrMain SetValue(decimal value,decimal hertz, Multipliers voltMult, Multipliers herzMult = Devices.Multipliers.None)
                        {
                            _calibrMain.WriteLine($@"OUT {JoinValueMult(value, voltMult)}V, {JoinValueMult(hertz, herzMult)}HZ");
                            return _calibrMain;
                        }

                        /// <summary>
                        /// Содержит команды установки формы генерируемого напряжения
                        /// </summary>
                        public enum Wave
                        {
                            /// <summary>
                            /// Команда генерации меандра
                            /// </summary>
                            [StringValue("WAVE SQUARE")] Square ,
                            /// <summary>
                            /// Команда генерации синусойды
                            /// </summary>
                            [StringValue("WAVE SINE")] Sine,
                            [StringValue("WAVE TRI")] Tri,
                            [StringValue("WAVE TRUNCS")] Truncs 
                        }
                    }

                }
                public CCurrent Current
                {
                    get;
                }
                /// <summary>
                /// Содержит набор команд по установке тока
                /// </summary>
                public class CCurrent
                {
                    private readonly CalibrMain _calibrMain;

                    public CCurrent(CalibrMain calibrMain)
                    {
                        this._calibrMain = calibrMain;
                        Ac = new CAc(calibrMain);
                        Dc = new CDc(calibrMain);
                    }

                    public CDc Dc { get; }
                    public CAc Ac { get; }
                    /// <summary>
                    /// Содержит набор команд по установке постоянного тока
                    /// </summary>
                    public class CDc : HelpIeeeBase
                    {
                        private readonly CalibrMain _calibrMain;

                        public CDc(CalibrMain calibrMain)
                        {
                            this._calibrMain = calibrMain;
                        }
                        /// <summary>
                        /// Генерирует команду установки постоянного тока указной величины
                        /// </summary> 
                        public CalibrMain SetValue(decimal value, Multipliers mult= Devices.Multipliers.None)
                        {
                            _calibrMain.WriteLine($@"OUT {JoinValueMult(value, mult)}A 0HZ");
                            return _calibrMain;
                        }

                    }
                    /// <summary>
                    /// Содержит набор команд по установке переменного тока
                    /// </summary>
                    public class CAc:HelpIeeeBase
                    {
                        private readonly CalibrMain _calibrMain;
                                                                                                                                          
                        public CAc(CalibrMain calibrMain)
                        {
                            this._calibrMain = calibrMain;
                        }

                        /// <summary>
                        /// Генерирует команду установки переменного тока указной величины и частоты
                        /// </summary>
                        /// <param name = "value"></param>
                        /// <param name="hertz">Устанавливаемое значение частоты</param>
                        /// <param name = "voltMult"></param>
                        /// <param name = "herzMult"></param>
                        /// <returns>Сформированую команду</returns>
                        public CalibrMain SetValue(decimal value, decimal hertz, Multipliers voltMult, Multipliers herzMult = Devices.Multipliers.None)
                        {
                            _calibrMain.WriteLine($@"OUT {JoinValueMult(value, voltMult)}A, {JoinValueMult(hertz, herzMult)}HZ");
                            return _calibrMain;
                        }
                    }
                }


                public CResistance Resistance { get; }
                /// <summary>
                /// Содержит набор команд по установке сопротивления
                /// </summary>
                public class CResistance :HelpIeeeBase
                {
                    private readonly CalibrMain _calibrMain;

                    public CResistance(CalibrMain calibrMain)
                    {
                        this._calibrMain = calibrMain;
                    }
                    public enum Zcomp
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
                        [StringValue("ZCOMP NONE")] WireNone
                    }

                    /// <summary>
                    /// Установить сопротивление
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <param name = "mult"></param>
                    /// <returns></returns>
                    public CalibrMain SetValue(decimal value, Multipliers mult = Devices.Multipliers.None)
                    {
                        _calibrMain.WriteLine($@"OUT {JoinValueMult(value, mult)}OHM");
                        return _calibrMain;
                    }

                }
                /// <summary>
                /// Активирует или дезактивирует компенсацию импеданса при 2-проводном или 4-проводном подключении. Компенсация в режиме воспроизведения сопротивления доступна для сопротивлений величиной менее 110 кΩ. Компенсация в режиме воспроизведения емкости доступна для емкостей величиной не менее 110 нФ. 
                /// </summary>
                
                public CCapacitance Capacitance { get; }
                /// <summary>
                /// Содержит набор команд по установке емкости
                /// </summary>
                public class CCapacitance : HelpIeeeBase
                {
                    private readonly CalibrMain _calibrMain;

                    public CCapacitance(CalibrMain calibrMain)
                    {
                        this._calibrMain = calibrMain;
                    }
                    public enum Zcomp
                    {
                        /// <summary>
                        /// компенсация 2х проводная
                        /// </summary>
                        [StringValue("ZCOMP WIRE2")] Wire2,
                        /// <summary>
                        /// Отключает компенсацию
                        /// </summary>
                        [StringValue("ZCOMP NONE")] WireNone
                    }

                    /// <summary>
                    /// Установить емкость
                    /// </summary>
                    /// <param name="value">Значение</param>
                    /// <param name = "mult"></param>
                    /// <returns></returns>
                    public CalibrMain SetValue(decimal value, Multipliers mult = Devices.Multipliers.None)
                    {
                        _calibrMain.WriteLine($@"OUT {JoinValueMult(value, mult)}F");
                        return _calibrMain;
                    }
                }
                /// <summary>
                /// Содержит набор команд по установке температуры
                /// </summary>
                public class Temperature
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
                [StringValue("OPER")] On,
                /// <summary>
                /// Выключить выход
                /// </summary>
                [StringValue("STBY")] Off
            }
            public CalibrMain SetEarth(Earth earth)
            {
                _calibrMain.WriteLine(earth.GetStringValue());
                return _calibrMain;
            }
            /// <summary>
            /// Замыкает или размыкает внутренний контакт между защитным заземлением и заземлением корпуса (шасси). 
            /// </summary>
            public enum Earth
             {  /// <summary>
                /// подключить клемму передней панели LO к заземлению шасси
                /// </summary>
                [StringValue("EARTH TIED")] On,
                /// <summary>
                /// отсоединить клемму передней панели LO от заземления шасси
                /// </summary>
                [StringValue("EARTH OPEN")]Off

             }

        }
        
        public class RangeCalibr : Command
        {
            /// <summary>
            /// Предоставляет окончание, которое необходимо добавить к комманде.
            /// </summary>
            public string Postfix { get; }

            public RangeCalibr(string command, string description, double value, string postfix) : base(command+postfix, description, value)
            {
                Postfix = postfix;
            }

        }

        
    }
}