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
using ASMC.Devices.Interface.SourceAndMeter;
using ASMC.Devices.Model;
using Current = ASMC.Data.Model.PhysicalQuantity.Current;

namespace ASMC.Devices.IEEE.Fluke.Calibrator
{
    public abstract class CalibrMain: ICalibratorMultimeterFlukeBase
    {

        #region ErrosCatch

        protected internal abstract void CheckErrors();
        

        protected abstract string GetCommandForErrorQuery();
        protected abstract int GetLastErrorCode();
       

        #endregion ErrosCatch

        protected IeeeBase Device { get; }
        protected CalibrMain()
        {
            Device= new IeeeBase();
            DcVoltage = new DcVolt(Device, this);
            AcVoltage = new AcVolt(Device, this);
            DcCurrent = new DcCurr(Device, this);
            AcCurrent = new AcCurr(Device, this);
            Resistance2W = new Resist2W(Device, this);
            Frequency = new Freq(Device, this);
            Capacity = new Cap(Device, this);
            Temperature = new Temp(Device, this);
            
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
            public DcVolt(IeeeBase calibrator, CalibrMain abstrCalibr) : base(calibrator, abstrCalibr)
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
            public AcVolt(IeeeBase device, CalibrMain abstrCalibr) : base(device, abstrCalibr)
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
            public DcCurr(IeeeBase calibrator, CalibrMain abstrCalibr) : base(calibrator, abstrCalibr)
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
            public AcCurr(IeeeBase device, CalibrMain abstrCalibr) : base(device, abstrCalibr)
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
                [AccRange("Mode: Amps SI", typeof(MeasPoint<Current, Frequency>))]
                public override RangeStorage<PhysicalRange<Current, Frequency>> Ranges { get; set; }
                

            }

        }

        public class Cap: SimplyPhysicalQuantity<Capacity>
        {
            public Cap(IeeeBase calibrator, CalibrMain abstrCalibr) : base(calibrator, abstrCalibr)
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
            
            public Temp(IeeeBase calibrator, CalibrMain abstrCalibr) : base(calibrator, abstrCalibr)
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

            public void SetTermoCoupleType(FlukeTypeTermocouple flukeTypeTermocouple)
            {
                calibrator.WriteLine("TC_TYPE " + flukeTypeTermocouple.GetStringValue());
                _calibrMain.CheckErrors();
            }

            public ITermocoupleType Temperature { get; }
        }

        public abstract class Resist : SimplyPhysicalQuantity<Resistance>, IResistance
        {
            protected Resist(IeeeBase calibrator, CalibrMain abstrCalibr) : base(calibrator, abstrCalibr)
            {
                
            }

            public ICommand[] CompensationMode { get; set; }
            public void SetCompensation(Compensation compMode)
            {
                var command = CompensationMode.FirstOrDefault(q => (int)q.Value == (int) compMode);
                calibrator.WriteLine(command.StrCommand);
                _calibrMain.CheckErrors();
            }
        }
        public class Resist2W :Resist
        {
           

            public Resist2W(IeeeBase calibrator, CalibrMain abstrCalibr) : base(calibrator, abstrCalibr)
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
             ISourcePhysicalQuantity<TPhysicalQuantity>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        {
            private TPhysicalQuantity _value;
            protected IeeeBase calibrator{get; private set; }
            protected CalibrMain _calibrMain { get; }

            protected SimplyPhysicalQuantity(IeeeBase calibrator, CalibrMain abstrCalibr)
            {
                calibrator = calibrator;
                _calibrMain = abstrCalibr;
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
                calibrator.WriteLine(@"OUT " + ConvetrMeasPointToCommand(value));
                calibrator.WaitingRemoteOperationComplete();
                _calibrMain.CheckErrors();
            }


           
            public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; protected set; }
            public bool IsEnableOutput { get; private set; }
            
        }

        public abstract class ComplexPhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> : 
                                                                                               ISourcePhysicalQuantity<TPhysicalQuantity,
                                                                                                   TPhysicalQuantity2>
            where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
            where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
        {
            /// <summary>
            /// Поиск пробела между значениями двух физических величин.
            /// </summary>
            private static readonly Regex regex = new Regex(@"(?!\w) (?=\d)");
            protected CalibrMain _calibrMain { get; }

            private IeeeBase _device;

            protected ComplexPhysicalQuantity(IeeeBase device, CalibrMain abstrCalibr)
            {
                _device = device;
                _calibrMain = abstrCalibr;
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
                _device.WriteLine(@"OUT " + ConvetrMeasPointToCommand(value));
                _device.WaitingRemoteOperationComplete();
                _calibrMain.CheckErrors();
            }

           

            public IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; protected set; }
        }

      

       public string UserType { get; protected set; }
        public string StringConnection
        {
            get => Device.StringConnection;
            set
            {
                Device.StringConnection = value;
                Initialize();
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }
        public async void Initialize()
        {
            var fileName = @$"{Environment.CurrentDirectory}\acc\{UserType}.acc";
            if(!File.Exists(fileName)) return;
            this.FillRangesDevice(fileName);
            
        }

        public class Freq : ComplexPhysicalQuantity<Frequency, Voltage>
        {
            public Freq(IeeeBase device, CalibrMain abstrCalibr) : base(device, abstrCalibr)
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

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public bool IsEnableOutput { get; private set; }
        public void OutputOff()
        {
            Device.WriteLine(State.Off.GetStringValue());
            Device.WaitingRemoteOperationComplete();
            IsEnableOutput = false;
        }

        public void OutputOn()
        {
            Device.WriteLine(State.On.GetStringValue());
            Device.WaitingRemoteOperationComplete();
            IsEnableOutput = true;
        }

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
}