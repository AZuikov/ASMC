﻿using System.Linq;
using ASMC.Data.Model;
using ASMC.Data.Model.PhysicalQuantity;
using ASMC.Devices.IEEE.Fluke.Calibrator;
using NLog.LayoutRenderers.Wrappers;

namespace ASMC.Devices.Interface
{
    public interface ICalibratorOld
    {
    void SetVoltageDc(MeasPoint<Voltage> setPoint);

    void SetVoltageAc(MeasPoint<Voltage, Frequency> setPoint);

    void SetResistance2W(MeasPoint<Resistance> setPoint);
    void SetResistance4W(MeasPoint<Resistance> setPoint);

    void SetCurrentDc(MeasPoint<Current> setPoint);

    void SetCurrentAc(MeasPoint<Current, Frequency> setPoint);

    void SetTemperature(MeasPoint<Temperature> setPoint,
        CalibrMain.COut.CSet.СTemperature.TypeTermocouple typeTermocouple, string unitDegreas);

    void SetOutputOn();

    void SetOutputOff();

    void Reset();
    }

    public interface Calibrator
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IRangePhysicalQuantity<TPhysicalQuantity> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
         RangeStorage<PhysicalRange<TPhysicalQuantity>> Ranges { get; }
         /// <summary>
         /// Выбранный предел измерения.
         /// </summary>
         PhysicalRange<TPhysicalQuantity> SelectRange { get; }

         /// <summary>
         /// Установить диапазон физ. величины.
         /// </summary>
         /// <param name="inRange">Предел, который нужно установить.</param>
         void SetRange(PhysicalRange<TPhysicalQuantity> inRange);
        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Величина, для которой нужно подобрать и установить диапазон.</param>
        void SetRange(MeasPoint<TPhysicalQuantity> inRange);

         /// <summary>
         /// Автоматический выбор предела.
         /// </summary>
         bool IsAutoRange { get; set; }
    }

    public abstract class RangeBaseDevice<TPhysicalQuantity> : IRangePhysicalQuantity<TPhysicalQuantity> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        protected RangeBaseDevice()
        {
            Ranges = new RangeStorage<PhysicalRange<TPhysicalQuantity>>();
        }
        /// <inheritdoc />
        public RangeStorage<PhysicalRange<TPhysicalQuantity>> Ranges { get;  set; }

        /// <inheritdoc />
        public PhysicalRange<TPhysicalQuantity> SelectRange { get; private set; }

        /// <inheritdoc />
        public void SetRange(PhysicalRange<TPhysicalQuantity> inRange)
        {
            SelectRange = Ranges.Ranges.FirstOrDefault(q=>q.Start== inRange.Start&& q.End == inRange.End);
        }

        /// <inheritdoc />
        public void SetRange(MeasPoint<TPhysicalQuantity> inRange)
        {
            SelectRange = Ranges.GetRangePointBelong(inRange);
        }

        /// <inheritdoc />
        public bool IsAutoRange { get; set; }
    }

    public interface IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new()
    {
        RangeStorage<PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2>> Ranges { get; }
        /// <summary>
        /// Выбранный предел измерения.
        /// </summary>
        PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> SelectRange { get; }

        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Предел, который нужно установить.</param>
        void SetRange(PhysicalRange<TPhysicalQuantity, TPhysicalQuantity2> inRange);
        /// <summary>
        /// Установить диапазон физ. величины.
        /// </summary>
        /// <param name="inRange">Величина, для которой нужно подобрать и установить диапазон.</param>
        void SetRange(MeasPoint<TPhysicalQuantity, TPhysicalQuantity2> inRange);

        /// <summary>
        /// Автоматический выбор предела.
        /// </summary>
        bool AutoRange { get; set; }
    }

    public interface ISourcePhysicalQuantityBase<T> : IDevice
    {
        /// <summary>
        /// Установить значение величины.
        /// </summary>
        void SetValue(T value);

        bool IsEnableOutput { get; }
        void OutputOn();
        void OutputOff();


    }
    /// <summary>
    /// Интерфейс источника одной физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity">Физическая величина.</typeparam>
    public interface ISourcePhysicalQuantity<TPhysicalQuantity> : ISourcePhysicalQuantityBase<MeasPoint<TPhysicalQuantity>> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()  
    {
       public IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; set; }

       

    }

    /// <summary>
    /// Интерфейс источника составной физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity">Физическая величина.</typeparam>
    /// /// <typeparam name="TPhysicalQuantity2">Физическая величина.</typeparam>
    public interface ISourcePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> : ISourcePhysicalQuantityBase<MeasPoint<TPhysicalQuantity, TPhysicalQuantity2>> where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new() where TPhysicalQuantity2 : class, IPhysicalQuantity<TPhysicalQuantity2>, new() 
    {
        IRangePhysicalQuantity<TPhysicalQuantity, TPhysicalQuantity2> RangeStorage { get; set; }
    }



    /// <summary>
    /// Измерение физической величины.
    /// </summary>
    /// <typeparam name="TPhysicalQuantity"></typeparam>
    public interface IMeterPhysicalQuantity<TPhysicalQuantity> : IDevice
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        IRangePhysicalQuantity<TPhysicalQuantity> RangeStorage { get; set; }
        public MeasPoint<TPhysicalQuantity> GetValue();
        public MeasPoint<TPhysicalQuantity> Value { get; }
    }
}