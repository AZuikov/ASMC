using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    public interface ISignalStandartSetParametrs<TPhysicalQuantity, TPhysicalQuantity2> : IDeviceSettingsControl, ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        /// <summary>
        /// Смещение сигнала.
        /// </summary>
        public MeasPoint<TPhysicalQuantity> SignalOffset { get; set; }

        /// <summary>
        /// Задержка сигнала (работает не для всех видов сигналов).
        /// </summary>
        public MeasPoint<Time> Delay { get; set; }

        /// <summary>
        /// Позволяет указать способ представления амплитуды.
        /// </summary>
        /// <param name="amplitude"></param>
        /// <param name="amplitudeUnit"></param>
        public MeasureUnitsAmplitude AmplitudeUnitValue { get; set; }

        /// <summary>
        /// Полярность сигнала.
        /// </summary>
        public bool IsPositivePolarity { get; set; }

        public string SignalFormName { get; }
    }

    public interface ISineSignal<TPhysicalQuantity, TPhysicalQuantity2>  : ISignalStandartSetParametrs<TPhysicalQuantity, TPhysicalQuantity2> 
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {

    }

    /// <summary>
    /// Единичный импульс.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    /// <typeparam name="TPhysicalQuantity2"></typeparam>
    public interface IImpulseSignal<TPhysicalQuantity, TPhysicalQuantity2> :
        ISignalStandartSetParametrs<TPhysicalQuantity, TPhysicalQuantity2> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
                                                                       where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        /// <summary>
        /// Длительность фронта.
        /// </summary>
        public MeasPoint<Time> RiseEdge { get; set; }

        /// <summary>
        /// Длительность спада.
        /// </summary>
        public MeasPoint<Time> FallEdge { get; set; }

        /// <summary>
        /// Длительность импульса.
        /// </summary>
        public MeasPoint<Time> Width { get; set; }
    }

    public interface ISquareSignal<TPhysicalQuantity, TPhysicalQuantity2> :
        ISignalStandartSetParametrs<TPhysicalQuantity, TPhysicalQuantity2>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        public MeasPoint<Percent> DutyCicle { get; set; }
    }

    /// <summary>
    /// Пилообразный сигнал.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    /// <typeparam name="TPhysicalQuantity2"></typeparam>
    public interface IRampSignal<TPhysicalQuantity, TPhysicalQuantity2> :
        ISignalStandartSetParametrs<TPhysicalQuantity, TPhysicalQuantity2>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        /// <summary>
        /// Симметричность сигнала, относительно горба.
        /// </summary>
        public MeasPoint<Percent> Symmetry { get; set; }
    }

    /// <summary>
    /// Интерфейс выхода генератора сигналов.
    /// </summary>
    public interface IOutputGenerator: ISourceOutputControl
    {
        public string NameOfOutput { get; set; }

        public IOutputSettingGenerator OutputSetting { get; set; }
        public ISignalStandartSetParametrs<Voltage, Frequency> CurrentSignal { get; }

        public void SetSignal(ISignalStandartSetParametrs<Voltage, Frequency> currentSignal);

        public ISineSignal<Voltage, Frequency> SineSignal { get; set; }
        public IImpulseSignal<Voltage, Frequency> ImpulseSignal { get; set; }
        public ISquareSignal<Voltage, Frequency> SquareSignal { get; set; }
        public IRampSignal<Voltage, Frequency> RampSignal { get; set; }
    }

    

    /// <summary>
    /// Типовые настройки выхода генератора.
    /// </summary>
    public interface IOutputSettingGenerator
    {
        public MeasPoint<Resistance> OutputLoad { get; set; }
    }

    /// <summary>
    /// Интерфейс генератора сигналов.
    /// </summary>
    
    public interface ISignalGenerator: IProtocolStringLine, IReferenceClock
        
    {
        public IOutputGenerator [] OUT { get; }
        
    }
}