using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using System;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    public class OutputSignalGenerator81160A : IOutputSignalGenerator<Voltage, Frequency>
    {
        /// <summary>
        /// Доступные для воспроизведения формы сигналов.
        /// </summary>
        public ISignalStandartParametr<Voltage,Frequency>[] AvailableWaveForms { get; protected set; }
        private IeeeBase device;
        public OutputSignalGenerator81160A(IeeeBase inDevice, string name)
        {
            device = inDevice;
            NameOfOutput = name;
            AvailableWaveForms = new ISignalStandartParametr<Voltage, Frequency>[]
            {
                new ImpulseFormSignal(), 
                new RampFormSignal(), 
                new SquareFormSignal(), 
                new SineFormSignal(),
            };

           
        }

        public void Getting()
        {
            /*нужно считать стандартные для сигнала параметры*/
            throw new NotImplementedException();
        }

        public void Setting()
        {
            throw new NotImplementedException();
        }
        

        public MeasPoint<Voltage, Frequency> Value { get; }

        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new NotImplementedException();
        }

        public bool IsEnableOutput { get; protected set; }

        public void OutputOn()
        {
            IsEnableOutput = true;
            throw new NotImplementedException();
        }

        public void OutputOff()
        {
            IsEnableOutput = false;
            throw new NotImplementedException();
        }

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public ISignalStandartParametr<Voltage, Frequency> ActiveSignalForm { get; set; }
        public string NameOfOutput { get; set; }
    }

    public class SignalGenerator81160A : ISignalGenerator<Voltage, Frequency>
    {
        protected readonly IeeeBase _device;

        public SignalGenerator81160A()
        {
            _device = new IeeeBase();
            outputs = new[]
            {
                new OutputSignalGenerator81160A(_device, "1"),
                new OutputSignalGenerator81160A(_device, "2")
            };
        }

        public IOutputSignalGenerator<Voltage, Frequency>[] outputs { get; protected set; }
    }

   
}