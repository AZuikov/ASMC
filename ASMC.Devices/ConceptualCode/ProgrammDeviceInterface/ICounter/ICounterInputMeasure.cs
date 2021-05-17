using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    /// <summary>
    /// Измерения, для которых нужны сразу два канала.
    /// </summary>
    public interface ICounterInputDualChanelMeasure 
    {
        /// <summary>
        /// Отношение частот каналов.
        /// </summary>
        MeasPoint<NoUnits> MeasFrequencyRatio(ICounterInput input, ICounterInput input2);

        ///// <summary>
        ///// Отношение напряжений.
        ///// </summary>
        //MeasPoint<Voltage> MeasRatioAB (ICounterInput input, ICounterInput input2);



        /// <summary>
        /// Измерение фазы.
        /// </summary>
        MeasPoint<Degreas> MeasPhase(ICounterInput input, ICounterInput input2);

        /// <summary>
        /// Измерение временных интервалов (задержек) между каналами.
        /// </summary>
        MeasPoint<Time> MeasTimeInterval(ICounterInput input, ICounterInput input2);
        
        
    }

    /// <summary>
    /// Типовые измерительные операции для каналов A и B частотомера.
    /// </summary>
    public interface ICounterSingleChanelMeasure
    {
        ///// <summary>
        ///// Коэффициент заполнения.
        ///// </summary>
        //IMeterPhysicalQuantity<Percent> MeasPositiveDutyCycle { get; set; }

        ///// <summary>
        ///// Коэффициент заполнения.
        ///// </summary>
        //IMeterPhysicalQuantity<Percent> MeasNegativeDutyCycle { get; set; }

        ///// <summary>
        ///// Максимальное значение переменного сигнала.
        ///// </summary>
        //IMeterPhysicalQuantity<Voltage> MeasMaximum { get; set; }

        ///// <summary>
        ///// Минимальное значение переменного сигнала.
        ///// </summary>
        //IMeterPhysicalQuantity<Voltage> MeasMinimum { get; set; }

        ///// <summary>
        ///// Измерение от пика до пика.
        ///// </summary>
        //IMeterPhysicalQuantity<Voltage> MeasPeakToPeak { get; set; }

        //IMeterPhysicalQuantity<Time> MeasPositivePulseWidth { get; set; }
        //IMeterPhysicalQuantity<Time> MeasNegativePulseWidth { get; set; }

        /// <summary>
        /// Измерение частоты.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> MeasFrequency { get; set; }

        ///// <summary>
        ///// Измерение несущей частоты пакета.
        ///// </summary>
        //IMeterPhysicalQuantity<Frequency> MeasFrequencyBURSt { get; set; }

        ///// <summary>
        ///// Частота следования импульсов в пакете.
        ///// </summary>
        //IMeterPhysicalQuantity<Frequency> MeasPulseRepetitionFrequencyBurstSignal { get; set; }
        
        ///// <summary>
        ///// Количество циклов в пакете.
        ///// </summary>
        //IMeterPhysicalQuantity<NoUnits> MeasNumberOfCyclesInBurst { get; set; }

        IMeterPhysicalQuantity<Time> MeasPeriod { get; set; }
        //IMeterPhysicalQuantity<Time> MeasPeriodAver { get; set; }

     
    }

   

    public interface ICounterInputPowerMeasure
    {
        /// <summary>
        /// Измерение мощности сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Power> MeasPowerAC_Signal { get; set; }
    }
}