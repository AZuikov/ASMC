﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using AP.Utils.Helps;

namespace ASMC.Devices.IEEE.Fluke.CalibtatorOscilloscope
{
    public class Calibr9500B : IeeeBase
    {
        public enum Chanel
        {
            Ch1,
            Ch2,
            Ch3,
            Ch4,
            Ch5
        }

        public enum State
        {
            On,
            Off
        }

        #region Property

        public AUXFunctional AuxFunc { get; }

        public ROUTe Route { get; }

        public CSource Source { get; }

        #endregion

        public Calibr9500B()
        {
            Source = new CSource(this);
            Route = new ROUTe(this);
            AuxFunc = new AUXFunctional(this);
        }

        /// <summary>
        /// Настройка выхода
        /// </summary>
        public class CSource : HelpDeviceBase
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

            #region Fields

            private readonly Calibr9500B _calibrMain;

            #endregion

            public CSource(Calibr9500B calibrMain)
            {
                _calibrMain = calibrMain;
            }

            #region Methods

            /// <summary>
            /// Управление переключением выхода: включено/выключено
            /// </summary>
            public Calibr9500B Output(State st)
            {
                _calibrMain.WriteLine("OUTP " + st);
                return _calibrMain;
            }

            /// <summary>
            /// Установка частоты
            /// </summary>
            /// <param name = "value">The value.</param>
            /// <param name = "mult">The mult.</param>
            /// <returns></returns>
            public Calibr9500B SetFreq(double value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
            {
                _calibrMain.WriteLine($@"SOUR:FREQ {JoinValueMult(value, mult)}");
                return _calibrMain;
            }

            /// <summary>
            /// Данная команда определяет основной требуемый сигнальный выход, т.е. выбирает функцию источника калибратора 9500B.
            /// </summary>
            public Calibr9500B SetFunc(Shap sp)
            {
                _calibrMain.WriteLine("SOUR:FUNC " + sp);
                return _calibrMain;
            }

            /// <summary>
            /// Установка периода
            /// </summary>
            /// <param name = "value">The value.</param>
            /// <param name = "mult">The mult.</param>
            /// <returns></returns>
            public Calibr9500B SetPeriod(double value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
            {
                _calibrMain.WriteLine($@"SOUR:PER {JoinValueMult(value, mult)}");
                return _calibrMain;
            }

            /// <summary>
            /// Установка напряжения
            /// </summary>
            /// <param name = "value">The value.</param>
            /// <param name = "mult">The mult.</param>
            /// <returns></returns>
            public Calibr9500B SetVoltage(double value, Multipliers mult = AP.Utils.Helps.Multipliers.None)
            {
                _calibrMain.WriteLine($@"SOUR:VOLT {JoinValueMult(value, mult)}");
                return _calibrMain;
            }

            #endregion

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
                    #region Fields

                    private readonly Calibr9500B _calibrMain;

                    #endregion

                    public DC(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    #region Methods

                    /// <summary>
                    /// Данная команда позволяет установить постоянный сигнал на нескольких каналах (если устанавливается в «ON») и отключить
                    /// многоканальность, если выбрано «OFF»
                    /// </summary>
                    /// <param name = "st">The st.</param>
                    /// <returns></returns>
                    public Calibr9500B MCH(State st)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:DC:MCH " + st);
                        return _calibrMain;
                    }

                    /// <summary>
                    /// Данная команда устанавливает выход напряжения постоянного тока (DCV) в ноль (0 В), если выбрано «ON» и вернет
                    /// предыдущее значение, если выбрано «OFF».
                    /// Изменение функция отключает состояние заземления.
                    /// С выходом, установленным в состояние «ON», выводится ошибка конфликта параметров настройки, если выбрана не функция DC.
                    /// </summary>
                    /// <param name = "st">The st.</param>
                    /// <returns></returns>
                    public Calibr9500B SetGND(State st)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:DC:GRO " + st);
                        return _calibrMain;
                    }

                    #endregion
                }

                /// <summary>
                /// Настройка Меандра
                /// </summary>
                public class SQU
                {
                    /// <summary>
                    /// </summary>
                    public enum Polar
                    {
                        /// <summary>
                        /// устанавливает выходной прямоугольный сигнал положительной полярности относительно заземления
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

                    #region Fields

                    private readonly Calibr9500B _calibrMain;

                    #endregion

                    public SQU(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    #region Methods

                    /// <summary>
                    /// Данная команда устанавливает выходной сигнал прямоугольной формы в ноль(0 В), если выбрано «ON» и вернет предыдущее
                    /// выходное значение, если выбрано «OFF».
                    /// </summary>
                    /// <param name = "st">The st.</param>
                    /// <returns></returns>
                    public Calibr9500B SetGND(State st)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:SQU:GRO " + st);
                        return _calibrMain;
                    }

