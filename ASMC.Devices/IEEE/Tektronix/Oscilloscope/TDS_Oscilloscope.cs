// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Globalization;
using System.Threading;
using AP.Utils.Data;
using AP.Utils.Helps;
using MathNet.Numerics.Statistics;

namespace ASMC.Devices.IEEE.Tektronix.Oscilloscope
{
   public class TDS_Oscilloscope: IeeeBase
   {
       
        /// <summary>
        /// Перечень каналов
        /// </summary>
        public enum Chanel
        {
            /// <summary>
            /// Канал 1
            /// </summary>
            CH1,
            /// <summary>
            /// Канал 2
            /// </summary>
            CH2,
            /// <summary>
            /// Канал 3
            /// </summary>
            CH3,
            /// <summary>
            /// Канал 4
            /// </summary>
            CH4
        }
       
       
        /// <summary>
        /// Состояние
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Выключить
            /// </summary>
            OFF,
            /// <summary>
            /// Включить
            /// </summary>
            ON
        }
        /// <summary>
        /// Выбрать канал
        /// </summary>
        /// <param name="ch">The ch.</param>
        /// <param name="st">The st.</param>
        /// <returns></returns>
        public static string SetChanel(Chanel ch, State st= State.ON)
        {
            return "SELECT:" + ch.ToString() + " " + st.ToString();
        }
        /// <summary>
        /// Запуск или остановка осцилографа
        /// </summary>
        /// <param name="st">The st.</param>
        /// <returns></returns>
        public static string EnableRun(State st)
        {
            return "ACQ:STATE " + st.ToString();
        }
        /// <summary>
        /// Команды калибровки и диагностики
        /// </summary>
        public class CalibrAndDiagnostic
        {

        }
        /// <summary>
        /// Команды управление курсорами
        /// </summary>
        public class Cursor
        {

        }
        /// <summary>
        /// Команды управления дисплеем
        /// </summary>
        public class Display
        {
        }
        /// <summary>
        /// Управление разверткой
        /// </summary>
        public class Horizontal
        {
            /// <summary>
            /// Допустимые значения развертки
            /// </summary>
            public enum SCAle
            {
                /// <summary>
                /// 2.5 нс
                /// </summary>
               [StringValue("2.5E-9")]
               Scal_25E10,
                /// <summary>
                /// 5 нс
                /// </summary>
                [StringValue("5E-9")]
                Scal_50E10,
                /// <summary>
                /// 10 нс
                /// </summary>
                [StringValue("10E-9")]
                Scal_10E9,
                /// <summary>
                /// 25 нс
                /// </summary>
                [StringValue("25E-9")]
                Scal_25E9,
                /// <summary>
                /// 50 нс
                /// </summary>
                [StringValue("50E-9")]
                Scal_50E9,
                /// <summary>
                /// 100 нс
                /// </summary>
                [StringValue("10E-8")]
                Scal_10E8,
                /// <summary>
                /// 250 нс
                /// </summary>
                [StringValue("25E-8")]
                Scal_25E8,
                /// <summary>
                /// 500 нс
                /// </summary>
                [StringValue("50E-8")]
                Scal_50E8,
                /// <summary>
                /// 1 мкс
                /// </summary>
                [StringValue("1E-6")]
                Scal_1E6,
                /// <summary>
                /// 2.5 мкс
                /// </summary>
                [StringValue("2.5E-6")]
                Scal_25E7,
                /// <summary>
                /// 5 мкс
                /// </summary>
                [StringValue("5E-6")]
                Scal_50E7,
                /// <summary>
                /// 10 мкс
                /// </summary>
                [StringValue("10E-6")]
                Scal_10E6,
                /// <summary>
                /// 25 мкс
                /// </summary>
                [StringValue("25E-6")]
                Scal_25E6,
                /// <summary>
                /// 50 мкс
                /// </summary>
                [StringValue("50E-6")]
                Scal_50E6,
                /// <summary>
                /// 100 мкс
                /// </summary>
                [StringValue("100E-6")]
                Scal_100E6,
                /// <summary>
                /// 250 мкс
                /// </summary>
                [StringValue("250E-6")]
                Scal_250E6,
                /// <summary>
                /// 500 мкс
                /// </summary>
                [StringValue("500E-6")]
                Scal_500E6,
                /// <summary>
                /// 1 мс
                /// </summary>
                [StringValue("1E-3")]
                Scal_10E4,
                /// <summary>
                /// 2.5 мс
                /// </summary>
                [StringValue("2.5E-3")]
                Scal_25E4,
                /// <summary>
                /// 5 мс
                /// </summary>
                [StringValue("5E-3")]
                Scal_50E4,
                /// <summary>
                /// 10 мс
                /// </summary>
                [StringValue("10E-3")]
                Scal_10E3,
                /// <summary>
                /// 25 мс
                /// </summary>
                [StringValue("25E-3")]
                Scal_25E3,
                /// <summary>
                /// 50 мс
                /// </summary>
                [StringValue("50E-3")]
                Scal_50E3,
                /// <summary>
                /// 100 мс
                /// </summary>
                [StringValue("100E-3")]
                Scal_100E3,
                /// <summary>
                /// 250 мс
                /// </summary>
                [StringValue("250E-3")]
                Scal_250E3,
                /// <summary>
                /// 500 мс
                /// </summary>
                [StringValue("500E-3")]
                Scal_500E3,
                /// <summary>
                /// 1 с
                /// </summary>
                [StringValue("1")]
                Scal_1,
                /// <summary>
                /// 2.5 с
                /// </summary>
                [StringValue("2.5")]
                Scal_25E1,
                /// <summary>
                /// 5 с
                /// </summary>
                [StringValue("5")]
                Scal_5,
                /// <summary>
                /// 10 с
                /// </summary>
                [StringValue("10")]
                Scal_10,
                /// <summary>
                /// 25 с
                /// </summary>
                [StringValue("25")]
                Scal_25,
                /// <summary>
                /// 50 с
                /// </summary>
                [StringValue("50")]
                Scal_50
            }
            /// <summary>
            /// Установка развертки
            /// </summary>
            /// <param name="sc">The sc.</param>
            /// <returns></returns>
            public static string SetScale(SCAle sc)
            {
                return "HORi:SCAL " /*+ MyEnum.GetStringValue(sc)*/;
            }
            //HORizontal:DELay:POSition Position window
            //HORizontal:DELay:SCAle Set or query the window time base time/division
            //HORizontal:DELay:SECdiv Same as HORizontal:DELay:SCAle
            //HORizontal:MAIn:POSition Set or query the main time base trigger point
            //HORizontal:MAIn:SCAle Set or query the main time base time/division
            //HORizontal:MAIn:SECdiv Same as HORizontal:MAIn:SCAle
            //HORizontal:POSition Set or query the position of waveform to display
            //HORizontal:RECOrdlength Return waveform record length
            //HORizontal:SCAle Same as HORizontal:MAIn:SCAle
            //HORizontal:SECdiv Same as HORizontal:MAIn:SCAle
            //HORizontal:VIEW Select view
        }
        /// <summary>
        /// Математика
        /// </summary>
        public class Math
        {
            //MATH:DEFINE Set or query the math waveform definition
            //            MATH:FFT:HORizontal:POSition(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT horizontal display position
            //            MATH:FFT:HORizontal:SCAle(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT horizontal zoom factor
            //            MATH:FFT:VERtical:POSition(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT vertical display position
            //            MATH:FFT:VERtical:SCAle(TDS200 with a TDS2MM module, TDS1000, TDS2000, TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the FFT vertical zoom factor
            //            MATH:VERtical:POSition(TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the math waveform display position
            //            MATH:VERtical:SCAle(TDS1000B, TDS2000B, and TPS2000 only)
            //Set or query the math waveform display scale
        }
        /// <summary>
        /// Измерение
        /// </summary>
        public class Measurement
        {
            /// <summary>
            /// Тип измеряемой величины
            /// </summary>
            public enum TypeMeas
            {
                /// <summary>
                /// Частота
                /// </summary>
                FREQ,
                /// <summary>
                /// Среднеарефметическе
                /// </summary>
                MEAN,
                /// <summary>
                /// Период
                /// </summary>
                PERI,
                /// <summary>
                /// Размах
                /// </summary>
                PK2,
                /// <summary>
                /// Среднеквадратическое
                /// </summary>
                CRM,
                /// <summary>
                /// Минимум
                /// </summary>
                MIN,
                /// <summary>
                /// Максимум
                /// </summary>
                MAXI,
                /// <summary>
                /// Фронт
                /// </summary>
                RIS,
                /// <summary>
                /// Срез
                /// </summary>
                FALL,
                /// <summary>
                /// Длительноть положительного
                /// </summary>
                PWI,
                /// <summary>
                /// Длительность отрецательного
                /// </summary>
                NWI,
                /// <summary>
                /// Нет
                /// </summary>
                NONe
            }
            /// <summary>
            /// Измерительный канал
            /// </summary>
            public enum CHMeas
            {
                CH1 = 1,
                CH2 = 2,
                CH3 = 3,
                CH4 = 4,
                CH5 = 5
            }
             /// <summary>
             /// Установить измерения
             /// </summary>
             /// <param name="ch">на канале</param>
             /// <param name="chm">измерительный канал</param>
             /// <param name="tm">тип измерения</param>
             /// <returns></returns>
            public static string SetMeas(Chanel ch, CHMeas chm, TypeMeas tm)
            {
                return "MEASU:MEAS" + (int)chm + ":SOU " + ch.ToString() + "\nMEASU:MEAS" + (int)chm + ":TYP " + tm.ToString();
            }
            /// <summary>
            /// Запрос данных с указаного измерительного канала
            /// </summary>
            /// <param name="chm">Измерительный канал</param>
            /// <returns></returns>
            public static string QueruValue(CHMeas chm)
            {
                return "MEASU:MEAS"+(int)chm + ":VAL?";
            }

        }
        /// <summary>
        /// Различные команды
        /// </summary>
        public class Miscellaneous
        {
            /// <summary>
            /// ПО каким параметрам необходимо выбирать автопредел
            /// </summary>
            public enum Setting
            {
                /// <summary>
                /// Развертка
                /// </summary>
                HORizontal,
                /// <summary>
                /// Коэфициент отклонения
                /// </summary>
                VERTical,
                /// <summary>
                /// По Х и Y, Значение по умолчанию
                /// </summary>
                BOTH
            }
            /// <summary>
            /// Выбор автодиапазона(TDS1000B, TDS2000B, and TPS2000 only)
            /// </summary>
            /// <param name="st">Состояние</param>
            /// <param name="sett">Параметр</param>
            /// <returns></returns>
            public static string AutoRange(State st, Setting sett= Setting.BOTH)
            {
                return "AUTOR:STATE " + st.ToString() + "\n" + "AUTOR:SETT" + sett.ToString();
            }
            /// <summary>
            /// Автоустановка
            /// </summary>
            /// <returns></returns>
            public static string AutoSet()
            {
                return "AUTOS EXEC";
            }
            /// <summary>
            /// Режим сбора данных
            /// </summary>
            public enum Mode
            {
                SAMple,
                /// <summary>
                /// The pea kdetect
                /// </summary>
                PEAKdetect,
                /// <summary>
                /// Усреднение
                /// </summary>
                AVErage
            }
            /// <summary>
            /// Количество накоплений данных при усреднении
            /// </summary>
            public enum NUMAV
            {
                Number_4 = 4,
                Number_16 = 16,
                Number_64 = 64,
                Number_128 = 128,
            }
            /// <summary>
            /// Установка сбора данных
            /// </summary>
            /// <param name="md">Режим сбора данных</param>
            /// <param name="num">Количество накоплений, только для <see cref="Mode.AVErage"/>/param>
            /// <returns></returns>
            public static string SetDataCollection(Mode md, NUMAV num= NUMAV.Number_128)
            {
                if (md== Mode.AVErage)
                {
                    return "ACQuire:MODe " + md.ToString()+ "\nACQuire:NUMAVg "+ (int)num;
                }
                else
                {
                    return "ACQuire:MODe " + md.ToString();
                }
            }


        }
        /// <summary>
        /// Управление тригером
        /// </summary>
        public class Trigger
        {
            public enum Sours
            {
                /// <summary>
                /// Канал 1
                /// </summary>
                [StringValue("CH1")]
                CH1,
                /// <summary>
                /// Канал 2
                /// </summary>
                [StringValue("CH2")]
                CH2,
                /// <summary>
                /// Канал 3
                /// </summary>
                [StringValue("CH3")]
                CH3,
                /// <summary>
                /// Канал 4
                /// </summary>
                [StringValue("CH4")]
                CH4,
                [StringValue("EXT")]
                EXT,
                [StringValue("EXT5")]
                EXT5,
                [StringValue("EXT10")]
                EXT10,
                [StringValue("AC LINE")]
                AC_LINE
            }
            /// <summary>
            /// Сробатывание тригера
            /// </summary>
            public enum Slope
            {
                /// <summary>
                /// по фронту
                /// </summary>
                FALL,
                /// <summary>
                /// По резу
                /// </summary>
                RIS
            }
            /// <summary>
            /// Режим работы
            /// </summary>
            public enum Mode
            {
                /// <summary>
                /// Авто
                /// </summary>
                AUTO,
                /// <summary>
                /// Вручную
                /// </summary>
                NORM
            }
            //public static string SetTriger(Sours sur, Slope sp,Mode md= Mode.AUTO, double Level=0)
            //{
            //    if (md!= Mode.AUTO)
            //    {
            //        return "TRIG:MAIn:EDGE:SOU " + MyEnum.GetStringValue(sur) + "\nTRIG:MAI:EDGE:SLO " + sp.ToString() + "\nTRIG:MAI:MODe " + md.ToString()+ "\nTRIG:MAI:LEV " +Level;
            //    }
            //    else
            //    {
            //        return "TRIG:MAIn:EDGE:SOU " + MyEnum.GetStringValue(sur) + "\nTRIG:MAI:EDGE:SLO " + sp.ToString() + "\nTRIG:MAI:MODe " + md.ToString();
            //    }
             
