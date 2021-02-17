using System;
using System.IO;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calibr_9100 : IVoltageGroupForCalibrator, ICurrnetGroupForCalibrator, IResistance2W, IResistance4W,
                               ICapacity,
                               ITemperature, IProtocolStringLine
    {
        #region Property

        public IeeeBase Device { get; }

        #endregion

        public Calibr_9100()
        {
            UserType = "Fluke 9100";
            Device = new IeeeBase();
            DcVoltage = new DcVolt(this);
            AcVoltage = new AcVolt(this);
            DcCurrent = new DcCurr(this);
            AcCurrent = new AcCurr(this);
        }

        public ISourcePhysicalQuantity<Capacity> Capacity { get; }
        public ISourcePhysicalQuantity<Current> DcCurrent { get; }
        public ISourcePhysicalQuantity<Current, Frequency> AcCurrent { get; }
        public string UserType { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
            var fileName = @$"{Environment.CurrentDirectory}acc\{UserType}.acc";
            if (!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
        }

        public string StringConnection
        {
            get => Device.StringConnection;
            set => Device.StringConnection = value;
        }

        public IResistance Resistance2W { get; }
        public ISourcePhysicalQuantity<Resistance> Resistance4W { get; }
        public ITermocoupleType Temperature { get; }
        public ISourcePhysicalQuantity<Voltage> DcVoltage { get; }
        public ISourcePhysicalQuantity<Voltage, Frequency> AcVoltage { get; }

        public class DcVolt : SimplyPhysicalQuantity<Voltage>
        {
            public DcVolt(Calibr_9100 device) : base(device)
            {
                functionName = "DC";
                sourceName = "VOLT";
                RangeStorage = new RangeDevice();
            }

            public class RangeDevice : RangeDeviceBase<Voltage>
            {
                #region Property

                [AccRange("Mode: Volts", typeof(MeasPoint<Voltage>))]
                public override RangeStorage<PhysicalRange<Voltage>> Ranges { get; set; }

                #endregion
            }
        }

        public class DcCurr : SimplyPhysicalQuantity<Current>
        {
            public DcCurr(Calibr_9100 device) : base(device)
            {
                functionName = "DC";
                sourceName = "CURRent";
                RangeStorage = new RangeDevice();
            }

            public class RangeDevice : RangeDeviceBase<Current>
            {
                #region Property

                [AccRange("Mode: Amps", typeof(MeasPoint<Current>))]
                public override RangeStorage<PhysicalRange<Current>> Ranges { get; set; }

                #endregion
            }
        }

        public class AcCurr : ComplexPhysicalQuantity<Current, Frequency>
        {
            public AcCurr(Calibr_9100 device) : base(device)
            {
                functionName = "sin";
                mainSourceName = "CURRent";
                additionalSourceName = "freq";
                RangeStorage = new RangeDevice();
            }

            #region Methods

            protected override string GetAdditionalUnit()
            {
                throw new NotImplementedException();
            }

            protected override string GetMainUnit()
            {
                throw new NotImplementedException();
            }

            #endregion

            public class RangeDevice : RangeDeviceBase<Current, Frequency>
            {
                #region Property

                [AccRange("Mode: Amps SI", typeof(MeasPoint<Current, Frequency>))]
                public override RangeStorage<PhysicalRange<Current, Frequency>> Ranges { get; set; }

                #endregion
            }
        }

        public class AcVolt : ComplexPhysicalQuantity<Voltage, Frequency>
        {
            public AcVolt(Calibr_9100 device) : base(device)
            {
                functionName = "sin";
                mainSourceName = "VOLT";
                additionalSourceName = "freq";
                RangeStorage = new RangeDevice();
            }

            #region Methods

            protected override string GetAdditionalUnit()
            {
                throw new NotImplementedException();
            }

            protected override string GetMainUnit()
            {
                throw new NotImplementedException();
            }

            #endregion

            public class RangeDevice : RangeDeviceBase<Voltage, Frequency>
            {
                #region Property

                [AccRange("Mode: Volts SI", typeof(MeasPoint<Voltage, Frequency>))]
                public override RangeStorage<PhysicalRange<Voltage, Frequency>> Ranges { get; set; }

                #endregion
            }
        }

        public abstract class SimplyPhysicalQuantity<TPhysicalQuantity> :
            OutputControl, ISourcePhysicalQuantity<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            #region Property

            protected string sourceName { get; set; }

            #endregion

            protected SimplyPhysicalQuantity(Calibr_9100 device) : base(device)
            {
            }

            #region Methods

            protected string ConvetrMeasPointToCommand(MeasPoint<TPhysicalQuantity> value)
            {
                var point = (MeasPoint<TPhysicalQuantity>) value.Clone();
                var unit = point.MainPhysicalQuantity.Unit.GetStringValue();
                point.MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.None);
                return point.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.');
            }

            #endregion

            public virtual void Getting()
            {
            }

            public virtual void Setting()
            {
                throw new NotImplementedException();
            }

            public MeasPoint<TPhysicalQuantity> Value { get; protected set; }

            public virtual void SetValue(MeasPoint<TPhysicalQuantity> value)
            {
                Value = value;
                Calibr.Device.WriteLine($"Source:func {functionName}");
                Calibr.Device.WriteLine($"Source:{sourceName} {ConvetrMeasPointToCommand(value)}");
                //todo проверка на ошибки после отправки команды
            }

            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; protected set; }
        }

        public abstract class ComplexPhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> : OutputControl,
                                                                                               ISourcePhysicalQuantity<
                                                                                                   TPhysicalQuantity,
                                                                                                   TPhysicalQuantity2>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
            where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
        {
            #region Property

            protected string additionalSourceName { get; set; }
            protected string mainSourceName { get; set; }

            #endregion

            protected ComplexPhysicalQuantity(Calibr_9100 device) : base(device)
            {
            }

            #region Methods

            protected string[] ConvetrMeasPointToCommand(MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> inPoint)
            {
                var values = new string[2];
                values[0] = inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.');
                values[1] = inPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.');
                return values;
            }

            /// <summary>
            /// Единица измерениядополнительной физической величины калибратора.
            /// </summary>
            /// <returns></returns>
            protected abstract string GetAdditionalUnit();

            /// <summary>
            /// Единица измерения основной физической величины калибратора.
            /// </summary>
            /// <returns></returns>
            protected abstract string GetMainUnit();

            #endregion

            public virtual void Getting()
            {
                throw new NotImplementedException();
            }

            public virtual void Setting()
            {
                throw new NotImplementedException();
            }

            public MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> Value { get; protected set; }

            public virtual void SetValue(MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> value)
            {
                Value = value;
                var arr = ConvetrMeasPointToCommand(value);
                Calibr.Device.WriteLine($"Source:func {functionName}");
                Calibr.Device.WriteLine($"Source:{mainSourceName} {arr[0]}");
                Calibr.Device.WriteLine($"Source:{additionalSourceName} {arr[1]}");
                //todo проверка на ошибки после отправки команды
            }

            public IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; protected set; }
        }

        public abstract class OutputControl
        {
            #region Property

            public bool IsEnableOutput { get; }
            protected Calibr_9100 Calibr { get; }

            protected string functionName { get; set; }

            #endregion

            protected OutputControl(Calibr_9100 device)
            {
                Calibr = device;
            }

            #region Methods

            public void OutputOff()
            {
                Calibr.Device.WriteLine(State.Off.GetStringValue());
                //todo проверка на ошибки после отправки команды
            }

            public void OutputOn()
            {
                Calibr.Device.WriteLine(State.On.GetStringValue());
                //todo проверка на ошибки после отправки команды
            }

            #endregion

            private enum State
            {
                /// <summary>
                /// Включить выход
                /// </summary>
                [StringValue("outp:stat on")] On,

                /// <summary>
                /// Выключить выход
                /// </summary>
                [StringValue("outp:stat off")] Off
            }
        }
    }
}

