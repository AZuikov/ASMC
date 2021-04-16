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

    public interface ITypicalCounterInputMeasure<TPhysicalQuantity>
        where TPhysicalQuantity : class, IPhysicalQuantity<TPhysicalQuantity>, new()
    {
        public void
    }

}
