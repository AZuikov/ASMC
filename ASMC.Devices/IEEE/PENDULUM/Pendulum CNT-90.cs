using System;
using System.Threading.Tasks;
using AP.Utils.Data;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.PENDULUM
{
    /// <summary>
    /// Класс частотомера.
    /// </summary>
    public class Pendulum_CNT_90 : ICounter
    {
        public Pendulum_CNT_90()
        {
            UserType = "CNT-90";
            //todo нужно как то проверять наличие опций и создавать нужную конфигурацию
        }

        public void SetExternalReferenceClock()
        {
            //:ROSCillator:SOURce EXT
            throw new NotImplementedException();
        }

        public void SetInternalReferenceClock()
        {
            //:ROSCillator:SOURce INT
            throw new NotImplementedException();
        }

        public string UserType { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
        }

        public string StringConnection { get; set; }

        private enum InstallTimebaseOption
        {
            [StringValue("Standard")] Standard,
            [StringValue("Option 19")] Option19,
            [StringValue("Option 30")] Option30,
            [StringValue("Option 40")] Option40,
            [StringValue("Rubidium")] Rubidium
        }

        private enum InstallPrescalerOption
        {
            [StringValue("0")] NullOption,
            [StringValue("Option 10")] Option10,
            [StringValue("Option 13")] Option13,
            [StringValue("Option 14")] Option14,
            [StringValue("Option 14B")] Option14B
        }

        private enum InstallMicrowaveConverter
        {
            [StringValue("27GHz")] Microwave27GHz,
            [StringValue("40GHz")] Microwave40GHz,
            [StringValue("46GHz")] Microwave46GHz,
            [StringValue("60GHz")] Microwave60GHz
        }
    }

    public abstract class CounterInput : ITypicalCounterInput<Frequency>
    {
        #region Enums

        protected enum InputCouple
        { AC, DC }

        enum InputImpedance
        {
            [DoubleValue(50)] IMP50Ohm,
            [DoubleValue(1e6)] IMPMegaOhm
        }

        protected enum InputAtt
        {
            ATT1,
            ATT10
        }

        protected enum InputSlope
        {
            POS,
            NEG
        }

        protected enum Status
        {ON, OFF}

        #endregion

        #region Property



        public IeeeBase Device { get; }

        #endregion

        protected CounterInput(string chanelName)
        {
            Device = new IeeeBase();
            NameOfChanel = chanelName;
        }

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }

        public MeasPoint<Frequency> GetValue()
        {
            throw new NotImplementedException();
        }

        public MeasPoint<Frequency> Value { get; }
        public IRangePhysicalQuantity<Frequency> RangeStorage { get; }
        public string NameOfChanel { get; }
        public string UserType { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public string StringConnection { get; set; }
    }
}