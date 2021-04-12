using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    
    public abstract class AbstractSignalGenerator : OutputSignalGenerator81160A, ISignalStandartParametr<Voltage, Frequency>
    {
        public MeasPoint<Voltage, Frequency> AmplitudeAndFrequency { get; set; }
        
        public MeasPoint<Voltage> SignalOffset { get; set; }
        public MeasPoint<Time> Delay { get; set; }

        public bool IsPositivePolarity
        {
            get
            {
                string answer = Generator.Device.QueryLine($"OUTP{ChanelNumber}:POL?");
                return answer.Equals(Polarity.NORM.ToString());
            }
            set
            {
                if (value)
                {
                    Generator.Device.WriteLine($"OUTP{ChanelNumber}:POL {Polarity.NORM}");
                }
                else
                {
                    Generator.Device.WriteLine($"OUTP{ChanelNumber}:POL {Polarity.INV}");
                }
                Generator.Device.WaitingRemoteOperationComplete();
            }
        }

        public string SignalFormName { get; protected set; }

        /// <summary>
        /// статусы выхода генератора.
        /// </summary>
        enum ChanelStatus
        {
            OFF=0,
            ON=1
        }

        /// <summary>
        /// Полярность сигнала.
        /// </summary>
        enum Polarity
        {
            NORM,
            INV
        }
        
        protected AbstractSignalGenerator(string chanelNumber) : base(chanelNumber)
        {
            Delay = new MeasPoint<Time>(0);
            SignalOffset = new MeasPoint<Voltage>(0);
            IsPositivePolarity = true;
        
        }

        public virtual void Setting()
        {
            Generator.Device.WriteLine($":FUNC{ChanelNumber} {SignalFormName}");
            //одной командой  устанавливает частоту, амплитуду и смещение
            Generator.Device.WriteLine($":APPL{ChanelNumber}:{SignalFormName} {AmplitudeAndFrequency.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}, "+
                             $"{AmplitudeAndFrequency.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}, "+
                             $"{SignalOffset.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
           Generator.Device.WaitingRemoteOperationComplete(); 
           
        }

        public bool IsEnableOutput { get; protected set; }
        public void OutputOn()
        {
           Generator.Device.WriteLine($"OUTP{ChanelNumber} {ChanelStatus.ON}");
           Generator.Device.WaitingRemoteOperationComplete();
           //теперь проверим, что выход включился.
           string answer = Generator.Device.QueryLine($"OUTP{ChanelNumber}?");
           int resultAnswerNumb = -1;
           if (int.TryParse(answer, out resultAnswerNumb))
           {
               IsEnableOutput = resultAnswerNumb == (int) ChanelStatus.ON;
           }

        }

        public void OutputOff()
        {
            Generator.Device.WriteLine($"OUTP{ChanelNumber} {ChanelStatus.OFF}");
            Generator.Device.WaitingRemoteOperationComplete();
            //теперь проверим, что выход включился.
            string answer = Generator.Device.QueryLine($"OUTP{ChanelNumber}?");
            int resultAnswerNumb = -1;
            if (int.TryParse(answer, out resultAnswerNumb))
            {
                IsEnableOutput = resultAnswerNumb == (int)ChanelStatus.ON;
            }
        }


    }

    #region SignalsForm

    public class SineFormSignal : AbstractSignalGenerator, IOutputSignalGenerator
    {
        public SineFormSignal(string chanelNumber) : base(chanelNumber)
        {
            SignalFormName = "SINusoid";
            
        }

        public void Getting()
        {
            throw new System.NotImplementedException();
        }

       

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

      

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Одиночный импульс.
    /// </summary>
    public class ImpulseFormSignal : AbstractSignalGenerator, IOutputSignalGenerator, IImpulseSignal<Voltage, Frequency>
    {
        public ImpulseFormSignal(string chanelNumber) : base(chanelNumber)
        {
            SignalFormName = "PULS";
            RiseEdge = new MeasPoint<Time>(0);
            FallEdge = new MeasPoint<Time>(0);
            Width = new MeasPoint<Time>(50, UnitMultiplier.Nano);
        }

        public MeasPoint<Time> RiseEdge { get; set; }
        public MeasPoint<Time> FallEdge { get; set; }
        public MeasPoint<Time> Width { get; set; }
        public void Getting()
        {
            throw new System.NotImplementedException();
        }

        public void Setting()
        {
            base.Setting();
            //ставим единицы измерения фронтов в секундах
            Generator.Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran:unit SEC");
            Generator.Device.WriteLine($"{NameOfOutput}:del{SignalFormName}:unit SEC");
            //ставим длительность импульса
            Generator.Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}WIDT {Width.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //фронт импульса
            Generator.Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran {RiseEdge.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            //спад импульса
            Generator.Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran:tra {RiseEdge.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            Generator.Device.WaitingRemoteOperationComplete();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnableOutput { get; }
       

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Импульсы с коэффициентом заполнения.
    /// </summary>
    public class SquareFormSignal : AbstractSignalGenerator, IOutputSignalGenerator, ISquareSignal<Voltage, Frequency>
    {
        private MeasPoint<Percent> dutyCilcle;
        public MeasPoint<Percent> DutyCicle
        {
            get => dutyCilcle;
            set
            {
                if (value.MainPhysicalQuantity.Value < 0)
                {
                    dutyCilcle = new MeasPoint<Percent>(0);
                }
                else if (value.MainPhysicalQuantity.Value > 100)
                    dutyCilcle = new MeasPoint<Percent>(100);
                else
                {
                    dutyCilcle = value;
                }
            }
        }

        public SquareFormSignal(string chanelNumber) : base(chanelNumber)
        {
            SignalFormName = "SQU";
            DutyCicle = new MeasPoint<Percent>(50);
        }

        public void Getting()
        {
            throw new System.NotImplementedException();
        }

        public void Setting()
        {
            base.Setting();
            Generator.Device.WriteLine($"func{NameOfOutput}:{SignalFormName}:dcyc {DutyCicle.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}PCT");
            Generator.Device.WaitingRemoteOperationComplete();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnableOutput { get; }
       
        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Пилообразный сигнал.
    /// </summary>
    public class RampFormSignal : AbstractSignalGenerator, IOutputSignalGenerator, IRampSignal<Voltage, Frequency>
    {
        /// <summary>
        /// Процент симметричности сигнала.
        /// </summary>
        private MeasPoint<Percent> symmetry;
        /// <summary>
        /// Процент симметричности сигнала.
        /// </summary>
        public MeasPoint<Percent> Symmetry
        {
            get => symmetry;
            set
            {
                if (value.MainPhysicalQuantity.Value < 0)
                {
                    symmetry = new MeasPoint<Percent>(0);
                }
                else if (value.MainPhysicalQuantity.Value>100)
                    symmetry=new MeasPoint<Percent>(100);
                else
                {
                    symmetry = value;
                }
            }
        }

        public RampFormSignal(string chanelNumber) : base(chanelNumber)
        {
            SignalFormName = "RAMP";
            Symmetry = new MeasPoint<Percent>(100);
        }

        public void Getting()
        {
            
            throw new System.NotImplementedException();
        }

        public void Setting()
        {
           base.Setting();
           Generator.Device.WriteLine($":FUNC{NameOfOutput}:{SignalFormName}:SYMM {Symmetry.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(",",".")}PCT");
           Generator.Device.WaitingRemoteOperationComplete();

        }

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnableOutput { get; }
       

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    #endregion SignalsForm
}