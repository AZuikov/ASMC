using AP.Utils.Data;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASMC.Data.Model;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    public class GeneratorOfSignals_81160A : ISignalGenerator
    {
        private IeeeBase deviceIeeeBase = new IeeeBase();

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
            deviceIeeeBase.WriteLine(":ROSC:SOUR EXT");
            deviceIeeeBase.WaitingRemoteOperationComplete();
        }

        public void SetInternalReferenceClock()
        {
            deviceIeeeBase.WriteLine(":ROSC:SOUR INT");
            deviceIeeeBase.WaitingRemoteOperationComplete();
        }

        public string UserType { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsTestConnect { get; }

        public  void Initialize()
        {
            var outs = new List<IOutputGenerator>();
            outs.Add(new GeneratorOutput_81160A(1, deviceIeeeBase));
            //для инициализации второго канала нужно проверить, есть ли такая опция.
            if (deviceIeeeBase.GetOption().Any(q => Equals(q, Option.Opt002.GetStringValue())))
            {
                outs.Add(new GeneratorOutput_81160A(2, deviceIeeeBase));

            }

            OUT = outs.ToArray();
        }

       

        public string StringConnection
        {
            get => deviceIeeeBase.StringConnection;
            set
            {
                deviceIeeeBase.StringConnection = value;
                Initialize();
            }
        }

        
        
        public IOutputGenerator[] OUT { get; protected set; }
    }

    /// <summary>
    /// Выход генератора сигналов.
    /// </summary>
    public class GeneratorOutput_81160A : IOutputGenerator
    {
        protected IeeeBase deviceGeneratorOutput { get; }
        public GeneratorOutput_81160A(int chanelNumber, IeeeBase deviceIeeeBase)
        {
           NameOfOutput = chanelNumber.ToString();
           deviceGeneratorOutput = deviceIeeeBase;
            OutputSetting = new OutputSetting();
            OutputSetting.OutputImpedance = new MeasPoint<Resistance>(50);
            OutputSetting.OutputLoad = new MeasPoint<Resistance>(50);
            
            SineSignal = new SineFormSignal(NameOfOutput, deviceIeeeBase);
            //ImpulseSignal = new ImpulseFormSignal(ChanelNumber, generator);
            //SquareSignal = new SquareFormSignal(ChanelNumber, generator);
            //RampSignal = new RampFormSignal(ChanelNumber, generator);
        }

        public string NameOfOutput { get; set; }
        public IOutputSettingGenerator OutputSetting { get; set; }
        public ISignalStandartSetParametrs<Voltage, Frequency> CurrentSignal { get; protected set; }
        public void SetSignal(ISignalStandartSetParametrs<Voltage, Frequency> currentSignal)
        {
            CurrentSignal = currentSignal;
        }

        public ISineSignal<Voltage, Frequency> SineSignal { get; set; }
        public IImpulseSignal<Voltage, Frequency> ImpulseSignal { get; set; }
        public ISquareSignal<Voltage, Frequency> SquareSignal { get; set; }
        public IRampSignal<Voltage, Frequency> RampSignal { get; set; }
        public string UserType { get; }
       

       public void Getting()
        {
            CurrentSignal?.Getting();
        }

        public void Setting()
        {
           CurrentSignal?.Setting();
        }

        public bool IsEnableOutput { get; }
        public void OutputOn()
        {
            deviceGeneratorOutput.WriteLine($":OUTput{NameOfOutput} ON");
        }

        public void OutputOff()
        {
            deviceGeneratorOutput.WriteLine($":OUTput{NameOfOutput} OFF");
        }
    }

    public class OutputSetting : IOutputSettingGenerator
    {
        public MeasPoint<Resistance> OutputImpedance { get; set; }
        public MeasPoint<Resistance> OutputLoad { get; set; }
    }



}