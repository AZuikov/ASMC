using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface;

namespace ASMC.Devices.IEEE.Keysight.Generator
{
    public abstract class AbstractSignalGenerator : ISignalStandartParametr<Voltage, Frequency>
    {
        public MeasPoint<Voltage, Frequency> AmplitudeAndFrequency { get; set; }
        public MeasPoint<Voltage> SignalOffset { get; set; }
        public MeasPoint<Time> Delay { get; set; }
        public bool IsPositivePolarity { get; set; }
        public string SignalFormName { get; protected set; }

        public AbstractSignalGenerator()
        {
            Delay = new MeasPoint<Time>(0);
            SignalOffset = new MeasPoint<Voltage>(0);
            IsPositivePolarity = true;
        }
    }

    #region SignalsForm

    public class SineFormSignal : AbstractSignalGenerator
    {
        public SineFormSignal()
        {
            SignalFormName = "sine";
        }
    }

    /// <summary>
    /// Одиночный импульс.
    /// </summary>
    public class ImpulseFormSignal : AbstractSignalGenerator, IImpulseSignal<Voltage, Frequency>
    {
        public ImpulseFormSignal()
        {
            SignalFormName = "pulse";
            RiseEdge = new MeasPoint<Time>(0);
            FallEdge = new MeasPoint<Time>(0);
            Width = new MeasPoint<Time>(50, UnitMultiplier.Nano);
        }

        public MeasPoint<Time> RiseEdge { get; set; }
        public MeasPoint<Time> FallEdge { get; set; }
        public MeasPoint<Time> Width { get; set; }
    }

    /// <summary>
    /// Импульсы с коэффициентом заполнения.
    /// </summary>
    public class SquareFormSignal : AbstractSignalGenerator, ISquareSignal<Voltage, Frequency>
    {
        public MeasPoint<Percent> DutyCicle { get; set; }

        public SquareFormSignal()
        {
            SignalFormName = "square";
            DutyCicle = new MeasPoint<Percent>(50);
        }
    }

    /// <summary>
    /// Пилообразный сигнал.
    /// </summary>
    public class RampFormSignal : AbstractSignalGenerator, IRampSignal<Voltage, Frequency>
    {

        public MeasPoint<Percent> Symmetry { get; set; }

        public RampFormSignal()
        {
            SignalFormName = "ramp";
            Symmetry = new MeasPoint<Percent>(100);
        }
    }

    #endregion SignalsForm
}