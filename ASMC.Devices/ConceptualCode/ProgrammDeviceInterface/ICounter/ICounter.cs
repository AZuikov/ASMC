using ASMC.Data.Model;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс типового частотомера.
    /// </summary>
    public interface ICounter : IReferenceClock, IProtocolStringLine
    {
        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC_HighFrequency { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
    }

    /// <summary>
    /// Базовые параметры любого канала частотомера.
    /// </summary>
    public interface ICounterInputTypicalParametr
    {
        public int NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; set; }
    }

    /// <summary>
    /// Интерфейс измерительного канал частотомера.
    /// </summary>
    public interface ICounterInput : ICounterInputTypicalParametr, ICounterSingleChanelMeasureAorB,
                                     ICounterSingleChanelMeasureABC
    {
    }

    public interface ICOunterInputHighFrequency : ICounterInput, ICounterInputPowerMeasure
    {
    }
}