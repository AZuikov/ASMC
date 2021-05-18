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
        
    }

    /// <summary>
    /// Интерфейс измерительного канал частотомера.
    /// </summary>
    public interface ICounterInput : ICounterInputTypicalParametr
    {

        ICounterInputSlopeSetting SettingSlope { get; set; }
        ICounterStandartMeasureOperation MeasureFunctionStandart { get; set; }
        
    }

    public interface ICOunterInputHighFrequency :   ICounterInput
    {
        //ICounterInputPowerMeasure MeasurePower { get; set; }
    }

    public interface ICounterStandartMeasureOperation: ICounterSingleChanelMeasure
    {

    }
}