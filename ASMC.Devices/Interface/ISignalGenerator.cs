﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.Interface.SourceAndMeter;

namespace ASMC.Devices.Interface
{
    public interface ISignalStandartParametr<TPhysicalQuantity, TPhysicalQuantity2> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() 
                                                                                    where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        /// <summary>
        /// Амплитуда и частота сигнала.
        /// </summary>
        public MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> AmplitudeAndFrequency { get; set; }
        /// <summary>
        /// Смещение сигнала.
        /// </summary>
        public MeasPoint<TPhysicalQuantity> SignalOffset { get; set; }
        /// <summary>
        /// Здержка сигнала (работает не для всех видов сигналов).
        /// </summary>
        public MeasPoint<Time> Delay { get; set; }

        /// <summary>
        /// Полярность сигнала.
        /// </summary>
        public bool IsPositivePolarity { get; set; }

        public string SignalFormName { get; }

    }

    /// <summary>
    /// Единичный импульс.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    /// <typeparam name="TPhysicalQuantity2"></typeparam>
    public interface IImpulseSignal <TPhysicalQuantity, TPhysicalQuantity2> : 
        ISignalStandartParametr<TPhysicalQuantity, TPhysicalQuantity2> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() 
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
        ISignalStandartParametr<TPhysicalQuantity, TPhysicalQuantity2>
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
        ISignalStandartParametr<TPhysicalQuantity, TPhysicalQuantity2>
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
     public interface IOutputSignalGenerator<TPhysicalQuantity, TPhysicalQuantity2>: 
        ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> 
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        /// <summary>
        /// Активная в данный момент форма сигнала.
        /// </summary>
        public ISignalStandartParametr<TPhysicalQuantity, TPhysicalQuantity2> ActiveSignalForm { get; set; }
        public string NameOfOutput { get; set; }

    }

    public interface ISignalGenerator<TPhysicalQuantity, TPhysicalQuantity2> 
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
        where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
     public   IOutputSignalGenerator<TPhysicalQuantity, TPhysicalQuantity2>[] outputs { get;   }
    }
}
