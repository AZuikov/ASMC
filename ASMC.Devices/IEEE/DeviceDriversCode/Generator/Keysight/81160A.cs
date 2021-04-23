using AP.Utils.Data;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    /// <summary>
    /// Выход генератора сигналов.
    /// </summary>
    public class GeneratorOutput_81160A : IOutputSignalGenerator
    {
        public GeneratorOutput_81160A(string chanelNumber, GeneratorOfSignals_81160A generator)
        {
            StringConnection = generator.StringConnection;
            NameOfOutput = chanelNumber;
            SineSignal = new SineFormSignal(NameOfOutput, this);
            //ImpulseSignal = new ImpulseFormSignal(ChanelNumber, generator);
            //SquareSignal = new SquareFormSignal(ChanelNumber, generator);
            //RampSignal = new RampFormSignal(ChanelNumber, generator);
        }

        public string NameOfOutput { get; set; }
        public ISignalStandartSetParametrs<Voltage, Frequency> SineSignal { get; set; }
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

    public class GeneratorOfSignals_81160A : ISignalGenerator<Voltage, Frequency>
    {
        /// этот объект нужен только для установки параметров внешнего/внутреннего источника частоты (опорного стандарта).
        private IeeeBase deviceForReferenceClock = new IeeeBase();

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
            OUT1 = new GeneratorOutput_81160A("1", this);
            //для инициализации второго канала нужно проверить, есть ли такая опция.
            //это наверное должно происходить в методе InitializeAsync
        }

        public void SetExternalReferenceClock()
        {
            deviceForReferenceClock.WriteLine(":ROSC:SOUR EXT");
            deviceForReferenceClock.WaitingRemoteOperationComplete();
        }

        public void SetInternalReferenceClock()
        {
            deviceForReferenceClock.WriteLine(":ROSC:SOUR INT");
            deviceForReferenceClock.WaitingRemoteOperationComplete();
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

        private string stringconnect = "";

        public string StringConnection
        {
            get => stringconnect;
            set
            {
                stringconnect = value;
                OUT1.StringConnection = stringconnect;
                OUT2.StringConnection = stringconnect;
            }
        }

        public IOutputSignalGenerator OUT1 { get; set; }
        public IOutputSignalGenerator OUT2 { get; set; }
    }
}