            //}
        }
        /// <summary>
        /// Коэфициент отклонения
        /// </summary>
        public class Vertical
        {
            /// <summary>
            ///Значения внешнего делителя в режиме напряжения
            /// </summary>
            public enum Probe
            {
                
                Att_1=1, 
                Att_10= 10,
                Att_20 = 20,
                Att_50 = 50,
                Att_100=100,
                Att_500=500,
                Att_1000=1000,
            }
            /// <summary>
            /// Режимы работы канала
            /// </summary>
            public enum COUPling
            {
                /// <summary>
                /// Только АС
                /// </summary>
                AC,
                /// <summary>
                /// AC + DC 
                /// </summary>
                DC,
                /// <summary>
                /// Земля
                /// </summary>
                GND
            }
            /// <summary>
            /// Настройка канала
            /// </summary>
            /// <param name="ch">Канал<see cref="Chanel"/></param>
            /// <param name="coup">Режимы работы канала<see cref="COUPling"/></param>
            /// <param name="BWlim">Ограничение полосы пропускания</param>
            /// <param name="Invert">Инвертирование сигнала</param>
            /// <returns></returns>
            public static string SetSetupChanel(Chanel ch, COUPling coup, State BWlim= State.OFF, State Invert= State.OFF)
            {
                return ch.ToString() + ":COUP " + coup.ToString() +"\n"+ ch.ToString() + ":BAN " + BWlim.ToString() + "\n" + ch.ToString() + ":INV " + Invert.ToString();
            }
            /// <summary>
            /// Масштаб, указан для <see cref="Probe.Att_1"/>
            /// </summary>
            public enum Scale
            {
                /// <summary>
                /// 2 mV
                /// </summary>
                Scale_2 = 2,
                /// <summary>
                /// 5 mV
                /// </summary>
                Scale_5 = 5,
                /// <summary>
                /// 10 mV
                /// </summary>
                Scale_10 = 10,
                /// <summary>
                /// 20 mV
                /// </summary>
                Scale_20 = 20,
                /// <summary>
                /// 50 mV
                /// </summary>
                Scale_50 = 50,
                /// <summary>
                /// 100 mV
                /// </summary>
                Scale_100 = 100,
                /// <summary>
                /// 200 mV
                /// </summary>
                Scale_200 = 200,
                /// <summary>
                /// 500 mV
                /// </summary>
                Scale_500 = 500,
                /// <summary>
                /// 1000 mV
                /// </summary>
                Scale_1000 = 1000,
                /// <summary>
                /// 2000 mV
                /// </summary>
                Scale_2000 = 2000,
                /// <summary>
                /// 5000 mV
                /// </summary>
                Scale_5000 = 5000
            }
            /// <summary>
            /// Sets the sc ale.
            /// </summary>
            /// <param name="ch">The ch.</param>
            /// <param name="sc">The sc.</param>
            /// <param name="prb">The PRB.</param>
            /// <returns></returns>
            public static string SetSCAle(Chanel ch, Scale sc, Probe prb = Probe.Att_1)
            {
                return ch.ToString() + ":PRO " + ((int)prb).ToString() + "\n" + ch.ToString() + ":SCAle " + ((int)sc/1000.0 * (int)prb).ToString();
            }
        }
        /// <summary>
        /// Пребразует данные в нужных единицах
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="Mult">The mult.</param>
        /// <returns></returns>
        public double DataPreparationAndConvert(string date, Multipliers Mult = AP.Utils.Helps.Multipliers.None)
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
                case AP.Utils.Helps.Multipliers.Mega:
                    _return=(dDate[0] * System.Math.Pow(10, dDate[1])) * 1E-6;
                    break;
                case AP.Utils.Helps.Multipliers.Mili:
                    _return=(dDate[0] * System.Math.Pow(10, dDate[1])) *1E3;
                    break;
                case AP.Utils.Helps.Multipliers.Nano:
                    _return=(dDate[0] * System.Math.Pow(10, dDate[1])) * 1E9;
                    break;
                case AP.Utils.Helps.Multipliers.Kilo:
                    _return = (dDate[0] * System.Math.Pow(10, dDate[1])) * 1E-3;
                    break;
                case AP.Utils.Helps.Multipliers.Micro:
                    _return=(dDate[0] * System.Math.Pow(10, dDate[1])) * 1E6;
                    break;
                case AP.Utils.Helps.Multipliers.None:
                    _return=(dDate[0] * System.Math.Pow(10, dDate[1]));
                    break;
            }
            return _return;
        }
   }
}
