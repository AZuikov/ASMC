// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AP.Extension;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Keysight.Multimeter;
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public abstract class CalibrMain: ICalibratorMultimeterFlukeBase
    {

        #region ErrosCatch

        protected internal abstract void CheckErrors();
        

        protected abstract string GetError();
        protected abstract string GetLastErrorCode();
       

        #endregion ErrosCatch

        public IeeeBase Device { get; }
        protected CalibrMain()
        {
            Device= new IeeeBase();
            DcVoltage = new DcVolt(this);
            AcVoltage = new AcVolt(this);
            DcCurrent = new DcCurr(this);
            AcCurrent = new AcCurr(this);
            Resistance2W = new Resist2W(this);
            Frequency = new Freq(this);
            
        }
        /// <inheritdoc />
        public ISourcePhysicalQuantity<Voltage> DcVoltage { get; protected set; }
        /// <inheritdoc />
        public ISourcePhysicalQuantity<Voltage, Frequency> AcVoltage { get; protected set; }
        /// <inheritdoc />
        public ISourcePhysicalQuantity<Current> DcCurrent { get; protected set; }
        /// <inheritdoc />
        public ISourcePhysicalQuantity<Current, Frequency> AcCurrent { get; protected set; }
        /// <inheritdoc />
        public IResistance Resistance2W { get; protected set; }
        /// <inheritdoc />
        public ISourcePhysicalQuantity<Capacity> Capacity { get; protected set; }
        /// <inheritdoc />
        public ITermocoupleType Temperature { get; }

        /// <inheritdoc />
        public ISourcePhysicalQuantity<Frequency, Voltage> Frequency { get; }


        public class DcVolt : SimplyPhysicalQuantity<Voltage>
        {
            public DcVolt(CalibrMain device) : base(device)
            {
                RangeStorage = new RangeDevice();
            }

            #region Methods

            protected override string GetUnit()
            {
                return "V, 0Hz";
            }

            

            #endregion

            public class RangeDevice : RangeDeviceBase<Voltage>
            {
                [AccRange("Mode: Volts", typeof(MeasPoint<Voltage>))]
                public override RangeStorage<PhysicalRange<Voltage>> Ranges { get; set; }
                
            }

        }

        public class AcVolt : ComplexPhysicalQuantity<Voltage, Frequency>
        {
            public AcVolt(CalibrMain device) : base(device)
            {
                this.RangeStorage = new RangeDevice();
            }

            #region Methods

            protected override string GetMainUnit()
            {
                return "V";
            }

            /// <inheritdoc />
            protected override string GetAdditionalUnit()
            {
                return "Hz";
            }

            #endregion
            public class RangeDevice : RangeDeviceBase<Voltage, Frequency>
            {
                [AccRange("Mode: Volts SI", typeof(MeasPoint<Voltage, Frequency>))]
                public override RangeStorage<PhysicalRange<Voltage, Frequency>> Ranges { get; set; }

            }
        }

        public class DcCurr : SimplyPhysicalQuantity<Current>
        {
            public DcCurr(CalibrMain device) : base(device)
            {
                RangeStorage = new RangeDevice();
            }

            #region Methods

            protected override string GetUnit()
            {
                return "A, 0Hz";
            }

            #endregion
            public class RangeDevice : RangeDeviceBase<Current>
            {
                [AccRange("Mode: Amps", typeof(MeasPoint<Current>))]
                public override RangeStorage<PhysicalRange<Current>> Ranges { get; set; }

            }
        }

        public class AcCurr : ComplexPhysicalQuantity<Current, Frequency>
        {
            public AcCurr(CalibrMain device) : base(device)
            {
                RangeStorage  = new RangeDevice();
            }

            #region Methods

            protected override string GetMainUnit()
            {
                return "A";
            }

            /// <inheritdoc />
            protected override string GetAdditionalUnit()
            {
                return "Hz";
            }

            #endregion

            public class RangeDevice : RangeDeviceBase<Current, Frequency>
            {
                public override RangeStorage<PhysicalRange<Current, Frequency>> Ranges { get; set; }
                
                
                //todo склеить два диапазона в один?
                [AccRange("Mode: Amps SI", typeof(MeasPoint<Current, Frequency>))]
                public  RangeStorage<PhysicalRange<Current, Frequency>> Ranges1 { get; set; }

                [AccRange("Mode: Amps SI HC", typeof(MeasPoint<Current, Frequency>))]
                public  RangeStorage<PhysicalRange<Current, Frequency>> Ranges2 { get; set; }

            }

        }

        public class Cap: SimplyPhysicalQuantity<Capacity>
        {
            public Cap(CalibrMain device) : base(device)
            {
                RangeStorage = new RangeDevice();
            }

            protected override string GetUnit()
            {
                return "F";
            }

            public class RangeDevice : RangeDeviceBase<Capacity>
            {
                #region Property

                [AccRange("Mode: Farads 2W", typeof(MeasPoint<Capacity>))]
                public override RangeStorage<PhysicalRange<Capacity>> Ranges { get; set; }

                #endregion
            }
        }

        public class Temp : SimplyPhysicalQuantity<Temperature>, ITermocoupleType
        {
            public Temp(CalibrMain device) : base(device)
            {
            }

            #region Methods

            protected override string GetUnit()
            {
                return "CEL";
            }

            #endregion

            public class RangeDevice : RangeDeviceBase<Temperature>
            {
                #region Property

                [AccRange("Mode: Amps", typeof(MeasPoint<Temperature>))]
                public override RangeStorage<PhysicalRange<Temperature>> Ranges { get; set; }

                #endregion
            }

            public ICommand[] TermoCoupleType { get; set; }

            public void SetTermoCoupleType(TypeTermocouple typeTermocouple)
            {
                Calibrator.Device.WriteLine("TC_TYPE " + typeTermocouple.GetStringValue());
                this.Calibrator.CheckErrors();
            }

            public ITermocoupleType Temperature { get; }
        }

        public abstract class Resist : SimplyPhysicalQuantity<Resistance>, IResistance
        {
            protected Resist(CalibrMain device) : base(device)
            {
                
            }

            public ICommand[] CompensationMode { get; set; }
            public void SetCompensation(Compensation compMode)
            {
                var command = CompensationMode.FirstOrDefault(q => (int)q.Value == (int) compMode);
                Calibrator.Device.WriteLine(command.StrCommand);
                Calibrator.CheckErrors();
            }
        }
        public class Resist2W :Resist
        {
           

            public Resist2W(CalibrMain device) : base(device)
            {
                RangeStorage = new RangeDevice();
                CompensationMode = new ICommand[]
                {
                    new Command("ZCOMP NONE","Отключает компенсации",0),
                    new Command("ZCOMP WIRE2","2х проводная компенсация",2)
                    
                };
            }

            
            #region Methods

            protected override string GetUnit()
            {
                return "OHM";
            }

            

           
            #endregion

            public class RangeDevice : RangeDeviceBase<Resistance>
            {
                [AccRange("Mode: Ohms 2W", typeof(MeasPoint<Resistance>))]
                public override RangeStorage<PhysicalRange<Resistance>> Ranges { get; set; }

            }
        }

        public abstract class SimplyPhysicalQuantity<TPhysicalQuantity> : 
            OutputControl, ISourcePhysicalQuantity<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            protected SimplyPhysicalQuantity(CalibrMain device) : base(device)
            {
                
            }

            #region Methods

            protected string ConvetrMeasPointToCommand(MeasPoint<TPhysicalQuantity> value)
            {
                var point = (MeasPoint<TPhysicalQuantity>) value.Clone();
                var unit = point.MainPhysicalQuantity.Unit.GetStringValue();
                point.MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.None);
                return point.Description.Replace(',', '.').Replace(unit, GetUnit());
            }

            /// <summary>
            /// Единица измерения физической величины калибратора.
            /// </summary>
            /// <returns></returns>
            protected abstract string GetUnit();

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
                Calibrator.Device.WriteLine(@"OUT " + ConvetrMeasPointToCommand(value));
                Calibrator.Device.WaitingRemoteOperationComplete();
                Calibrator.CheckErrors();
            }

            
            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; protected set; }
        }

        public abstract class ComplexPhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> : OutputControl,
                                                                                               ISourcePhysicalQuantity<TPhysicalQuantity,
                                                                                                   TPhysicalQuantity2>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
            where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
        {
            /// <summary>
            /// Поиск пробела между занчениями двух физических величин.
            /// </summary>
            private static readonly Regex regex = new Regex(@"(?!\w) (?=\d)");

            protected ComplexPhysicalQuantity(CalibrMain device) : base(device)
            {
                
            }

            #region Methods

            protected string ConvetrMeasPointToCommand(MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> value)
            {
                var point = (MeasPoint<TPhysicalQuantity, TPhysicalQuantity2>) value.Clone();
                var unit = point.MainPhysicalQuantity.Unit.GetStringValue();
                var unitadd = point.AdditionalPhysicalQuantity.Unit.GetStringValue(/*CultureInfo.GetCultureInfo("en-US")*/);
                point.MainPhysicalQuantity.ChangeMultiplier(UnitMultiplier.None);
                point.AdditionalPhysicalQuantity.ChangeMultiplier(UnitMultiplier.None);
                var returnCommand = point.Description.Replace(unit, GetMainUnit()).Replace(unitadd, GetAdditionalUnit())
                                         .Replace(',', '.');
                returnCommand = regex.Replace(returnCommand, ", ");
                return returnCommand;
            }

            /// <summary>
            /// Единица измерения основной физической величины калибратора.
            /// </summary>
            /// <returns></returns>
            protected abstract string GetMainUnit();

            /// <summary>
            /// Единица измерениядополнительной физической величины калибратора.
            /// </summary>
            /// <returns></returns>
            protected abstract string GetAdditionalUnit();
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
                Calibrator.Device.WriteLine(@"OUT " + ConvetrMeasPointToCommand(value));
                Calibrator.Device.WaitingRemoteOperationComplete();
                Calibrator.CheckErrors();
            }

           

            public IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; protected set; }
        }

        public abstract class OutputControl
        {
            #region Fields

            protected CalibrMain Calibrator { get; }

            #endregion

            #region Property

            public bool IsEnableOutput { get; }

            #endregion

            protected OutputControl(CalibrMain device)
            {
                Calibrator = device;
            }

            #region Methods

           

            public void OutputOff()
            {
                Calibrator.Device.WriteLine(State.Off.GetStringValue());
                Calibrator.Device.WaitingRemoteOperationComplete();
            }

            public void OutputOn()
            {
                Calibrator.Device.WriteLine(State.On.GetStringValue());
                Calibrator.Device.WaitingRemoteOperationComplete();
            }

           



            #endregion

        private enum State
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

        }

       public string UserType { get; protected set; }
        public string StringConnection
        {
            get => Device.StringConnection;
            set => Device.StringConnection =value;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }
        public async Task InitializeAsync()
        {
            var fileName = @$"{Environment.CurrentDirectory}\acc\{UserType}.acc";
            if(!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
        }

        public class Freq : ComplexPhysicalQuantity<Frequency, Voltage>
        {
            public Freq(CalibrMain device) : base(device)
            {
                this.RangeStorage = new RangeDevice();
            }

            #region Methods

            protected override string GetMainUnit()
            {
                return "Hz";
            }

            /// <inheritdoc />
            protected override string GetAdditionalUnit()
            {
                return "V";
            }

            #endregion
            public class RangeDevice : RangeDeviceBase<Frequency, Voltage>
            {
                [AccRange("Mode: Hertz", typeof(MeasPoint<Frequency, Voltage>))]
                public override RangeStorage<PhysicalRange<Frequency, Voltage>> Ranges { get; set; }

            }
        }
    }

    /// <summary>
    /// Перечисление кодов ошибок для калибратора 5522А.
    /// </summary>
   
}