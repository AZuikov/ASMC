using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using System;
using System.Runtime.CompilerServices;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    public  class OutputSignalGenerator81160A :GeneratorOfSignals_81160A
    {
        public string ChanelNumber { get; }
        public OutputSignalGenerator81160A(string chanelNumber)
        {
            ChanelNumber = chanelNumber;
        }
    }

    public class GeneratorOfSignals_81160A : ISignalGenerator<Voltage, Frequency>
    {
        public IeeeBase Device { get; }

        public GeneratorOfSignals_81160A()
        {
            Device = new IeeeBase();
            
        }

        
    }

   
}