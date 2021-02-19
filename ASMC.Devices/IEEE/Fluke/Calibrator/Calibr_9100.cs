using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public class Calibr_9100 : ICalibratorMultimeterFlukeBase
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
            Resistance2W = new Resist2W(this);
            Resistance4W = new Resist4W(this);
            Temperature = new Temp(this);
        }


        public IResistance Resistance2W { get; }
        public ISourcePhysicalQuantity<Resistance> Resistance4W { get; }
        public ITermocoupleType Temperature { get; }
        public ISourcePhysicalQuantity<Voltage> DcVoltage { get; }
        public ISourcePhysicalQuantity<Voltage, Frequency> AcVoltage { get; }
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

        public class Resist2W : Resist
        {
            public Resist2W(Calibr_9100 device) : base(device)
            {
                CompensationMode = new[] {new Command("outp:comp off", "компенсация отключена", 0)};
                RangeStorage = new RangeDevice();
            }

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                #region Property

                [AccRange("Mode: Ohms 2W SP", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

                #endregion
            }
        }

        public class Resist4W : Resist
        {
            public Resist4W(Calibr_9100 device) : base(device)
            {
                CompensationMode = new[] { new Command("outp:comp on", "компенсация отключена", 0) };
                RangeStorage = new RangeDevice();
            }

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                #region Property

                [AccRange("Mode: Ohms 4W SP", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

                #endregion
            }
        }

        public abstract class Resist : SimplyPhysicalQuantity<Resistance>, IResistance
        {
            public Resist(Calibr_9100 device) : base(device)
            {
                functionName = "DC";
                sourceName = "RESistance";
               
            }

            public ICommand[] CompensationMode { get; set; }
            public void SetCompensation(Compensation compMode)
            {
                Calibr.Device.WriteLine(CompensationMode[0].StrCommand);
            }
        }

        public class Temp: SimplyPhysicalQuantity<Temperature>, ITermocoupleType
        {
            private ICommand typeSetTermocouple;
            public Temp(Calibr_9100 device) : base(device)
            {
                functionName = "DC";
                sourceName = "TEMPerature";
                /*типы поддерживаемых термопар*/
                
                TermoCoupleType = new[]
                {
                    new Command(TypeTermocouple.B.GetStringValue(), $"термопара типа {TypeTermocouple.B.GetStringValue()}", (int) TypeTermocouple.B),
                    new Command(TypeTermocouple.C.GetStringValue(), $"термопара типа {TypeTermocouple.C.GetStringValue()}", (int) TypeTermocouple.C),
                    new Command(TypeTermocouple.E.GetStringValue(), $"термопара типа {TypeTermocouple.E.GetStringValue()}", (int) TypeTermocouple.E),
                    new Command(TypeTermocouple.J.GetStringValue(), $"термопара типа {TypeTermocouple.J.GetStringValue()}", (int) TypeTermocouple.J),
                    new Command(TypeTermocouple.K.GetStringValue(), $"термопара типа {TypeTermocouple.K.GetStringValue()}", (int) TypeTermocouple.K),
                    new Command(TypeTermocouple.N.GetStringValue(), $"термопара типа {TypeTermocouple.N.GetStringValue()}", (int) TypeTermocouple.N),
                    new Command(TypeTermocouple.R.GetStringValue(), $"термопара типа {TypeTermocouple.R.GetStringValue()}", (int) TypeTermocouple.R),
                    new Command(TypeTermocouple.S.GetStringValue(), $"термопара типа {TypeTermocouple.S.GetStringValue()}", (int) TypeTermocouple.S),
                    new Command(TypeTermocouple.T.GetStringValue(), $"термопара типа {TypeTermocouple.T.GetStringValue()}", (int) TypeTermocouple.T),
                    new Command(TypeTermocouple.L.GetStringValue(), $"термопара типа {TypeTermocouple.L.GetStringValue()}", (int) TypeTermocouple.L),
                };
            }

            //WriteLine("Source:func DC");
            //WriteLine($"Source:TEMPerature:THERmocouple:TYPE {typeTermocouple.GetStringValue()}");
            //WriteLine($"Source:TEMPerature:UNITs {unit}");
            //WriteLine($"Source:TEMPerature:THERmocouple {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");

            public ICommand[] TermoCoupleType { get; set; }

            public override void SetValue(MeasPoint<Temperature> value)
            {
                Value = value;
                Calibr.Device.WriteLine($"Source:func {functionName}");
                Calibr.Device.WriteLine($"Source:{sourceName}:THERmocouple:TYPE {typeSetTermocouple.StrCommand}");
                Calibr.Device.WriteLine($"Source:TEMPerature:UNITs C"); //пока единицы только цельсии
                Calibr.Device.WriteLine($"Source:TEMPerature:THERmocouple {value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
                
            }

            public void SetTermoCoupleType(TypeTermocouple typeTermocouple)
            {
                typeSetTermocouple = TermoCoupleType.FirstOrDefault(q => q.Value == (int)typeTermocouple);
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
                Calibr.Device.WriteLine("SOUR:RES:UUT_I SUP");
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

        public ISourcePhysicalQuantity<Frequency, Voltage> Frequency { get; }
    }
}

/* Remove this code!!!!!!
      

        //public void SetTemperature(MeasPoint<Temperature> setPoint, COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unit)
        //{
        //    //WriteLine("Source:func DC");
        //    //WriteLine($"Source:TEMPerature:THERmocouple:TYPE {typeTermocouple.GetStringValue()}");
        //    //WriteLine($"Source:TEMPerature:UNITs {unit}");
        //    //WriteLine($"Source:TEMPerature:THERmocouple {setPoint.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
        //}
   
        

        
 *
 *
 */