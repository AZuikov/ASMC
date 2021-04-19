using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    
    public abstract class AbstractSignalGenerator : OutputSignalGenerator81160A, ISignalStandartSetParametrs<Voltage, Frequency>
    {
        
        
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
        
        protected AbstractSignalGenerator(string chanelNumber, GeneratorOfSignals_81160A generator) : base(chanelNumber,generator)
        {
            Delay = new MeasPoint<Time>(0);
            SignalOffset = new MeasPoint<Voltage>(0);
            IsPositivePolarity = true;
        
        }

        public void Getting()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Setting()
        {
            
            Device.WriteLine($":FUNC{ChanelNumber} {SignalFormName}");
            //одной командой  устанавливает частоту, амплитуду и смещение
            Device.WriteLine($":APPL{ChanelNumber}:{SignalFormName} {Value.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}, "+
                             $"{Value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}, "+
                             $"{SignalOffset.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
           Device.WaitingRemoteOperationComplete(); 
           
        }
        /// <summary>
        /// Значение Амплитуды и частоты.
        /// </summary>
        public MeasPoint<Voltage, Frequency> Value { get; private set; }
        /// <summary>
        /// Установить амплитуду и частоту.
        /// </summary>
        /// <param name="value">Измерительная точка содержащая амплитуду и частоту.</param>
        public void SetValue(MeasPoint<Voltage, Frequency> value)
        {
            Value = value;
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


        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }
    }

    #region SignalsForm

    public class SineFormSignal : AbstractSignalGenerator
    {
        public SineFormSignal(string chanelNumber, GeneratorOfSignals_81160A generator) : base(chanelNumber,generator)
        {
            SignalFormName = "SINusoid";
            
        }

        public void Getting()
        {
            throw new System.NotImplementedException();
        }

       

        public MeasPoint<Voltage, Frequency> Value { get; }
       
        
    }

    /// <summary>
    /// Одиночный импульс.
    /// </summary>
    public class ImpulseFormSignal : AbstractSignalGenerator,  IImpulseSignal<Voltage, Frequency>
    {
        public ImpulseFormSignal(string chanelNumber, GeneratorOfSignals_81160A generator) : base(chanelNumber, generator)
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
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran:unit SEC");
            Device.WriteLine($"{NameOfOutput}:del{SignalFormName}:unit SEC");
            //ставим длительность импульса
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}WIDT {Width.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //фронт импульса
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran {RiseEdge.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            //спад импульса
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran:tra {RiseEdge.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}");
            Device.WaitingRemoteOperationComplete();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
       

        
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Импульсы с коэффициентом заполнения.
    /// </summary>
    public class SquareFormSignal : AbstractSignalGenerator,  ISquareSignal<Voltage, Frequency>
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

        public SquareFormSignal(string chanelNumber, GeneratorOfSignals_81160A generator) : 
            base(chanelNumber, generator)
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
            Device.WriteLine($"func{NameOfOutput}:{SignalFormName}:dcyc {DutyCicle.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',','.')}PCT");
            Device.WaitingRemoteOperationComplete();
        }

        public MeasPoint<Voltage, Frequency> Value { get; }
       

      
        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Пилообразный сигнал.
    /// </summary>
    public class RampFormSignal : AbstractSignalGenerator,  IRampSignal<Voltage, Frequency>
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

        public RampFormSignal(string chanelNumber, GeneratorOfSignals_81160A generator) : base(chanelNumber, generator)
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
           Device.WriteLine($":FUNC{NameOfOutput}:{SignalFormName}:SYMM {Symmetry.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(",",".")}PCT");
           Device.WaitingRemoteOperationComplete();

        }
        
        public string NameOfOutput { get; set; }
    }

    #endregion SignalsForm
}