// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using AP.Math;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using NLog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using Accord.Statistics;
using Timer = System.Timers.Timer;

namespace ASMC.Devices.Port.APPA
{
    // ReSharper disable once InconsistentNaming
    public class Mult107_109N : ComPort
    {
        #region MeasureCharacteristic

        public RangeStorage<PhysicalRange<Frequency>> FrequencyRangeStorage
        {
            get => getFrequencyRangeStorage();
        }

        protected virtual RangeStorage<PhysicalRange<Frequency>> getFrequencyRangeStorage()
        {
            return new RangeStorage<PhysicalRange<Frequency>>(new []
            {
                //20 Hz
                new PhysicalRange<Frequency>(new MeasPoint<Frequency>(0), new MeasPoint<Frequency>(20),
                                             new AccuracyChatacteristic(0.050M,null,0.01M)), 
                //200 Hz
                new PhysicalRange<Frequency>(new MeasPoint<Frequency>(0), new MeasPoint<Frequency>(200),
                                             new AccuracyChatacteristic(0.10M,null,0.01M)),
                //2 kHz
                new PhysicalRange<Frequency>(new MeasPoint<Frequency>(0), new MeasPoint<Frequency>(2,UnitMultiplier.Kilo),
                                             new AccuracyChatacteristic(1.0M,null,0.01M)),
                //20 kHz
                new PhysicalRange<Frequency>(new MeasPoint<Frequency>(0), new MeasPoint<Frequency>(20,UnitMultiplier.Kilo),
                                             new AccuracyChatacteristic(0.010M,null,0.01M)),
                //200 kHz
                new PhysicalRange<Frequency>(new MeasPoint<Frequency>(0), new MeasPoint<Frequency>(200,UnitMultiplier.Kilo),
                                             new AccuracyChatacteristic(0.00010M,null,0.01M)),
                //1 MHz
                new PhysicalRange<Frequency>(new MeasPoint<Frequency>(0), new MeasPoint<Frequency>(200,UnitMultiplier.Kilo),
                                             new AccuracyChatacteristic(0.0010M,null,0.01M)),
            });
        }

        public RangeStorage<PhysicalRange<CelsiumGrad>> GetCelsiumRangeStorage
        {
            get => CelsiumRangeStorage();
        }

        protected virtual RangeStorage<PhysicalRange<CelsiumGrad>> CelsiumRangeStorage()
        {
            return new RangeStorage<PhysicalRange<CelsiumGrad>>(new []{
                
                //-200....-100
                new PhysicalRange<CelsiumGrad>(new MeasPoint<CelsiumGrad>(-200),new MeasPoint<CelsiumGrad>(-100),
                                               new AccuracyChatacteristic(6.0M,null,0.1M)),
                //-100...400
                new PhysicalRange<CelsiumGrad>(new MeasPoint<CelsiumGrad>(-100), new MeasPoint<CelsiumGrad>(400), 
                                               new AccuracyChatacteristic(3.0M, null, 0.1M)),
                //400...1200
                new PhysicalRange<CelsiumGrad>(new MeasPoint<CelsiumGrad>(400),new MeasPoint<CelsiumGrad>(1200), 
                                               new AccuracyChatacteristic(3M, null,0.1M))
                
            });
        }

        public RangeStorage<PhysicalRange<Capacity>> CapacityRangeStorage
        {
            get => getCapacityRangeStorage();
        }

