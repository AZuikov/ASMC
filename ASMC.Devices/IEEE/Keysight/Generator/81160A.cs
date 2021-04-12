using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using System;
using System.Runtime.CompilerServices;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    
    public  class OutputSignalGenerator81160A 
    {
        protected GeneratorOfSignals_81160A Generator { get; }
        public string ChanelNumber { get; }
        public OutputSignalGenerator81160A(string chanelNumber, GeneratorOfSignals_81160A device)
        {
            ChanelNumber = chanelNumber;
            Generator = device;
        }
    }

    public class GeneratorOfSignals_81160A : ISignalGenerator<Voltage, Frequency>
    {
        public IeeeBase Device { get; }
        public OutputSignalGenerator81160A OUT1 {get; }
        public OutputSignalGenerator81160A OUT2 {get; }

        public GeneratorOfSignals_81160A()
        {
            Device = new IeeeBase();
            OUT1 = new OutputSignalGenerator81160A("",this);
            OUT2 = new OutputSignalGenerator81160A("",this);
            
        }


        public void SetExternalReferenceClock()
        {
            Device.WriteLine(":ROSC:SOUR EXT");
            Device.WaitingRemoteOperationComplete();
        }

        public void SetInternalReferenceClock()
        {
            Device.WriteLine(":ROSC:SOUR INT");
            Device.WaitingRemoteOperationComplete();
        }
    }

   
}