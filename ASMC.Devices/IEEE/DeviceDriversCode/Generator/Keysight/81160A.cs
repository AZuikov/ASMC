using AP.Utils.Data;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    public class GeneratorOfSignals_81160A : ISignalGenerator<Voltage, Frequency>
    {
        private IeeeBase generator = new IeeeBase();

        private enum Option
        {
            [StringValue("Opt. 001")] Opt001,//81160A-001	1 Channel 330MHz Pulse Function Arbitrary Generator
            [StringValue("Opt. 002")] Opt002,//81160A-002	2 Channel 330MHz Pulse Function Arbitrary Generator
            [StringValue("Opt. PAT_330")] PAT_330,//81160A-330	License for 330 Mbit/s Pattern Generation
            [StringValue("Opt. PAT_660")] PAT_660//81160A-660	License for 660 Mbit/s Pattern Generation
        }

        public List<string> OptionList { get; private set; }

        public GeneratorOfSignals_81160A()
        {
            UserType = "81160A";

        }

        public void SetExternalReferenceClock()
        {
            generator.WriteLine(":ROSC:SOUR EXT");
            generator.WaitingRemoteOperationComplete();
        }

        public void SetInternalReferenceClock()
        {
            generator.WriteLine(":ROSC:SOUR INT");
            generator.WaitingRemoteOperationComplete();
        }

        public string UserType { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public async Task InitializeAsync()
        {
            OUT1 = new GeneratorOutput_81160A(1, generator);
            if (generator.GetOption().Any(q => Equals(q, Option.Opt002.GetStringValue())))
            {
                OUT2 = new GeneratorOutput_81160A(2, generator);

            }

            //для инициализации второго канала нужно проверить, есть ли такая опция.
            //это наверное должно происходить в методе InitializeAsync
        }

        private string stringconnect = "";

        public string StringConnection
        {
            get => stringconnect;
            set
            {
                stringconnect = value;
                InitializeAsync();

            }
        }

        public IOutputSignalGenerator OUT1 { get; set; }
        public IOutputSignalGenerator OUT2 { get; set; }
    }

    /// <summary>
    /// Выход генератора сигналов.
    /// </summary>
    public class GeneratorOutput_81160A : IOutputSignalGenerator
    {
        public GeneratorOutput_81160A(int chanelNumber, IeeeBase generator)
        {
            StringConnection = generator.StringConnection;
            NameOfOutput = chanelNumber.ToString();
            SineSignal = new SineFormSignal(NameOfOutput, generator);
            //ImpulseSignal = new ImpulseFormSignal(ChanelNumber, generator);
            //SquareSignal = new SquareFormSignal(ChanelNumber, generator);
            //RampSignal = new RampFormSignal(ChanelNumber, generator);
        }

        public string NameOfOutput { get; set; }
        public ISineSignal<Voltage, Frequency> SineSignal { get; set; }
        public IImpulseSignal<Voltage, Frequency> ImpulseSignal { get; set; }
        public ISquareSignal<Voltage, Frequency> SquareSignal { get; set; }
        public IRampSignal<Voltage, Frequency> RampSignal { get; set; }
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