        protected virtual RangeStorage<PhysicalRange<Capacity>> getCapacityRangeStorage()
        {
            return new RangeStorage<PhysicalRange<Capacity>>(new []
            {
                // 4 nF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(4,UnitMultiplier.Nano), 
                                            new AccuracyChatacteristic(0.00000000001M, null,1.5M)), 
                // 40 nF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(40,UnitMultiplier.Nano),
                                            new AccuracyChatacteristic(0.00000000010M, null,1.5M)),
                // 400 nF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(400,UnitMultiplier.Nano),
                                            new AccuracyChatacteristic(0.0000000005M, null,0.9M)),
                // 4 uF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(4,UnitMultiplier.Micro),
                                            new AccuracyChatacteristic(0.000000005M, null,0.9M)),
                // 40 uF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(40,UnitMultiplier.Micro),
                                            new AccuracyChatacteristic(0.00000005M, null,1.2M)),
                // 400 uF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(400,UnitMultiplier.Micro),
                                            new AccuracyChatacteristic(0.00000005M, null,1.2M)),
                // 4 mF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(4,UnitMultiplier.Mili),
                                            new AccuracyChatacteristic(0.000005M, null,1.5M)),
                // 40 mF
                new PhysicalRange<Capacity>(new MeasPoint<Capacity>(0), new MeasPoint<Capacity>(40,UnitMultiplier.Mili),
                                            new AccuracyChatacteristic(0.00005M, null,1.5M)),
            });
        }


        public RangeStorage<PhysicalRange<Resistance>> ResistanceRangeStorage
        {
            get => getResistanceRangeStorage();
        }

        protected virtual RangeStorage<PhysicalRange<Resistance>> getResistanceRangeStorage()
        {
            return new RangeStorage<PhysicalRange<Resistance>>(new[]
            {
                //200 Om
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(200),
                                              new AccuracyChatacteristic(0.30M,null,0.3M)),
                // 2kOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(2,UnitMultiplier.Kilo), 
                                              new AccuracyChatacteristic(0.0030M,null,0.3M)),
                // 20kOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(20,UnitMultiplier.Kilo),
                                              new AccuracyChatacteristic(0.030M,null,0.3M)),
                // 200kOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(200,UnitMultiplier.Kilo),
                                              new AccuracyChatacteristic(0.30M,null,0.3M)),
                // 2MOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(2,UnitMultiplier.Mega),
                                              new AccuracyChatacteristic(0.0050M,null,0.3M)),

                // 20MOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(20,UnitMultiplier.Mega),
                                              new AccuracyChatacteristic(0.050M,null,5M)),
                // 200MOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(200,UnitMultiplier.Mega),
                                              new AccuracyChatacteristic(20M,null,5M)),
                // 2GOm
                new PhysicalRange<Resistance>(new MeasPoint<Resistance>(0), new MeasPoint<Resistance>(2,UnitMultiplier.Giga),
                                              new AccuracyChatacteristic(0.8M,null,5M))

            });
        }

        public RangeStorage<PhysicalRange<Current>> DciRangeStorage
        {
            get => getDciStorage();
        }

        protected virtual RangeStorage<PhysicalRange<Current>> getDciStorage()
        {
            return new RangeStorage<PhysicalRange<Current>>(new[]
            {
                // 20 mA
                new PhysicalRange<Current>(new MeasPoint<Current>(0),new MeasPoint<Current>(20,UnitMultiplier.Mili),
                                           new AccuracyChatacteristic(0.000040M,null,0.2M)),
                // 200 mA
                new PhysicalRange<Current>(new MeasPoint<Current>(0),new MeasPoint<Current>(200,UnitMultiplier.Mili),
                                           new AccuracyChatacteristic(0.00040M,null,0.2M)),
                // 2 A
                new PhysicalRange<Current>(new MeasPoint<Current>(0),new MeasPoint<Current>(2),
                                           new AccuracyChatacteristic(0.0040M,null,0.2M)),
                // 10 A
                new PhysicalRange<Current>(new MeasPoint<Current>(0),new MeasPoint<Current>(10),
                                           new AccuracyChatacteristic(0.040M,null,0.2M)),
            });
        }

        /// <summary>
        /// Пределы измерения с характеристиками точности для переменного тока.
        /// </summary>
        public RangeStorage<PhysicalRange<Current, Frequency>> aciRangeStorage
        {
            get => GetAciRangeStorage();
        }

        protected virtual RangeStorage<PhysicalRange<Current, Frequency>> GetAciRangeStorage()
        {
            return new RangeStorage<PhysicalRange<Current, Frequency>>(new[]
            {
                // 20 mA
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.Mili, 40),
                                                      new MeasPoint<Current, Frequency>(20, UnitMultiplier.Mili, 500),
                                                      new AccuracyChatacteristic(0.000050m, null, 0.8M)),
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.Mili, 500),
                                                      new MeasPoint<Current, Frequency>(20, UnitMultiplier.Mili, 1,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.000080m, null, 1.2M)),
                //200 mA
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.Mili, 40),
                                                      new MeasPoint<Current, Frequency>(200, UnitMultiplier.Mili, 500),
                                                      new AccuracyChatacteristic(0.00050M, null, 0.8M)),
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.Mili, 500),
                                                      new MeasPoint<Current, Frequency>(200, UnitMultiplier.Mili, 1,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.00080M, null, 1.2M)),
                new
                    PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.Mili, 1, UnitMultiplier.Kilo),
                                                      new MeasPoint<Current, Frequency>(200, UnitMultiplier.Mili, 3,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.00080M, null, 2M)),

                //2 A
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.None, 40),
                                                      new MeasPoint<Current, Frequency>(2, UnitMultiplier.None, 500),
                                                      new AccuracyChatacteristic(0.0050M, null, 0.8M)),
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.None, 500),
                                                      new MeasPoint<Current, Frequency>(2, UnitMultiplier.None, 1,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.0080M, null, 1.2M)),
                new
                    PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                                                      new MeasPoint<Current, Frequency>(2, UnitMultiplier.None, 3,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.0080M, null, 2M)),

                // 10 A
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.None, 40),
                                                      new MeasPoint<Current, Frequency>(10, UnitMultiplier.None, 500),
                                                      new AccuracyChatacteristic(0.050M, null, 0.8M)),
                new PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.None, 500),
                                                      new MeasPoint<Current, Frequency>(10, UnitMultiplier.None, 1,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.080M, null, 1.2M)),
                new
                    PhysicalRange<Current, Frequency>(new MeasPoint<Current, Frequency>(0, UnitMultiplier.None, 1, UnitMultiplier.Kilo),
                                                      new MeasPoint<Current, Frequency>(10, UnitMultiplier.None, 3,
                                                                                        UnitMultiplier.Kilo),
                                                      new AccuracyChatacteristic(0.080M, null, 2M)),
            });
        }

        /// <summary>
        /// Пределы измерения постоянного напряжения.
        /// </summary>
        public RangeStorage<PhysicalRange<Voltage>> DcvRangeStorage
        {
            get => GetDcvRangeStorage();
        }

        protected virtual RangeStorage<PhysicalRange<Voltage>> GetDcvRangeStorage()
        {
            return new RangeStorage<PhysicalRange<Voltage>>(new[]
            {
                // 20 мВ
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0), new MeasPoint<Voltage>(20, UnitMultiplier.Mili),
                                           new AccuracyChatacteristic(0.000060M,null,0.06M)),
                // 200 мВ
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0), new MeasPoint<Voltage>(200,UnitMultiplier.Mili),
                                           new AccuracyChatacteristic(0.00020M, null,0.06M)),

                // 2 V
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),new MeasPoint<Voltage>(2),
                                           new AccuracyChatacteristic(0.0010M, null, 0.06M)),

                // 20 V
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),new MeasPoint<Voltage>(20),
                                           new AccuracyChatacteristic(0.010M, null, 0.06M)),

                //200 V
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),new MeasPoint<Voltage>(200),
                                           new AccuracyChatacteristic(0.10M, null, 0.06M)),

                //1000 V
                new PhysicalRange<Voltage>(new MeasPoint<Voltage>(0),new MeasPoint<Voltage>(1000),
                                           new AccuracyChatacteristic(1M, null, 0.06M)),
            });
        }

        public RangeStorage<PhysicalRange<Voltage, Frequency>> AcvStorage
        {
            get => GetAcvStorage();
        }

        protected RangeStorage<PhysicalRange<Voltage, Frequency>> GetAcvStorage()
        {
            return new RangeStorage<PhysicalRange<Voltage, Frequency>>(new[]
            {
                //20 mV 40-100Hz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.Mili, new Frequency(100)),
                                                      new AccuracyChatacteristic(0.000080M,null,0.7M)),
                //20 mV 100Hz-1kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(100)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.Mili, new Frequency(1000)),
                                                      new AccuracyChatacteristic(0.000080M,null,1M)),

                //200 mV 40-100Hz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.Mili, new Frequency(100)),
                                                      new AccuracyChatacteristic(0.00080M,null,0.7M)),
                //200 mV 100Hz-1kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(100)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.Mili, new Frequency(1000)),
                                                      new AccuracyChatacteristic(0.00080M,null,1M)),

                //2 V 40-100Hz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.None, new Frequency(100)),
                                                      new AccuracyChatacteristic(0.0050M,null,0.7M)),
                //2 V 100Hz-1kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(100)),
                                                      new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.None, new Frequency(1000)),
                                                      new AccuracyChatacteristic(0.0050M,null,1M)),

                //2 V 1kHz-10kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(1,UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.None, new Frequency(10,UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.0060M,null,2M)),

                //2 V 10kHz-20kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(10, UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.None, new Frequency(20, UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.0070M,null,3M)),

                //2 V 20kHz-50kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(20, UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.None, new Frequency(50, UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.0080M,null,5M)),
                //2 V 50kHz-100kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(50, UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(2, UnitMultiplier.None, new Frequency(100, UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.0100M,null,10M)),

                //20 V 40-100Hz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, new Frequency(100)),
                                                      new AccuracyChatacteristic(0.050M,null,0.7M)),
                //20 V 100Hz-1kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(100)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, new Frequency(1000)),
                                                      new AccuracyChatacteristic(0.050M,null,1M)),

                //20 V 1kHz-10kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(1,UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, new Frequency(10,UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.060M,null,2M)),

                //20 V 10kHz-20kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(10, UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, new Frequency(20, UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.070M,null,3M)),

                //20 V 20kHz-50kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(20, UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, new Frequency(50, UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.080M,null,5M)),
                //20 V 50kHz-100kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(50, UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(20, UnitMultiplier.None, new Frequency(100, UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.100M,null,10M)),

                //200 V 40-100Hz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.None, new Frequency(100)),
                                                      new AccuracyChatacteristic(0.50M,null,0.7M)),
                //200 V 100Hz-1kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(100)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.None, new Frequency(1,UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.50M,null,1M)),
                //200 V 1kHz-10kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(1,UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.None, new Frequency(10,UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.60M,null,2M)),
                //200 V 10kHz-20kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(10,UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.None, new Frequency(20,UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.70M,null,3M)),
                //200 V 20kHz-50kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(20,UnitMultiplier.Kilo)),
                                                      new MeasPoint<Voltage, Frequency>(200, UnitMultiplier.None, new Frequency(50,UnitMultiplier.Kilo)),
                                                      new AccuracyChatacteristic(0.80M,null,5M)),

                //750 V 40Hz-100Hz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(750, UnitMultiplier.None, new Frequency(100)),
                                                      new AccuracyChatacteristic(5.0M,null,0.7M)),
                //750 V 100Hz-1kHz
                new PhysicalRange<Voltage, Frequency>(new MeasPoint<Voltage, Frequency>(0,UnitMultiplier.None, new Frequency(40)),
                                                      new MeasPoint<Voltage, Frequency>(750, UnitMultiplier.None, new Frequency(100)),
                                                      new AccuracyChatacteristic(5.0M,null,1M))
            });
        }

        #endregion MeasureCharacteristic

        public enum BlueState
        {
            NoPress = 0,
            OnPress = 0x01,
            DoublePress = 0x02
        }

        public enum Function
        {
            None = 0x00,
            InputReading = 0x01,
            Freq = 0x02,
            Period = 0x03,
            DutyFactor = 0x04,
            StampStoreRecallLoginLogout = 0x08,
            Store = 0x09,
            Recall = 0x0A,
            AutoHold = 0x0C,
            Max = 0x0D,
            Min = 0x0E,
            PeakHoldMax = 0x10,
            PeakHoldMin = 0x11,
            Delta = 0x17,
            Ref = 0x19,
            dBm = 0x1A,
            dB = 0x1B,
            Avg = 0x25,
            ProbECharacter = 0x26,
            ErCharacter = 0x27,
            FuseCharacter = 0x28,
            PausCharacter = 0x29,
            LogoutMaxData = 0x2A,
            LogoutMinData = 0x2B,
            LogoutMaxTurningPoint = 0x2C,
            LogoutMinTurningPoint = 0x2D,
            LogoutData = 0x2E,
            PeriodTime = 0x2F,
            FullCharacter = 0x30,
            EpErCharacter = 0x31,
            EepromCharacter = 0x32,
            LoginStamp = 0x33
        }

        /// <summary>
        /// Допустимые режимы змерения
        /// </summary>
        public enum MeasureMode
        {
            [StringValue("Переменное напряжение")] ACV,
            [StringValue("Переменное напряжение")] ACmV,
            [StringValue("Постоянное напряжение")] DCV,
            [StringValue("Постоянное напряжение")] DCmV,
            [StringValue("Переменное ток")] ACI,
            [StringValue("Переменное ток")] ACmA,
            [StringValue("Постоянное ток")] DCI,
            [StringValue("Постоянное ток")] DCmA,

            [StringValue("Измерение переменного напряжения со смещением")]
            AC_DC_V,

            [StringValue("Измерение переменного напряжения со смещением")]
            AC_DC_mV,

            [StringValue("Измерение переменного тока со смещением")]
            AC_DC_I,

            [StringValue("Измерение переменного тока со смещением")]
            AC_DC_mA,

            [StringValue("Измерение сопртивления")]
            Ohm,

            [StringValue("Измерение сопротивления малым напряжением")]
            LowOhm,

            [StringValue("Испытание p-n переходов")]
            Diode,

            [StringValue("Прозвонка цепей")] Beeper,
            [StringValue("Измерение ёмкости")] Cap,
            [StringValue("Измерение частоты")] Herz,

            [StringValue("Измерение коэффициента заполнения")]
            DutyFactor,

            [StringValue("Измерение температуры в град. Цельсия")]
            degC,

            [StringValue("Измерение температуры в град. Фарингейта")]
            DegF,

            [StringValue("Неизвестный режим")] None
        }

        /// <summary>
        /// Коды переключатля прибора
        /// </summary>
        public enum RangeCode
        {
            Range1Manual = 0x80,
            Range2Manual = 0x81,
            Range3Manual = 0x82,
            Range4Manual = 0x83,
            Range5Manual = 0x84,
            Range6Manual = 0x85,
            Range7Manual = 0x86,
            Range8Manual = 0x87,
            Range1Auto = 0,
            Range2Auto = 0x01,
            Range3Auto = 0x02,
            Range4Auto = 0x03,
            Range5Auto = 0x04,
            Range6Auto = 0x05,
            Range7Auto = 0x06,
            Range8Auto = 0x07
        }

        /// <summary>
        /// Допустимые номиналы пределов измерения
        /// </summary>
        public enum RangeNominal
        {
            [StringValue("20 мВ")] Range20mV,
            [StringValue("200 мВ")] Range200mV,
            [StringValue("2 В")] Range2V,
            [StringValue("20 В ")] Range20V,
            [StringValue("200 В")] Range200V,
            [StringValue("750 В")] Range750V,
            [StringValue("1000 В")] Range1000V,
            [StringValue("20 мА")] Range20mA,
            [StringValue("200 мА")] Range200mA,
            [StringValue("400 мА")] Range400mA,
            [StringValue("2 А")] Range2A,
            [StringValue("10 А")] Range10A,
            [StringValue("200 Ом")] Range200Ohm,
            [StringValue("2 кОм")] Range2kOhm,
            [StringValue("20 кОм")] Range20kOhm,
            [StringValue("200 кОм")] Range200kOhm,
            [StringValue("2 МОм")] Range2Mohm,
            [StringValue("20 МОм")] Range20Mohm,
            [StringValue("200 МОм")] Range200Mohm,
            [StringValue("2 ГОм")] Range2Gohm,
            [StringValue("4 нФ")] Range4nF,
            [StringValue("40 нФ")] Range40nF,
            [StringValue("400 нФ")] Range400nF,
            [StringValue("4 мкФ")] Range4uF,
            [StringValue("40 мкФ")] Range40uF,
            [StringValue("400 мкФ")] Range400uF,
            [StringValue("4 мФ")] Range4mF,
            [StringValue("40 мФ")] Range40mF,
            [StringValue("20 Гц")] Range20Hz,
            [StringValue("200 Гц ")] Range200Hz,
            [StringValue("2 кГц")] Range2kHz,
            [StringValue("20 кГц")] Range20kHz,
            [StringValue("200 кГц")] Range200kHz,
            [StringValue("1 МГц")] Range1MHz,
            [StringValue("400 ℃")] Range400degC,
            [StringValue("400 ℉")] Range400degF,
            [StringValue("1200 ℃")] Range1200degC,
            [StringValue("2192 ℉")] Range2192degF,
            [StringValue("предел не установлен")] RangeNone
        }

        /// <summary>
        /// Режимы переключения пределов ручной/авто.
        /// </summary>
        public enum RangeSwitchMode
        {
            Manual = 0x80,
            Auto = 0x00
        }

        /// <summary>
        /// Положение переключатиля
        /// </summary>
        public enum Rotor
        {
            OFF = 0x00,
            [StringValue("В")] V = 0x01,
            [StringValue("мВ")] mV = 0x02,
            [StringValue("Ом")] Ohm = 0x03,
            [StringValue("В")] Diode = 0x04,
            [StringValue("мА")] mA = 0x05,
            [StringValue("А")] A = 0x06,
            [StringValue("Ф")] Cap = 0x07,
            [StringValue("Гц")] Hz = 0x08,
            Temp = 0x09
        }

        /// <summary>
        /// Единицы измерения мультиметра
        /// </summary>
        public enum Units
        {
            [DoubleValue(1)] [StringValue("")] None,
            [DoubleValue(1)] [StringValue("В")] V,

            [DoubleValue(1E-3)]
            [StringValue("мВ")]
            mV,

            [DoubleValue(1)] [StringValue("А")] A,

            [DoubleValue(1E-3)]
            [StringValue("мА")]
            mA,

            [DoubleValue(1)] [StringValue("дБ")] dB,
            [DoubleValue(1)] [StringValue("дБм")] dBm,

            [DoubleValue(1E-9)]
            [StringValue("нФ")]
            nF,

            [DoubleValue(1E-6)]
            [StringValue("мкФ")]
            uF,

            [DoubleValue(1E-3)]
            [StringValue("мФ")]
            mF,

            [DoubleValue(1)] [StringValue("Ом")] Ohm,

            [DoubleValue(1E3)]
            [StringValue("кОм")]
            KOhm,

            [DoubleValue(1E6)]
            [StringValue("МОм")]
            MOhm,

            [DoubleValue(1E9)]
            [StringValue("ГОм")]
            GOhm,

            [DoubleValue(1)] [StringValue("%")] Percent,
            [DoubleValue(1)] [StringValue("Гц")] Hz,

            [DoubleValue(1E3)]
            [StringValue("кГц")]
            KHz,

            [DoubleValue(1E6)]
            [StringValue("МГц")]
            MHz,

            [DoubleValue(1)] [StringValue("⁰C")] CelciumGrad,
            [DoubleValue(1)] [StringValue("⁰F")] FaringeitGrad,
            [DoubleValue(1)] [StringValue("сек")] Sec,

            [DoubleValue(1E-3)]
            [StringValue("мсек")]
            mSec,

            [DoubleValue(1E-9)]
            [StringValue("нсек")]
            nSec,

            [DoubleValue(1)] [StringValue("В")] Volt,

            [DoubleValue(1E-3)]
            [StringValue("мВ")]
            mVolt,

            [DoubleValue(1)] [StringValue("А")] Amp,

            [DoubleValue(1E-3)]
            [StringValue("мА")]
            mAmp,

            [DoubleValue(1)] [StringValue("Ом")] Ohm2,

            [DoubleValue(1E3)]
            [StringValue("кОм")]
            KOhm2,

            [DoubleValue(1E6)]
            [StringValue("МОм")]
            MOhm3
        }

        //размер посылаемых данных от прибора
        private const byte Cadr = 19;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly AutoResetEvent WaitEvent = new AutoResetEvent(false);

        #region Fields

        private readonly byte[] _readData = { 0x55, 0x55, 0x00, 0x0E };

        //хранит считанные с прибора данные
        private readonly List<byte> _readingBuffer;

        //данные для начало обмена информацией с прибором
        private readonly byte[] _sendData = { 0x55, 0x55, 0x00, 0x00, 0xAA };

        private readonly Timer _wait;
        private List<byte> _data;
        private bool _flagTimeout;

        #endregion Fields

        #region Property

        /// <summary>
        /// Возвращает единицы измерения со второго экрана.
        /// </summary>
        public Units GeSubMeasureUnit
        {
            get
            {
                SendQuery();
                Logger.Debug($"Единицы измерения на втором экране {((Units)(_data[16] >> 3)).ToString()}");
                return (Units)(_data[16] >> 3);
            }
        }

        public BlueState GetBlueState
        {
            get
            {
                SendQuery();
                Logger.Debug($"Статус синей клавиши {UserType} {((BlueState)_data[5]).ToString()}");
                return (BlueState)_data[5];
            }
        }

        /// <summary>
        /// Позволяет получить статус включенных дополнительных функций прибора
        /// </summary>
        public Function GetGeneralFunction
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} дополнительная функция {((Function)_data[12]).ToString()}");
                return (Function)_data[12];
            }
        }

        /// <summary>
        /// Позволяет получить информацию о текущих единицах измерения с основного экрана.
        /// </summary>
        public Units GetGeneralMeasureUnit
        {
            get
            {
                Logger.Debug($"Единицы измерени на основном экране {((Units)(_data[11] >> 3)).ToString()}");
                return (Units)(_data[11] >> 3);
            }
        }

        /// <summary>
        /// Возвращает информацию о текущем режиме измерения прибора
        /// </summary>
        public MeasureMode GetMeasureMode
        {
            get
            {
                var currBlueState = GetBlueState;
                var currRotor = GetRotor;

                if (currRotor == Rotor.V && currBlueState == BlueState.NoPress) return MeasureMode.ACV;
                if (currRotor == Rotor.V && currBlueState == BlueState.OnPress) return MeasureMode.DCV;
                if (currRotor == Rotor.V && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_V;

                if (currRotor == Rotor.mV && currBlueState == BlueState.NoPress) return MeasureMode.ACmV;
                if (currRotor == Rotor.mV && currBlueState == BlueState.OnPress) return MeasureMode.DCmV;
                if (currRotor == Rotor.mV && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_mV;

                if (currRotor == Rotor.Ohm && currBlueState == BlueState.NoPress) return MeasureMode.Ohm;
                if (currRotor == Rotor.Ohm && currBlueState == BlueState.OnPress) return MeasureMode.LowOhm;

                if (currRotor == Rotor.Diode && currBlueState == BlueState.NoPress) return MeasureMode.Diode;
                if (currRotor == Rotor.Diode && currBlueState == BlueState.OnPress) return MeasureMode.Beeper;

                if (currRotor == Rotor.mA && currBlueState == BlueState.NoPress) return MeasureMode.ACmA;
                if (currRotor == Rotor.mA && currBlueState == BlueState.OnPress) return MeasureMode.DCmA;
                if (currRotor == Rotor.mA && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_mA;

                if (currRotor == Rotor.A && currBlueState == BlueState.NoPress) return MeasureMode.ACI;
                if (currRotor == Rotor.A && currBlueState == BlueState.OnPress) return MeasureMode.DCI;
                if (currRotor == Rotor.A && currBlueState == BlueState.DoublePress) return MeasureMode.AC_DC_I;

                if (currRotor == Rotor.Cap && currBlueState == BlueState.NoPress) return MeasureMode.Cap;

                if (currRotor == Rotor.Hz && currBlueState == BlueState.NoPress) return MeasureMode.Herz;
                if (currRotor == Rotor.Hz && currBlueState == BlueState.OnPress) return MeasureMode.DutyFactor;

                if (currRotor == Rotor.Temp && currBlueState == BlueState.NoPress) return MeasureMode.degC;
                if (currRotor == Rotor.Temp && currBlueState == BlueState.OnPress) return MeasureMode.DegF;

                return MeasureMode.None;
            }
        }

        public RangeCode GetRangeCode
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} диапазон измерения {((RangeCode)_data[7]).ToString()}");
                return (RangeCode)_data[7];
            }
        }

        /// <summary>
        /// Возвращает номинал текущего предела измерения
        /// </summary>
        public RangeNominal GetRangeNominal
        {
            get
            {
                var currMode = GetMeasureMode;
                var currRangeCode = GetRangeCode;

                if (currMode == MeasureMode.DCV)
                {
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range1000V;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200V;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20V;
                    return RangeNominal.Range2V;
                }

                if (currMode == MeasureMode.ACV)
                {
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range750V;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200V;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20V;
                    return RangeNominal.Range2V;
                }

                if (currMode == MeasureMode.AC_DC_V)
                {
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range750V;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200V;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20V;
                    return RangeNominal.Range2V;
                }

                if (currMode == MeasureMode.DCmV || currMode == MeasureMode.ACmV || currMode == MeasureMode.AC_DC_mV)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range200mV;
                    return RangeNominal.Range20mV;
                }

                if (currMode == MeasureMode.DCmA || currMode == MeasureMode.ACmA || currMode == MeasureMode.AC_DC_mA)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range200mA;
                    return RangeNominal.Range20mA;
                }

                if (currMode == MeasureMode.DCI || currMode == MeasureMode.ACI || currMode == MeasureMode.AC_DC_I)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range10A;
                    return RangeNominal.Range2A;
                }

                if (currMode == MeasureMode.Ohm)
                {
                    if (((int)currRangeCode & 7) == 7) return RangeNominal.Range2Gohm;
                    if (((int)currRangeCode & 6) == 6) return RangeNominal.Range200Mohm;
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range20Mohm;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range2Mohm;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range200kOhm;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range2kOhm;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range20kOhm;
                    return RangeNominal.Range200Ohm;
                }

                if (currMode == MeasureMode.LowOhm)
                {
                    if (((int)currRangeCode & 6) == 6) return RangeNominal.Range2Gohm;
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range200Mohm;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range20Mohm;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range2Mohm;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range200kOhm;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range20kOhm;
                    return RangeNominal.Range2kOhm;
                }

                if (currMode == MeasureMode.Cap)
                {
                    if (((int)currRangeCode & 7) == 7) return RangeNominal.Range40mF;
                    if (((int)currRangeCode & 6) == 6) return RangeNominal.Range4mF;
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range400uF;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range40uF;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range4uF;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range400nF;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range40nF;
                    return RangeNominal.Range4nF;
                }

                if (currMode == MeasureMode.Herz)
                {
                    if (((int)currRangeCode & 5) == 5) return RangeNominal.Range1MHz;
                    if (((int)currRangeCode & 4) == 4) return RangeNominal.Range200kHz;
                    if (((int)currRangeCode & 3) == 3) return RangeNominal.Range20kHz;
                    if (((int)currRangeCode & 2) == 2) return RangeNominal.Range2kHz;
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range200Hz;
                    return RangeNominal.Range20Hz;
                }

                if (currMode == MeasureMode.degC)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range1200degC;
                    return RangeNominal.Range400degC;
                }

                if (currMode == MeasureMode.DegF)
                {
                    if (((int)currRangeCode & 1) == 1) return RangeNominal.Range2192degF;
                    return RangeNominal.Range400degF;
                }

                return RangeNominal.RangeNone;
            }
        }

        /// <summary>
        /// Позволяет получить режим переключения пределов Auto/Manual
        /// </summary>
        public RangeSwitchMode GetRangeSwitchMode
        {
            get
            {
                if (((int)GetRangeCode & (int)RangeSwitchMode.Manual) == (int)RangeSwitchMode.Manual)
                    return RangeSwitchMode.Manual;
                return RangeSwitchMode.Auto;
            }
        }

        public Rotor GetRotor
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} положение переключателя {((Rotor)_data[4]).ToString()}");
                return (Rotor)_data[4];
            }
        }

        /// <summary>
        /// Позволяет получить дополнительные функции, включенные на дополнительном экране.
        /// </summary>
        public Function GetSubFunction
        {
            get
            {
                SendQuery();
                Logger.Info($"{UserType} дополнительные функции на втором экране {((Function)_data[17]).ToString()}");
                return (Function)_data[17];
            }
        }

        #endregion Property

        public Mult107_109N()
        {
            _wait = new Timer();
            _readingBuffer = new List<byte>();
            _data = new List<byte>();
            _flagTimeout = false;
            _wait.Interval = 35000;
            _wait.Elapsed += TWait_Elapsed;
            _wait.AutoReset = false;
            //this.StringConnection = null;
        }

        #region Methods

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Возвращает одно измеренное значение с прибора, приведенное к единицам в соответствии с множителем mult
        /// </summary>
        /// <param name = "mult">Множитель единицы измерения.</param>
        /// <param name = "generalDsiplay">
        /// Если флаг true, тогда возвращаются показания с основного экрана прибора. Иначе с
        /// второстепенного.
        /// </param>
        /// <returns></returns>
        public double GetSingleValue(UnitMultiplier mult = UnitMultiplier.None, bool generalDsiplay = true)
        {
            SendQuery();
            double value;

            // по умолчанию запрашиваем показания с главного экрана
            if (generalDsiplay)
            {
                if (_data[10] == 255)
                    value = ~(((0xff - _data[10]) << 16) | ((0xff - _data[9]) << 8) | (0xff - _data[8])) - 1;
                else value = (_data[10] << 16) | (_data[9] << 8) | _data[8];
                value = GetPointInfo(value, _data[11]);
            }
            //если запрашиваются показания со второго экрана
            else
            {
                if (_data[15] == 255)
                    value = ~(((0xff - _data[15]) << 16) | ((0xff - _data[14]) << 8) | (0xff - _data[13])) - 1;
                else value = (_data[15] << 16) | (_data[14] << 8) | _data[13];
                value = GetPointInfo(value, _data[16]);
            }

            double GetPointInfo(double val, byte addres)
            {
                switch (addres & 0x07)
                {
                    case (int)Point.Point1:
                        return val /= 10.0;

                    case (int)Point.Point2:
                        return val /= 100.0;

                    case (int)Point.Point3:
                        return val /= 1000.0;

                    case (int)Point.Point4:
                        return val /= 10000.0;

                    default:
                        return val;
                }
            }

            Logger.Info($"{UserType} Измеренное значение {value}");
            return value;
        }

        /// <summary>
        /// Возвращает среднее арифметическое после countOfMeasure измерений. Исключает из выборки выбросы.
        /// </summary>
        /// <param name = "countOfMeasure">Число необходимых измерений.</param>
        /// <param name = "mult">Множитель единицы измерений (миллиб кило и т.д.)</param>
        /// <param name = "generalDsiplay">
        /// Если флаг true, тогда возвращаются показания с основного экрана прибора. Иначе с
        /// второстепенного.
        /// </param>
        /// <returns>Среднее арифметическое на основе выборки из countOfMeasure измерений.</returns>
        public double GetValue(int countOfMeasure = 10, UnitMultiplier mult = UnitMultiplier.None,
            bool generalDsiplay = true)
        {
            double resultValue;

            var valBuffer = new decimal[countOfMeasure];
            do
            {
                for (var i = 0; i < valBuffer.Length; i++)
                    valBuffer[i] = (decimal)GetSingleValue(mult, generalDsiplay);
            } while (!MathStatistics.IntoTreeSigma(valBuffer));

            MathStatistics.Grubbs(ref valBuffer);
            resultValue = (double)MathStatistics.GetArithmeticalMean(valBuffer);

            return resultValue;
        }

        public void SendQuery()
        {
            //проверка на признак открытого порта
            if (!IsOpen)
                // открываем порт
                Open();

            // отправляем запрос
            Write(_sendData, 0, _sendData.Length);
            _wait.Start();

            WaitEvent.WaitOne();
            if (_flagTimeout)
            {
                _flagTimeout = false;
                Logger.Debug($"{UserType} не отвечает.");
                throw new TimeoutException($"{UserType} не отвечает.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _wait?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Получает данные с прибора, записывает их в буфер для проверки контрольной суммы.
        /// </summary>
        /// <param name = "sender">Устройство передающее данные</param>
        /// <param name = "e"></param>
        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var CountResend = 0;
            _readingBuffer.Clear();
            var port = (SerialPort)sender;

            try
            {
                for (var i = 0; i < _readData.Length; i++)
                {
                    var bt = (byte)port.ReadByte();
                    if (bt == _readData[i])
                    {
                        _readingBuffer.Add(bt);
                    }
                    else
                    {
                        DiscardInBuffer();
                        _readingBuffer.Clear();
                        CountResend++;
                        SendQuery();
                        if (CountResend == 5)
                        {
                            Logger.Error($"Было выполнено 5 неудачных попыток запроса данных с {UserType}");
                            throw new
                                TimeoutException($"Было выполнено 5 неудачных попыток запроса данных с {UserType}");
                        }

                        i = -1;
                    }
                }

                while (_readingBuffer.Count < Cadr) _readingBuffer.Add((byte)port.ReadByte());

                DiscardInBuffer();

                Close();
                CheckControlSumm();
            }
            catch (Exception a)
            {
                Logger.Error(a);
            }
        }

        /// <summary>
        /// Проверка контрольной суммы полученных данных
        /// </summary>
        /// <returns></returns>
        private void CheckControlSumm()
        {
            if (_readingBuffer.Count != Cadr) return;
            var chechSum = 0;
            for (var i = 0; i < _readingBuffer.Count - 1; i++) chechSum = chechSum + _readingBuffer[i];

            /*берем последние биты*/
            var match = chechSum & 0xFF;
            Logger.Debug($"{StringConnection}:  Контрольная сумма считанных данных: {match.ToString("X")} а ожидается {_readingBuffer[_readingBuffer.Count - 1].ToString("X")}");
            if (match != _readingBuffer[_readingBuffer.Count - 1])
            {
                WaitEvent.Reset();

                SendQuery();
                return;
            }

            //если сумма совпадает тогда считанные данные из буфера пишем в поле
            _data = _readingBuffer;
            _wait.Stop();
            WaitEvent.Set();
        }

        private void TWait_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flagTimeout = true;
            WaitEvent.Set();
        }

        #endregion Methods

        private enum Point
        {
            None = 0,
            Point1 = 0x01,
            Point2 = 0x02,
            Point3 = 0x03,
            Point4 = 0x04
        }
    }

    public class MultAPPA107N : Mult107_109N
    {
        public MultAPPA107N()
        {
            UserType = "APPA-107N";
        }
    }

    public class MultAPPA109N : Mult107_109N
    {
        public MultAPPA109N()
        {
            UserType = "APPA-109N";
        }
    }
}