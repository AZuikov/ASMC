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
                string answer = Device.QueryLine($"OUTP{ChanelNumber}:POL?");
                return answer.Equals(Polarity.NORM.ToString());
            }
            set
            {
                if (value)
                {
                    Device.WriteLine($"OUTP{ChanelNumber}:POL {Polarity.NORM}");
                }
                else
                {
                    Device.WriteLine($"OUTP{ChanelNumber}:POL {Polarity.INV}");
                }
                Device.WaitingRemoteOperationComplete();
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
            Device.WriteLine($":FUNC{ChanelNumber} {SignalFormName}");
            //одной командой  устанавливает частоту, амплитуду и смещение
            Device.WriteLine($":APPL{ChanelNumber}:{SignalFormName} {AmplitudeAndFrequency.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}, "+
                             $"{AmplitudeAndFrequency.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}, "+
                             $"{SignalOffset.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
           Device.WaitingRemoteOperationComplete(); 
           
        }

        public bool IsEnableOutput { get; protected set; }
        public void OutputOn()
        {
           Device.WriteLine($"OUTP{ChanelNumber} {ChanelStatus.ON}");
           Device.WaitingRemoteOperationComplete();
           //теперь проверим, что выход включился.
           string answer = Device.QueryLine($"OUTP{ChanelNumber}?");
           int resultAnswerNumb = -1;
           if (int.TryParse(answer, out resultAnswerNumb))
           {
               IsEnableOutput = resultAnswerNumb == (int) ChanelStatus.ON;
           }

        }

        public void OutputOff()
        {
            Device.WriteLine($"OUTP{ChanelNumber} {ChanelStatus.OFF}");
            Device.WaitingRemoteOperationComplete();
            //теперь проверим, что выход включился.
            string answer = Device.QueryLine($"OUTP{ChanelNumber}?");
            int resultAnswerNumb = -1;
            if (int.TryParse(answer, out resultAnswerNumb))
            {
                IsEnableOutput = resultAnswerNumb == (int)ChanelStatus.ON;
            }
        }


    }

    #region SignalsForm

    public class SineFormSignal : AbstractSignalGenerator, IOutputSignalGenerator<Voltage, Frequency>
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
    public class ImpulseFormSignal : AbstractSignalGenerator, IOutputSignalGenerator<Voltage, Frequency>, IImpulseSignal<Voltage, Frequency>
    {
        public ImpulseFormSignal(string chanelNumber) : base(chanelNumber)
        {
            SignalFormName = "PULSe";
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
            throw new System.NotImplementedException();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnableOutput { get; }
        public void OutputOn()
        {
            throw new System.NotImplementedException();
        }

        public void OutputOff()
        {
            throw new System.NotImplementedException();
        }

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Импульсы с коэффициентом заполнения.
    /// </summary>
    public class SquareFormSignal : AbstractSignalGenerator, IOutputSignalGenerator<Voltage, Frequency>, ISquareSignal<Voltage, Frequency>
    {
        public MeasPoint<Percent> DutyCicle { get; set; }

        public SquareFormSignal(string chanelNumber) : base(chanelNumber)
        {
            SignalFormName = "SQUare";
            DutyCicle = new MeasPoint<Percent>(50);
        }

        public void Getting()
        {
            throw new System.NotImplementedException();
        }

        public void Setting()
        {
            throw new System.NotImplementedException();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnableOutput { get; }
        public void OutputOn()
        {
            throw new System.NotImplementedException();
        }

        public void OutputOff()
        {
            throw new System.NotImplementedException();
        }

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Пилообразный сигнал.
    /// </summary>
    public class RampFormSignal : AbstractSignalGenerator, IOutputSignalGenerator<Voltage, Frequency>, IRampSignal<Voltage, Frequency>
    {

        public MeasPoint<Percent> Symmetry { get; set; }

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
            throw new System.NotImplementedException();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnableOutput { get; }
        public void OutputOn()
        {
            throw new System.NotImplementedException();
        }

        public void OutputOff()
        {
            throw new System.NotImplementedException();
        }

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
        public string NameOfOutput { get; set; }
    }

    #endregion SignalsForm
}