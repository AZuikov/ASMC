using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    
    public  class OutputSignalGenerator81160A 
    {
        public IeeeBase Device { get; }

        
        public string ChanelNumber { get; }
        public OutputSignalGenerator81160A(string chanelNumber)
        {
            Device = new IeeeBase();
            ChanelNumber = chanelNumber;
            
        }
    }

    public class GeneratorOfSignals_81160A : ISignalGenerator<Voltage, Frequency>
    {
        
        public OutputSignalGenerator81160A OUT1 {get; }
        public OutputSignalGenerator81160A OUT2 {get; }

        public GeneratorOfSignals_81160A()
        {
            UserType = "81160A";
            OUT1 = new OutputSignalGenerator81160A("1");
            OUT2 = new OutputSignalGenerator81160A("2");
            

        }


        public void SetExternalReferenceClock()
        {
            
            OUT1.Device.WriteLine(":ROSC:SOUR EXT");
            OUT1.Device.WaitingRemoteOperationComplete();
        }

        public void SetInternalReferenceClock()
        {
            OUT1.Device.WriteLine(":ROSC:SOUR INT");
            OUT1.Device.WaitingRemoteOperationComplete();
        }

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

        private string stringConnection;

        public string StringConnection
        {
            get => stringConnection;
            set
            {
                stringConnection = value;
                OUT1.Device.StringConnection = stringConnection;
                OUT2.Device.StringConnection = stringConnection;
            }
        }
    }

   
}