using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    public interface ICounter: IReferenceClock, IProtocolStringLine
    {
        public ICounterInput InputA { get; set; }
        public ICounterInput InputB { get; set; }
        public ICounterInput InputE { get; set; }
    }

    public interface ICounterInput:IProtocolStringLine
    {
        public string NameOfChanel { get; }
        public ITypicalCounterInputSettings InputSetting { get; set; }
        /// <summary>
        /// Измерение частоты.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }
        /// <summary>
        /// Измерение несущей частоты пакета.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> MeasFrequencyBURSt { get; set; }
        /// <summary>
        /// Количество циклов в пакете.
        /// </summary>
        IMeterPhysicalQuantity<NoUnits> MeasNumberOfCyclesInBurst { get; set; }
        /// <summary>
        /// Измерение мощности сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Power> MeasPowerAC_Signal { get; set; }
        /// <summary>
        /// Частота следования импульсов в пакете.
        /// </summary>
        IMeterPhysicalQuantity<Percent> MeasPulseRepetitionFrequencyBurstSignal { get; set; }
        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        IMeterPhysicalQuantity<Percent> MeasPositiveDutyFactor { get; set; }
        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        IMeterPhysicalQuantity<Percent> MeasNegativeDutyFactor { get; set; }
        /// <summary>
        /// Отношение частот каналов.
        /// </summary>
        IMeterPhysicalQuantity<NoUnits> MeasFrequencyRatio { get; set; }
        /// <summary>
        /// Максимальное значение переменного сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Voltage> MeasMaximum { get; set; }
        /// <summary>
        /// Минимальное значение переменного сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Voltage> MeasMinimum { get; set; }
        /// <summary>
        /// Измерение от пика до пика.
        /// </summary>
        IMeterPhysicalQuantity<Voltage> MeasPeakToPeak { get; set; }
        IMeterPhysicalQuantity<Voltage> MeasRatio { get; set; }
        IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }
        IMeterPhysicalQuantity<Time> MeasPeriodAver { get; set; }
        IMeterPhysicalQuantity<Time> MeasTimeInterval { get; set; }
        IMeterPhysicalQuantity<Time> MeasPositivePulseWidth { get; set; }
        IMeterPhysicalQuantity<Time> MeasNegativePulseWidth { get; set; }
        IMeterPhysicalQuantity<Degreas> MeasPhase { get; set; }
        

    }

    /// <summary>
    /// Интерфейс выбора условий запуска измерений частотомера (по признакас формы сигнала).
    /// </summary>
    public interface ICounterInputSlopeSetting
    {
        /// <summary>
        /// Запуск по фронту.
        /// </summary>
        public void SetInputSlopePositive();
        /// <summary>
        /// Запуск по спаду.
        /// </summary>
        public void SetInputSlopeNegative();

        public string GetSlope();
    }

    /// <summary>
    /// Интрефейс содержащий типовые настройки входа частотомера.
    /// </summary>
    public interface ITypicalCounterInputSettings: ICounterInputSlopeSetting
    {
       

        /// <summary>
        /// Установить аттенюатор 1:1.
        /// </summary>
        public void SetAtt_1();
        /// <summary>
        /// Установить аттенюатор 1:10.
        /// </summary>
        public void SetAtt_10();

        public string GetAtt();

        /// <summary>
        ///Установить максимальный входной импеданс. 
        /// </summary>
        public void SetHightImpedance();
        /// <summary>
        /// Установить минимальный входной импеданс. 
        /// </summary>
        public void SetLowImpedance();

        public string GetImpedance();

        /// <summary>
        /// Связь по переменному току.
        /// </summary>
        public void SetCoupleAC();
        /// <summary>
        /// Связь по постоянному току.
        /// </summary>
        public void SetCoupleDC();

        public string GetCouple();


    }

    

}