/* Remove this code!!!!!!
 public void SetVoltageDc(MeasPoint<Voltage> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine($"Source:VOLT {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
        }

        public void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint)
        {
            //WriteLine("Source:func sin");
            //WriteLine($"Source:VOLT {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine($"Source:freq {setPoint.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        }

        public void SetResistance2W(MeasPoint<Resistance> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine($"Source:RESistance {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine("SOUR:RES:UUT_I SUP");
            //WriteLine("outp:comp off");
        }

        public void SetResistance4W(MeasPoint<Resistance> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine($"Source:RESistance {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine("SOUR:RES:UUT_I SUPer}");
            //WriteLine("outp:comp on");
        }

        public void SetCurrentDc(MeasPoint<Current> setPoint)
        {
            //WriteLine("Source:func DC");
            //WriteLine("Source:CURRent {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        }

        public void SetCurrentAc(MeasPoint<Current, Frequency> setPoint)
        {
            //WriteLine("Source:func sin");
            //WriteLine($"Source:CURRent {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //WriteLine($"Source:freq {setPoint.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        }

        //public void SetTemperature(MeasPoint<Temperature> setPoint, COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unit)
        //{
        //    //WriteLine("Source:func DC");
        //    //WriteLine($"Source:TEMPerature:THERmocouple:TYPE {typeTermocouple.GetStringValue()}");
        //    //WriteLine($"Source:TEMPerature:UNITs {unit}");
        //    //WriteLine($"Source:TEMPerature:THERmocouple {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        //}

        //public void SetOutputOn()
        //public void SetOutputOn()
        //public void SetOutputOn()
        //public void SetOutputOn()
        //public void SetOutputOn()ICalibratorMultimeterFluke
        //{
        //   //WriteLine("outp:stat on");
        //}

        public void SetOutputOff()
        {
            //WriteLine("outp:stat off");
        }

        public void Reset()
        {
            //WriteLine(IeeeBase.Reset);
        }

        protected override string GetError()
        {
            throw new NotImplementedException();
        }
 *
 *
 */