                    /// <summary>
                    /// Данная команда выбирает полярность прямоугольного сигнала: выше, ниже или симметрично относительно «0» вольт
                    /// </summary>
                    /// <param name = "pr">The pr.</param>
                    /// <returns></returns>
                    public Calibr9500B SetPolar(Polar pr)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:SQU:POL " + pr);
                        return _calibrMain;
                    }

                    #endregion
                }

                /// <summary>
                /// настройка импульса перепада
                /// </summary>
                public class EDGE
                {
                    public enum Direction
                    {
                        /// <summary>
                        /// устанавливает фронт (ступенька вверх).
                        /// </summary>
                        RIS,

                        /// <summary>
                        /// устанавливает спад (ступенька вниз).
                        /// </summary>
                        FALL
                    }

                    /// <summary>
                    /// Скорость наростания
                    /// </summary>
                    public enum SpeedEdge
                    {
                        /// <summary>
                        /// 100нс
                        /// </summary>
                        Low_100n = 700,

                        /// <summary>
                        /// 500 пс
                        /// </summary>
                        Mid_500p = 500,

                        /// <summary>
                        /// 150 пс
                        /// </summary>
                        Fast_150p = 100
                    }

                    #region Fields

                    private readonly Calibr9500B _calibrMain;

                    #endregion

                    public EDGE(Calibr9500B calibr)
                    {
                        _calibrMain = calibr;
                    }

                    #region Methods

                    /// <summary>
                    /// Задает направление перепада, который следует за запуском
                    /// </summary>
                    /// <param name = "Se"></param>
                    /// <returns></returns>
                    public Calibr9500B SetDerection(Direction Der)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:EDGE:TRAN " + Der);
                        return _calibrMain;
                    }

                    /// <summary>
                    /// Данная команда выбирает скорость (скорость нарастания выходного напряжения) функции Edge
                    /// </summary>
                    /// <param name = "Se"></param>
                    /// <returns></returns>
                    public Calibr9500B SetSpeed(SpeedEdge Se)
                    {
                        _calibrMain.WriteLine("SOUR:PAR:EDGE:SPE " + (double) Se * Math.Pow(10, -12));
                        return _calibrMain;
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Данная подсистема используется для конфигурирования выходных каналов, которые используются как выходы сигнала и источники триггера.
        /// </summary>
        public class ROUTe
        {
            #region Fields

            private Calibr9500B _calibMain;
            public ChanelSetting Chanel { get; }

            #endregion

            public ROUTe(Calibr9500B calibr)
            {
                _calibMain = calibr;
                Chanel = new ChanelSetting(calibr);


            }

            public class ChanelSetting
            {
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

                #region Fields

                private  Calibr9500B _calibMain;

                #endregion

                public ChanelSetting(Calibr9500B calibr)
                {
                    _calibMain = calibr;
                }

                #region Methods

                /// <summary>
                /// Данная команда используется для определения канала, связанного с сигнальным выходом.
                /// <cpd> не включает выход, а только выбирает используемый сигнальный канал.
                /// </summary>
                /// <param name = "ch">The ch.</param>
                /// <returns></returns>
                public Calibr9500B SetChanel(Chanel ch)
                {
                    _calibMain.WriteLine("ROUT:SIGN " + ch);
                    return _calibMain;
                }

                /// <summary>
                /// Данная команда выбирает между 50 Ом и 1 МОм импедансом осциллографа, в соответствии с уровнями для выбранного
                /// сигнального канала.
                /// </summary>
                /// <param name = "imp">The imp.</param>
                /// <returns></returns>
                public Calibr9500B SetImpedans(Impedans imp)
                {
                    _calibMain.WriteLine("ROUT:SIGN:IMP " + (int) imp);
                    return _calibMain;
                }

                #endregion
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

            #region Fields

            private readonly Calibr9500B _calibrMain;

            #endregion

            public AUXFunctional(Calibr9500B calibr)
            {
                _calibrMain = calibr;
            }

            #region Methods

            /// <summary>
            /// Запрос значения сопротивления или емкости.
            /// </summary>
            public string QuaryAuxValue()
            {
                var answer = _calibrMain.QueryLine("READ?");

                return answer;
            }

            /// <summary>
            /// Данная команда используется для выбора функции измерения входного сопротивления или входной емкости испытуемого
            /// осциллографа.
            /// </summary>
            /// <param name = "tm"></param>
            /// <returns></returns>
            public Calibr9500B SetMeasure(TypeMes tm)
            {
                _calibrMain.WriteLine("CONF:" + tm);
                return _calibrMain;
            }

            #endregion
        }
    }
}