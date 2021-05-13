﻿using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;
using ASMC.Devices.Interface.SourceAndMeter;
using System;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    /// <summary>
    /// Типовой сигнал генератора, с минимальным набором характеристик.
    /// </summary>
    public abstract class AbstractSignalForm :ISignalStandartSetParametrs<Voltage, Frequency>
    {
        protected IeeeBase Device;
        private string ChanelNumber = "";
        protected AbstractSignalForm(string chanelNumber, IeeeBase output)

        {
            ChanelNumber = chanelNumber;
            Device = output;
            Delay = new MeasPoint<Time>(0);
            SignalOffset = new MeasPoint<Voltage>(0);
            IsPositivePolarity = true;
        }

        public MeasPoint<Voltage> SignalOffset { get; set; }
        public MeasPoint<Time> Delay { get; set; }

        public bool IsPositivePolarity
        {
            get
            {
                var answer = Device.QueryLine($"OUTP{ChanelNumber}:POL?");
                return answer.Equals(Polarity.NORM.ToString());
            }
            set
            {
                if (value)
                    Device.WriteLine($"OUTP{ChanelNumber}:POL {Polarity.NORM}");
                else
                    Device.WriteLine($"OUTP{ChanelNumber}:POL {Polarity.INV}");
                Device.WaitingRemoteOperationComplete();
            }
        }

        public string SignalFormName { get; protected set; }

        public void Getting()
        {
            throw new NotImplementedException();
        }

        public virtual void Setting()
        {
            Device.WriteLine($":FUNC{ChanelNumber} {SignalFormName}");
            //одной командой  устанавливает частоту, амплитуду и смещение
            Device.WriteLine($":APPL{ChanelNumber}:{SignalFormName} {Value.AdditionalPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}, " +
                             $"{Value.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}, " +
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
        /// <param name = "value">Измерительная точка содержащая амплитуду и частоту.</param>
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
            var answer = Device.QueryLine($"OUTP{ChanelNumber}?");
            var resultAnswerNumb = -1;
            if (int.TryParse(answer, out resultAnswerNumb)) IsEnableOutput = resultAnswerNumb == (int)ChanelStatus.ON;
        }

        public void OutputOff()
        {
            Device.WriteLine($"OUTP{ChanelNumber} {ChanelStatus.OFF}");
            Device.WaitingRemoteOperationComplete();
            //теперь проверим, что выход включился.
            var answer = Device.QueryLine($"OUTP{ChanelNumber}?");
            var resultAnswerNumb = -1;
            if (int.TryParse(answer, out resultAnswerNumb)) IsEnableOutput = resultAnswerNumb == (int)ChanelStatus.ON;
        }

        public IRangePhysicalQuantity<Voltage, Frequency> RangeStorage { get; }

        /// <summary>
        /// статусы выхода генератора.
        /// </summary>
        private enum ChanelStatus
        {
            OFF = 0,
            ON = 1
        }

        /// <summary>
        /// Полярность сигнала.
        /// </summary>
        private enum Polarity
        {
            NORM,
            INV
        }

        public string NameOfOutput { get; protected set; }
    }

    #region SignalsForm

    public class SineFormSignal : AbstractSignalForm, ISineSignal<Voltage, Frequency>
    {
        #region Property

        public new MeasPoint<Voltage, Frequency> Value { get; }

        #endregion Property

        public SineFormSignal(string chanelNumber, IeeeBase output) : base(chanelNumber, output)
        {
            SignalFormName = "SINusoid";
        }

        #region Methods

        public new void Getting()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }

    /// <summary>
    /// Одиночный импульс.
    /// </summary>
    public class ImpulseFormSignal : AbstractSignalForm, IImpulseSignal<Voltage, Frequency>
    {
        public ImpulseFormSignal(string chanelNumber, IeeeBase output) :
            base(chanelNumber, output)
        {
            SignalFormName = "PULS";
            RiseEdge = new MeasPoint<Time>(0);
            FallEdge = new MeasPoint<Time>(0);
            Width = new MeasPoint<Time>(50, UnitMultiplier.Nano);
        }

        public MeasPoint<Time> RiseEdge { get; set; }
        public MeasPoint<Time> FallEdge { get; set; }
        public MeasPoint<Time> Width { get; set; }

        public new void Getting()
        {
            throw new NotImplementedException();
        }

        public new void Setting()
        {
            base.Setting();
            
            //ставим единицы измерения фронтов в секундах
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran:unit SEC");
            Device.WriteLine($"{NameOfOutput}:del{SignalFormName}:unit SEC");
            //ставим длительность импульса
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}WIDT {Width.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //фронт импульса
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran {RiseEdge.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            //спад импульса
            Device.WriteLine($"FUNC{NameOfOutput}:{SignalFormName}:tran:tra {RiseEdge.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}");
            Device.WaitingRemoteOperationComplete();
        }

        public new MeasPoint<Voltage, Frequency> Value { get; }

        public string NameOfOutput { get; set; }
    }

    /// <summary>
    /// Импульсы с коэффициентом заполнения.
    /// </summary>
    public class SquareFormSignal : AbstractSignalForm, ISquareSignal<Voltage, Frequency>
    {
        #region Fields

        private MeasPoint<Percent> dutyCilcle;

        #endregion Fields

        #region Property

        public string NameOfOutput { get; set; }

        #endregion Property

        public SquareFormSignal(string chanelNumber, IeeeBase output) :
            base(chanelNumber, output)
        {
            SignalFormName = "SQU";
            DutyCicle = new MeasPoint<Percent>(50);
        }

        public MeasPoint<Percent> DutyCicle
        {
            get => dutyCilcle;
            set
            {
                if (value.MainPhysicalQuantity.Value < 0)
                    dutyCilcle = new MeasPoint<Percent>(0);
                else if (value.MainPhysicalQuantity.Value > 100)
                    dutyCilcle = new MeasPoint<Percent>(100);
                else
                    dutyCilcle = value;
            }
        }

        public new void Getting()
        {
            throw new NotImplementedException();
        }

        public new void Setting()
        {
            base.Setting();
            Device.WriteLine($"func{NameOfOutput}:{SignalFormName}:dcyc {DutyCicle.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(',', '.')}PCT");
            Device.WaitingRemoteOperationComplete();
        }

        public new MeasPoint<Voltage, Frequency> Value { get; }
    }

    /// <summary>
    /// Пилообразный сигнал.
    /// </summary>
    public class RampFormSignal : AbstractSignalForm, IRampSignal<Voltage, Frequency>
    {
        #region Fields

        /// <summary>
        /// Процент симметричности сигнала.
        /// </summary>
        private MeasPoint<Percent> symmetry;

        #endregion Fields

        #region Property

        public string NameOfOutput { get; set; }

        #endregion Property

        public RampFormSignal(string chanelNumber, IeeeBase output) : base(chanelNumber, output)
        {
            SignalFormName = "RAMP";
            Symmetry = new MeasPoint<Percent>(100);
        }

        /// <summary>
        /// Процент симметричности сигнала.
        /// </summary>
        public MeasPoint<Percent> Symmetry
        {
            get => symmetry;
            set
            {
                if (value.MainPhysicalQuantity.Value < 0)
                    symmetry = new MeasPoint<Percent>(0);
                else if (value.MainPhysicalQuantity.Value > 100)
                    symmetry = new MeasPoint<Percent>(100);
                else
                    symmetry = value;
            }
        }

        public new void Getting()
        {
            throw new NotImplementedException();
        }

        public new void Setting()
        {
            base.Setting();
            Device.WriteLine($":FUNC{NameOfOutput}:{SignalFormName}:SYMM {Symmetry.MainPhysicalQuantity.GetNoramalizeValueToSi().ToString().Replace(",", ".")}PCT");
            Device.WaitingRemoteOperationComplete();
        }
    }

    #endregion SignalsForm
}