using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Интерфейс типового частотомера.
    /// </summary>
    public interface ICounter : IReferenceClock, IProtocolStringLine
    {
        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICOunterInputHighFrequency InputC { get; set; }
        public ICounterInputDualChanelMeasure DualChanelMeasure { get; set; }
    }

    /// <summary>
    /// Бозовые парамертры любого канала частотомера.
    /// </summary>
    public interface ICounterInputTypicalParametr : IProtocolStringLine
    {
        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; set; }

    }

    /// <summary>
    /// Интефейс измерительного канал частотомера.
    /// </summary>
    public interface ICounterInput : ICounterInputTypicalParametr, ICounterSingleChanelMeasureAorB,
                                     ICounterSingleChanelMeasureABC
    {

    }

    public interface ICOunterInputHighFrequency : ICounterInput, ICounterInputPowerMeasure
    {

    }

    
                                     

   
}