using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AP.Utils.Data;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    
    public  class OutputSignalGenerator81160A 
    {
        public IeeeBase Device { get; }
        public ISignalStandartParametr<Voltage, Frequency> SineSignal { get;  }
        public IImpulseSignal<Voltage,Frequency>ImpulseSignal { get; }
        public ISquareSignal<Voltage,Frequency> SquareSignal { get; }
        public IRampSignal<Voltage,Frequency> RampSignal { get; }

        public string ChanelNumber { get; }
        public OutputSignalGenerator81160A(string chanelNumber)
        {
            Device = new IeeeBase();
            ChanelNumber = chanelNumber;
            SineSignal = new SineFormSignal(ChanelNumber);
            ImpulseSignal = new ImpulseFormSignal(ChanelNumber);
            SquareSignal = new SquareFormSignal(ChanelNumber);
            RampSignal = new RampFormSignal(ChanelNumber);
            
        }
    }

    public class GeneratorOfSignals_81160A : ISignalGenerator<Voltage, Frequency>
    {
        enum Option
        {
            [StringValue("Opt. 001")]Opt001,//81160A-001	1 Channel 330MHz Pulse Function Arbitrary Generator	 
            [StringValue("Opt. 002")]Opt002,//81160A-002	2 Channel 330MHz Pulse Function Arbitrary Generator	 
            [StringValue("Opt. PAT_330")]PAT_330,//81160A-330	License for 330 Mbit/s Pattern Generation
            [StringValue("Opt. PAT_660")] PAT_660//81160A-660	License for 660 Mbit/s Pattern Generation
        }

        public OutputSignalGenerator81160A OUT1 {get; }
        public OutputSignalGenerator81160A OUT2 {get; private set; }

        public List<string>OptionList { get; private set; }

        public GeneratorOfSignals_81160A()
        {
            UserType = "81160A";
            OUT1 = new OutputSignalGenerator81160A("1");
            //для инициализации второго канала нужно проверить, есть ли такая опция.
            //это происходит в методе InitializeAsync 


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
            //получим список опций
            OptionList = OUT1.Device.GetOption();
            if (OptionList.Contains(Option.Opt002.GetStringValue()))
            {
                OUT2 = new OutputSignalGenerator81160A("2");
            }

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