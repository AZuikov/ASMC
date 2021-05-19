using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;

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
        /// <summary>
        /// Номер или наименование канала частотомера.
        /// </summary>
        public int NameOfChanel { get; }

        /// <summary>
        /// Позволяет узнать сопротивление входа частотомера.
        /// </summary>
        public MeasPoint<Resistance> InputImpedance { get; }
        /// <summary>
        /// Устанавливаем сопротивление входа частотомера 50 Ом.
        /// </summary>
        public void Set50OhmInput();

        /// <summary>
        /// Устанавливаем сопротивление входа частотомера 1 МОм.
        /// </summary>
        public void Set1MOhmInput();

        //на будущее
        //public void Set75OhmInput();
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