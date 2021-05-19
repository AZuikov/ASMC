﻿using AP.Utils.Data;
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
    public interface ICounterInputTypicalParametr: ICounterInputFilter, IDeviceSettingsControl
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

        
        public MeasPoint<Voltage> TriggerLeve { get; set; }

        /// <summary>
        /// Диапазон напряжения триггера.
        /// </summary>
        public PhysicalRange<Voltage> TriggerRange { get; }

        /// <summary>
        /// Значение аттенюатора входа.
        /// </summary>
        public CounterAttenuator Attenuator { get; set; }

        public CounterCoupling Coupling { get; set; }

        //на будущее
        //public void Set75OhmInput();

        /// <summary>
        /// Время измерения канала.
        /// </summary>
        public MeasPoint<Time> MeasureTime { get; set; }
        /// <summary>
        /// Возможный диапазон времени измерения на канале.
        /// </summary>
        public PhysicalRange<Time> MeasureTimeRange { get; }


    }

    

    /// <summary>
    /// Фильтр входа частотомера.
    /// </summary>
    public interface ICounterInputFilter
    {
        public CounterOnOffState CounterOnOffState { get; set; }
    }

    /// <summary>
    /// Интерфейс измерительного канал частотомера.
    /// </summary>
    public interface ICounterInput : ICounterInputTypicalParametr
    {

        ICounterInputSlopeSetting SettingSlope { get; set; }
        /// <summary>
        /// Доступные измерительные функции.
        /// </summary>
        ICounterSingleChanelMeasure Measure { get; set; }

    }

    public interface ICOunterInputHighFrequency :   ICounterInput
    {
        //ICounterInputPowerMeasure MeasurePower { get; set; }
    }

    public interface ICounterStandartMeasureOperation: ICounterSingleChanelMeasure, IDeviceSettingsControl
    {

    }

    #region Enums

    public enum CounterAttenuator
    {
        Att1 = 1,
        Att10 = 10
    }

    public enum CounterCoupling
    {
        AC,
        DC
    }

    /// <summary>
    /// Статусы работы фильтра.
    /// </summary>
    public enum CounterOnOffState
    {
        ON,
        OFF
    }

    public enum InputSlope
    {
        POS,
        NEG
    }
    #endregion
}