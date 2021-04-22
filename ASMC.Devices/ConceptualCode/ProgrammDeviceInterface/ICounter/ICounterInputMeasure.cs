﻿using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Измерения, для котрых нужны сразу два канала.
    /// </summary>
    public interface ICounterInputDualChanelMeasure 
    {
        /// <summary>
        /// Отношение частот каналов.
        /// </summary>
        IMeterPhysicalQuantity<NoUnits> MeasFrequencyRatio { get; set; }

        IMeterPhysicalQuantity<Voltage> MeasRatio { get; set; }
        IMeterPhysicalQuantity<Degreas> MeasPhase { get; set; }
        IMeterPhysicalQuantity<Time> MeasTimeInterval { get; set; }
    }

    /// <summary>
    /// Типовые измерительные операции для каналов A и B частотомера.
    /// </summary>
    public interface ICounterSingleChanelMeasureAorB 
    {
        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        IMeterPhysicalQuantity<Percent> MeasPositiveDutyCycle { get; set; }

        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        IMeterPhysicalQuantity<Percent> MeasNegativeDutyCycle { get; set; }

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

        IMeterPhysicalQuantity<Time> MeasPositivePulseWidth { get; set; }
        IMeterPhysicalQuantity<Time> MeasNegativePulseWidth { get; set; }
    }

    /// <summary>
    /// Типовые измерительные операции для каналов частотомера A, B, C
    /// </summary>
    public interface ICounterSingleChanelMeasureABC 
    {
        /// <summary>
        /// Измерение частоты.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }

        /// <summary>
        /// Измерение несущей частоты пакета.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> MeasFrequencyBURSt { get; set; }

        /// <summary>
        /// Частота следования импульсов в пакете.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> MeasPulseRepetitionFrequencyBurstSignal { get; set; }
        IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }
        IMeterPhysicalQuantity<Time> MeasPeriodAver { get; set; }

        /// <summary>
        /// Количество циклов в пакете.
        /// </summary>
        IMeterPhysicalQuantity<NoUnits> MeasNumberOfCyclesInBurst { get; set; }
    }


    public interface ICounterInputPowerMeasure 
    {
        /// <summary>
        /// Измерение мощности сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Power> MeasPowerAC_Signal { get; set; }
    }
}