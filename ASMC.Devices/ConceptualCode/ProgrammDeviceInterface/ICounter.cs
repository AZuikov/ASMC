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
    }

    public interface ICounterInput
    {
        public string NameOfChanel { get; }
        ITypicalCounterInputSettings InputSetting { get; }
        /// <summary>
        /// Измерение частоты.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> Frequency { get; }
        /// <summary>
        /// Измерение несущей частоты пакета.
        /// </summary>
        IMeterPhysicalQuantity<Frequency> FrequencyBURSt { get; }
        /// <summary>
        /// Количество циклов в пакете.
        /// </summary>
        IMeterPhysicalQuantity<NoUnits> NumberOfCyclesInBurst { get; }
        /// <summary>
        /// Измерение мощности сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Power> PowerAC_Signal { get; }
        /// <summary>
        /// Частота следования импульсов в пакете.
        /// </summary>
        IMeterPhysicalQuantity<Percent> PulseRepetitionFrequencyBurstSignal { get; }
        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        IMeterPhysicalQuantity<Percent> PositiveDutyFactor { get; }
        /// <summary>
        /// Коэффициент заполнения.
        /// </summary>
        IMeterPhysicalQuantity<Percent> NegativeDutyFactor { get; }
        /// <summary>
        /// Отношение частот каналов.
        /// </summary>
        IMeterPhysicalQuantity<NoUnits> FrequencyRatio { get; }
        /// <summary>
        /// Максимальное значение переменного сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Voltage> Maximum{ get; }
        /// <summary>
        /// Минимальное значение переменного сигнала.
        /// </summary>
        IMeterPhysicalQuantity<Voltage> Minimum{ get; }
        /// <summary>
        /// Измерение от пика до пика.
        /// </summary>
        IMeterPhysicalQuantity<Voltage> PeakToPeak{ get; }
        IMeterPhysicalQuantity<Voltage> Ratio { get; }
        IMeterPhysicalQuantity<Time> Period{ get; }
        IMeterPhysicalQuantity<Time> PeriodAver{ get; }
        IMeterPhysicalQuantity<Time> TimeInterval{ get; }
        IMeterPhysicalQuantity<Time> PositivePulseWidth { get; }
        IMeterPhysicalQuantity<Time> NegativePulseWidth { get; }
        IMeterPhysicalQuantity<Degreas> Phase{ get; }
        

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

        /// <summary>
        ///Установить максимальный входной импеданс. 
        /// </summary>
        public void SetHightImpedance();
        /// <summary>
        /// Установить минимальный входной импеданс. 
        /// </summary>
        public void SetLowImpedance();

        /// <summary>
        /// Связь по переменному току.
        /// </summary>
        public void SetCoupleAC();
        /// <summary>
        /// Связь по постоянному току.
        /// </summary>
        public void SetCoupleDC();

       
    }

    

}
